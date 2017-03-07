using System;
using System.IO;

namespace CloudstrypeArray.Lib.Storage
{
	public class Storage
	{
		// Implements a key/value store that is backed by a memory-mapped
		// file.

		public string Path;
		public long TotalSize;

		protected StorageIndex Index;
		protected StorageFile[] Files;

		// Point to last file with free space. Searching increments this
		// value until free space is found.
		protected int FreeFile = 0;

		public Storage(string path, long totalSize)
		{
			Path = path;
			TotalSize = totalSize;
			this.Open();
		}

		public void Open()
		{
			// Open StorageIndex.
			Index = new StorageIndex(path, totalSize);
			// Calculate number of StorageFiles needed to store TotalSize.
			int fileCount = TotalSize / StorageFile.MaxSize;
			Files = new StorageFile[fileCount];
			for (int i = 0; i < fileCount; i++) {
				string name = string.Format("array-{0}.db", i);
				string path = Path.Combine(Path, name);
				Files [i] = new StorageFile(path);
			}
			// Open that many files.
		}

		public void Write(string id, byte[] data)
		{
			// Search for free space in `Files`.
			// Assert free space is available.
			// Write block.
			// Add id to Index.
		}

		public byte[] Read(string id)
		{
			// Read file, offset tuple from `Index`.
			// Read and return data from referenced `StorageFile`.
		}

		public void Delete(string id)
		{
			// Read file, offset tuple from `Index`.
			// Delete from Index.
			// Delete from referenced `StorageFile`.
		}
	}
}
