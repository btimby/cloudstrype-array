using NUnit.Framework;
using System;

using CloudStrypeArray.Lib;

namespace Tests
{
	[TestFixture ()]
	public class TestStorageFile
	{
		[Test ()]
		public void TestCase ()
		{
			StorageFile file = new StorageFile ("");
			file.Write ();
		}
	}
}

