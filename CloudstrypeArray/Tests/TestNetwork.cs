using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NUnit.Framework;
using CloudstrypeArray.Lib;
using CloudstrypeArray.Lib.Network;

namespace Tests
{
	public class TestServer
	{
		Socket _socket;
		public byte[] Name = new byte[16];
		public byte[] Data = new byte[38];

		public TestServer()
		{
			_socket = new Socket (SocketType.Stream, ProtocolType.IP);
			_socket.Bind (new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
		}

		public int Port
		{
			get {
				return ((IPEndPoint)_socket.LocalEndPoint).Port;
			}
		}

		public void Run(object arg)
		{
			_socket.Listen (1);
			Command cmd = new Command (CommandType.Ping, CommandStatus.None, 0, 0);
			Socket client = _socket.Accept ();
			client.Receive (Name);
			client.Send (cmd.ToByteArray ());
			client.Receive (Data);
		}

		public void Close()
		{
			_socket.Close ();
		}
	}

	[TestFixture]
	public class TestNetwork
	{
		TestServer Server;
		Client Client;
		Thread Thread;

		[SetUp]
		public void Init()
		{
			Server = new TestServer();
			Thread = new Thread (Server.Run);
			Thread.Start ();
			Client = new Client (
				string.Format("tcp://localhost:{0}/", Server.Port),
				Guid.Parse ("40dd1e40-544d-db46-9f67-df0cae847909"));
		}

		[TearDown]
		public void Cleanup()
		{
			Client.Close ();
			Thread.Abort ();
		}

		[Test ()]
		public void TestClient ()
		{
			Client.Connect ();
			Client.Receive ();

			Command cmd = new Command (CommandType.Get, CommandStatus.None, 24, 4);
			cmd.ID = "123412341234123412341234";
			cmd.Data = new byte[] { 65, 65, 65, 65 };
			Client.Send(cmd);

			byte[] name = Client.Name.ToByteArray ();
			Util.FixGuid(ref name);
			for (int i = 0; i < name.Length; i++)
			{
				Assert.AreEqual (name [i], Server.Name [i]);
			}

			byte[] data = cmd.ToByteArray ();
			for (int i = 0; i < data.Length; i++)
			{
				Assert.AreEqual (data [i], Server.Data [i]);
			}
		}
	}
}
