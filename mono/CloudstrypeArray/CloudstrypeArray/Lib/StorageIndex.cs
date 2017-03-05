using System;
using System.IO;

namespace CloudstrypeArray.Lib
{
	public class BlockLocation
	{
		public int FileID;
		public int BlockID;
	}

	public class StorageIndex
	{
		// Implements an index over a set of memory-mapped files. Contains
		// A reference from key to (file, offset) tuples. The index is
		// sorted for lookups. A log is appended for writes, when opened
		// the log is inserted into the sorted index file. This can also
		// happen periodically during operation. In this way, write
		// performance is bought by increasing startup time. Periodical
		// flush can be configured to ensure consistency.

		public string Path;
		public long TotalSize;

		protected File Log;
		protected File Index;

		public StorageIndex(string path, long totalSize)
		{
			Path = path;
			TotalSize = totalSize;
		}

		protected void Open()
		{
			// Open log file.
			// Open index file and read into memory.
			// Merge log into index in memory.
			this.Merge();
			// Commit index from memory to index file.
			this.Flush();
		}

		public void Merge()
		{
			// Obtain lock.
			// Read log entries and write to memory.
		}

		public void Flush()
		{
			// TODO: Since the data file is only read when instantiated,
			// we don't need to block for this. User could choose
			// synchronous or asynchronous writes.

			// Write sorted index to temp file.
			// Replace current data file with temp file.
		}

		public BlockLocation Insert(string id)
		{
			// Write to log.
			// Insert into memory (re-sort).
		}

		public BlockLocation Locate(string id)
		{
			// Use something like binary search to locate
			// id, return it's location.
		}

		public void Delete(string id)
		{
			// Write to log.
			// Remove id from list (re-sort).
		}
	}
}
