using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace CloudstrypeArray.Lib.Storage
{
	[Flags]
	internal enum BlockStatus : byte {
		Free = 0,
		Used = 1,
	}

	internal class BlockHeader
	{
		BlockStatus status;
		Int16 size;
	}

	public class BlockException : Exception
	{
		public int Block;

		public BlockException(string msg, params object[] args) : base(string.Format(msg, args))
		{
			Block = (Int32)args [0];
		}
	}

	public class StorageFile
	{
		// Implements a datastore.

		// This class stores individual blocks in a file. Multiple files
		// with an index make up a key/store database. A single file is
		// capable of reading, writing and deleting blocks of data. It
		// is responsible for keeping track of what blocks exist and 
		// performing validation of arguments (don't delete a missing
		// block, don't overwrite a block) before supported operations.

		// This class manages a small offset table at the beginning of
		// the file. Each table entry represets a fixed size block within
		// the file. As this is a memory-mapped file, this table is used
		// as an array, empty slots represent unused blocks, while filled
		// slots are used. Each filled slot contains a header describing
		// the block. Blocks are immutable and cannot be overwritten.

		public const int BlockSize = 1024 * 32;
		public const int MaxBlocks = 320;
		// Total file size is block size * block count (data portion)
		// plus block count (headers, one byte each).
		public const int DataSize = (MaxBlocks * BlockSize);
		public const int BlockTableSize = (MaxBlocks * sizeof(BlockHeader));
		public const int FileSize = DataSize + BlockTableSize;

		public string Path;

		protected MemoryMappedFile _file;
		protected MemoryMappedViewAccessor _view;

		// This points to the slot after the last known free slot.
		// Searching increments this until free space is found in the
		// preceding block (by checking header for 0). When appending
		// to a file, this will increment block by block.  Delete
		// sets this value to the recently vacated block.
		private Int32 Index = 0;
		private object _lock;

		public StorageFile(string path)
		{
			Path = path;
		}

		protected void Open() {
			_file = MemoryMappedFile.CreateFromFile (
				Path,
				FileMode.OpenOrCreate,
				System.IO.Path.GetFileNameWithoutExtension (Path),
				MaxSize,
				MemoryMappedFileAccess.ReadWrite);
			_view = _file.CreateViewAccessor ();
		}

		private BlockHeader ReadHeader(int block)
		{
			Int32 pos = block * sizeof(BlockHeader);
			BlockHeader header = new BlockHeader ();
			header.status = (BlockStatus)_view.ReadByte (pos++);
			header.size = _view.ReadInt16 (pos);
			return header;
		}

		private void WriteHeader(int block, BlockHeader header)
		{
			Int32 pos = block * sizeof(BlockHeader);
			_view.Write (pos++, header.status);
			_view.Write (pos, header.size);
		}

		public void Write(int block, byte[] data)
		{
			// Assert data length is equal to block size (file size is divisible by this).
			if (data.Length > BlockSize)
				throw new BlockException ("Block {0} too large at {1} bytes", block, data.Length);
			BlockHeader header = ReadHeader(block);
			// Assert requested block is unused.
			if (!header.status.HasFlag(BlockStatus.Free))
				throw new BlockException ("Block {0} in use", block);
			// Write block header
			header.status = BlockStatus.Used;
			header.size = data.Length;
			WriteHeader(block, header);
			// Write block data.
			Int32 offset = BlockTableSize + (block * BlockSize);
			_view.WriteArray (offset, data);
			_view.Flush ();
		}

		public void Write(byte[] data)
		{
			Write (Index, data);
			return Index++;
		}

		public byte[] Read(int block)
		{
			BlockHeader header = ReadHeader(block);
			// Assert block is used.
			if (header.status != BlockStatus.Used)
				throw new BlockException ("Block {0} unused", block);
			// Return block data.
			Int32 offset = BlockTableSize + (block * BlockSize);
			byte[] data = new byte[header.size];
			_view.ReadArray (offset, data, 0, data.Length);
			return data;
		}

		public void Delete(int block)
		{
			BlockHeader header = ReadHeader(block);
			// Assert block is used.
			if (header.status != BlockStatus.Used)
				throw new BlockException ("Block {0} unused", block);
			header.status = BlockStatus.Free;
			header.size = 0;
			// Update block header.
			WriteHeader(block, header);
			_view.Flush ();
		}
	}
}
