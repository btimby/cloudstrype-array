using System;
using System.IO;

namespace CloudstrypeArray.Lib.Storage
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

		protected FileStream Log;

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

		public void Iterate()
		{
			// Not sure how to implement Iterable, but we will want that
			// to be able to Flush();
		}

		public void Add(string id, BlockLocation block)
		{
			// Write to log.
			// Insert into memory (re-sort?).
			// https://github.com/dshulepov/DataStructures/blob/master/DataStructures/SkipList.cs
			// https://msdn.microsoft.com/en-us/library/ms379573(v=vs.80).aspx
			// Use a skip list to facilitate sorted insertion and binary search.
		}

		public BlockLocation Find(string id)
		{
			// Use binary search via skip list to locate
			// id.
			// Return id's location.
		}

		public void Delete(string id)
		{
			// Write to log.
			// Remove id from list (re-sort?).
		}
	}
}
