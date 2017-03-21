using System;
using System.IO;
using NUnit.Framework;
using CloudstrypeArray.Lib.Storage;

namespace Tests
{
	[TestFixture ()]
	public class TestStorage
	{
		Storage Store;

		[SetUp]
		public void Init()
		{
			Store = new Storage ();
		}

		[TearDown]
		public void Cleanup()
		{
			string[] list = Store.List ();
			for (int i = 0; i < list.Length; i++) {
				Store.Delete (list [i]);
			}
		}

		[Test ()]
		public void TestWriteDelete ()
		{
			byte[] data = new byte[] { 65, 65, 65, 10, 13 };
			Store.Write ("1234567890abcdef", data);
			Assert.AreEqual (data.Length, Store.UsedSize);
			Store.Delete ("1234567890abcdef");
			Assert.AreEqual (0, Store.UsedSize);
		}

		[Test ()]
		public void TestWriteRead ()
		{
			byte[] data = new byte[] { 65, 65, 65, 10, 13 };
			Store.Write ("1234567890abcdef", data);
			Assert.AreEqual (data.Length, Store.UsedSize);
			byte[] read = Store.Read ("1234567890abcdef");
			Assert.AreEqual (data.Length, read.Length);
			for (int i = 0; i < data.Length; i++) {
				Assert.AreEqual (data [i], read [i]);
			}
		}
	}
}
