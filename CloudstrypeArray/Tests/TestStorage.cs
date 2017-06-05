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
			Store.Clear ();
		}

		[Test ()]
		public void TestWriteDelete ()
		{
			byte[] data = new byte[] { 65, 65, 65, 10, 13 };
			Store.Write ("1234567890abcdef", data);
			Assert.AreEqual (data.Length, Store.Used);
			Store.Delete ("1234567890abcdef");
			Assert.AreEqual (0, Store.Used);
		}

		[Test ()]
		public void TestWriteRead ()
		{
			byte[] data = new byte[] { 65, 65, 65, 10, 13 };
			Store.Write ("1234567890abcdef", data);
			Assert.AreEqual (data.Length, Store.Used);
			byte[] read = Store.Read ("1234567890abcdef");
			Assert.AreEqual (data.Length, read.Length);
			for (int i = 0; i < data.Length; i++) {
				Assert.AreEqual (data [i], read [i]);
			}
		}
	}
}
