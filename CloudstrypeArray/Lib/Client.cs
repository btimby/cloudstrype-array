using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.WebSockets;

namespace CloudstrypeArray.Lib.Network
{
	public enum CommandType : byte
	{
		Get = 0,
		Put = 1,
		Delete = 2
	}

	public class Command
	{
		public CommandType Type;
		public string ID;
		public int Length = 0;
		public byte[] Data = null;

		public Command(CommandType type, string id)
		{
			Type = type;
			ID = id;
		}

		public byte[] ToBytes()
		{
			byte[] data = new byte[29 + Length];
			data [0] = (byte)Type;
			Array.Copy (Encoding.ASCII.GetBytes (ID), 0, data, 1, 24);
			byte[] len = BitConverter.GetBytes (Length);
			Array.Copy(len, 0, data, 25, len.Length); 
			Array.Copy(Data, 0, data, 29, Data.Length); 
			return data;
		}

		public static Command Parse(byte[] data)
		{
			// Parse the fix-sized portion of the command.
			Command cmd = new Command (
				(CommandType)data[0],
				Encoding.ASCII.GetString(data, 1, 24));
			cmd.Length = BitConverter.ToInt32 (data, 25);
			return cmd;
		}
	}

	public class Client
	{
		// Implements a network client.
		protected ClientWebSocket _ws;
		protected Guid ID;
		protected Uri Url;

		public Client(string url, Guid id)
		{
			Url = new Uri (url);
			ID = id;
			_ws = new ClientWebSocket ();
		}

		public void Connect()
		{
			Task t;
			t = _ws.ConnectAsync (Url, CancellationToken.None);
			//t.RunSynchronously ();
			t.Wait ();
			// Send our name to server.
			t = _ws.SendAsync (
				new ArraySegment<byte>(ID.ToByteArray()),
				WebSocketMessageType.Binary,
				true,
				CancellationToken.None);
			t.Wait ();
		}

		public void Close()
		{
			Task t = _ws.CloseAsync (
				WebSocketCloseStatus.NormalClosure,
				"",
				CancellationToken.None);
			t.Wait ();
		}

		public Command Receive()
		{
			Task t;
			byte[] data = new byte[29];
			t = _ws.ReceiveAsync (
				new ArraySegment<byte> (data), CancellationToken.None);
			t.Wait ();
			Command cmd = Command.Parse (data);
			if (cmd.Length > 0)
			{
				cmd.Data = new byte[cmd.Length];
				t = _ws.ReceiveAsync (
					new ArraySegment<byte> (cmd.Data),
					CancellationToken.None);
				t.Wait ();
			}
			return cmd;
		}

		public void Send(Command cmd)
		{
			byte[] data = cmd.ToBytes ();
			Task t = _ws.SendAsync (
				new ArraySegment<byte> (data),
				WebSocketMessageType.Text,
				true, CancellationToken.None);
			t.Wait ();
		}
	}
}
