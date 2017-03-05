using System;

namespace CloudstrypeArray.Lib
{
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

		public string Path;
		public long TotalSize;

		// This points to the slot after the last known free slot.
		// Searching increments this until free space is found in the
		// preceding block (by checking header for 0). When appending
		// to a file, this will increment block by block.  Delete
		// sets this value to the recently vacated block.
		protected int FreeBlock = 0;

		public StorageFile(string path, long totalSize)
		{
			Path = path;
			TotalSize = TotalSize;
		}

		public void Write(int block, byte[] data)
		{
			// Assert data length is equal to block size (file size is divisible by this).
			// Assert requested block is unused.
			// Create block header
			// Write block data & header.
		}

		public int Append(byte[] data)
		{
			// Assert data length is equal to block size (file size is divisible by this).
			// Search for free space.
			// Assert free space is available.
			// Write data to free space.
			// Increment FreeBlock.
			// Return block id.
		}

		public byte[] Read(int block)
		{
			// Assert block is used.
			// Return block data.
		}

		public void Delete(int block)
		{
			// Assert block is used.
			// Update block header to 0.
			// Set FreeBlock to `block`.
		}
	}
}
