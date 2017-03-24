using System;
using NUnit.Framework;
using CloudstrypeArray.Lib;

namespace Tests
{
	[TestFixture ()]
	public class TestUtil
	{
		[Test ()]
		public void TestParseFileSize()
		{
			Assert.AreEqual (100, Util.ParseFileSize ("100"));
			Assert.AreEqual (100, Util.ParseFileSize ("100 bytes"));
			Assert.AreEqual (100, Util.ParseFileSize ("100bytes"));
			Assert.AreEqual (100, Util.ParseFileSize ("100B"));
			Assert.AreEqual (10240, Util.ParseFileSize ("10KB"));
			Assert.AreEqual (10485760, Util.ParseFileSize ("10MB"));
			Assert.AreEqual (1073741824, Util.ParseFileSize ("1GB"));
			Assert.AreEqual (4831838208, Util.ParseFileSize ("4.5GB"));
		}
	}
}

