using System;
using NUnit.Framework;
using CloudstrypeArray.Lib.Network;

namespace Tests
{
	[TestFixture]
	public class TestNetwork
	{
		Client Client;

		[SetUp]
		public void Init()
		{
			Client = new Client ("ws://localhost:8765/", Guid.Parse ("40dd1e40-544d-db46-9f67-df0cae847909"));
			Client.Connect ();
		}

		[TearDown]
		public void Cleanup()
		{
			Client.Close ();
		}
			
		[Test ()]
		public void TestClient ()
		{
			Client.Receive ();

			Command cmd = new Command (CommandType.Get, CommandStatus.None, "123412341234123412341234");
			cmd.Length = 4;
			cmd.Data = new byte[] { 65, 65, 65, 65 };
			Client.Send(cmd);
		}
	}
}
