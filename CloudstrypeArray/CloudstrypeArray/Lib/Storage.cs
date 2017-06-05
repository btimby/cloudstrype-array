using System;
using System.IO;
using System.Collections.Generic;
using log4net;

namespace CloudstrypeArray.Lib.Storage
{
	public class StorageFullException : Exception
	{
		public StorageFullException(string msg) : base(msg) { }
	}

	public class Storage
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(Storage));

		// Implements a key/value store that is backed by a data directory
		// containing files.
		//
		// <path>/.cloudstrype/id[0:2]/id[2:4]/<id of chunk>

		public string Root;
		public long Size = -1;
		public long Used = 0;
		public Guid Name;

		public Storage(string path, long size, Guid name)
		{
			Size = size;
			if (!Directory.Exists (path)) {
				throw new IOException (string.Format("Path {0} does not exist", path));
			}
			path = Path.Combine(path, ".cloudstrype", name.ToString());
			Directory.CreateDirectory (path);
			Root = path;
			Used = GetUsed ();
		}

		public Storage() : this(Path.GetTempPath(), -1, Guid.NewGuid()) { }

		protected string[] List() {
			List<string> paths = new List<string> ();
			foreach (string dir0 in Directory.GetDirectories(Root)) {
				foreach (string dir1 in Directory.GetDirectories(Path.Combine (Root, dir0))) {
					paths.AddRange (Directory.GetFiles (Path.Combine(dir0, dir1)));
				}
			}
			return paths.ToArray ();
		}

		protected long GetUsed()
		{
			long size = 0;
			string[] files = List ();
			for (int i = 0; i < files.Length; i++) {
				size += new FileInfo (Path.Combine (Root, files[i])).Length;
			}
			Logger.DebugFormat ("Using {0} bytes in {1}", size, Root);
			return size;
		}

		protected string GetFullPath(string id) {
			return Path.Combine (Root, id.Substring(0, 2), id.Substring(2, 2), id);
		}

		public void Write(string id, byte[] data)
		{
			if (Size != -1 && Used + data.Length > Size)
				throw new StorageFullException ("Allocated space exhausted");
			string fullPath = GetFullPath (id);
			Logger.DebugFormat ("Writing to {0}", fullPath);
			Directory.CreateDirectory (Path.GetDirectoryName (fullPath));
			/* Allow overwriting. In theory we never overwrite, but in practice sometimes
			* our transactions fail and we recycle chunk IDs. Also during development and
			* testing this often happens. */
			using (FileStream file = File.OpenWrite (fullPath)) {
				// Account for used data by adding the net gain
				Used += data.Length - file.Length;
				file.Write (data, 0, data.Length);
			}
			Logger.DebugFormat ("Wrote {0} bytes to {1}", data.Length, fullPath);
		}

		public byte[] Read(string id)
		{
			string fullPath = GetFullPath (id);
			Logger.DebugFormat ("Reading from {0}", fullPath);
			using (FileStream file = File.OpenRead (fullPath)) {
				byte[] data = new byte[file.Length];
				int bytesRead = 0;
				while (bytesRead < data.Length) {
					bytesRead += file.Read (data, bytesRead, data.Length - bytesRead);
				}
				Logger.DebugFormat ("Read {0} bytes from {1}", bytesRead, fullPath);
				return data;
			}
		}

		public void Delete(string id)
		{
			string fullPath = GetFullPath (id);
			Logger.DebugFormat ("Deleting {0}", fullPath);
			Used -= new FileInfo (fullPath).Length;
			File.Delete (fullPath);
		}

		public void Clear()
		{
			foreach (string path in List()) {
				Delete (path);
			}
			Used = 0;
		}
	}
}
