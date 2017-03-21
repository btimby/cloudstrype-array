using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net.WebSockets;
using log4net;

namespace CloudstrypeArray.Lib.Network
{
	public enum CommandType : byte
	{
		Get = 0,
		Put = 1,
		Delete = 2
	}

	public enum CommandStatus : byte
	{
		None = 0,
		Success = 1,
		Error = 2
	}

	public class InvalidCommandException : Exception
	{
		public InvalidCommandException(string msg) : base(msg) { }
	}

	public class Command
	{
		public CommandType Type;
		public CommandStatus Status = 0;
		public string ID;
		public int Length = 0;
		public byte[] _data = null;

		public Command(CommandType type, CommandStatus status, string id)
		{
			Type = type;
			Status = status;
			ID = id;
		}

		public byte[] Data {
			get {
				return _data;
			}
			set {
				Length = (value == null)?0:value.Length;
				_data = value;
			}
		}

		public byte[] ToBytes()
		{
			byte[] data = new byte[30 + Length];
			data [0] = (byte)Type;
			data [1] = (byte)Status;
			Array.Copy (Encoding.ASCII.GetBytes (ID), 0, data, 2, ID.Length);
			byte[] len = BitConverter.GetBytes (Length);
			Array.Copy(len, 0, data, 26, len.Length);
			if (Data != null) {
				Array.Copy (Data, 0, data, 30, Data.Length); 
			}
			return data;
		}

		public static Command Parse(byte[] data, int len)
		{
			if (len != 30)
				throw new InvalidCommandException ("Not enough data");
			// Name is UP TO 24 bytes in length. If we find a null byte
			// then the name is shorter than 24 bytes.
			int nullPos = Array.IndexOf<byte> (data, 0, 2) - 1;
			nullPos = Math.Min (nullPos, 24);
			// Parse the fix-sized portion of the command.
			Command cmd = new Command (
				(CommandType)data[0],
				(CommandStatus)data[1],
				Encoding.ASCII.GetString(data, 2, nullPos));
			cmd.Length = BitConverter.ToInt32 (data, 26);
			return cmd;
		}

		public static bool TryParse(byte[] data, int len, out Command cmd)
		{
			cmd = null;
			try
			{
				cmd = Parse(data, len);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}

	public class Client
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(Client));

		// Implements a network client.
		protected ClientWebSocket _ws;
		protected Guid ID;
		protected Uri Url;

		public Client(string url, Guid id)
		{
			Url = new Uri (url);
			ID = id;
			_ws = new ClientWebSocket ();
			_ws.Options.KeepAliveInterval = TimeSpan.FromSeconds (30);
		}

		public void Connect()
		{
			Task t;
			t = _ws.ConnectAsync (Url, CancellationToken.None);
			t.Wait ();
			// Send our name to server.
			byte[] name = ID.ToByteArray();
			Util.FixGuid (ref name);
			t = _ws.SendAsync (
				new ArraySegment<byte>(name),
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

		public void Reconnect()
		{
			try
			{
				Close();
			} catch (Exception e) {
				Logger.Error (e);
			}
			Connect ();
		}

		public Command Receive()
		{
			if (_ws.State != WebSocketState.Open)
				throw new WebSocketException ();
			Task<WebSocketReceiveResult> t;
			byte[] data = new byte[30];
			t = _ws.ReceiveAsync (
				new ArraySegment<byte> (data), CancellationToken.None);
			t.Wait ();
			Command cmd = Command.Parse (data, t.Result.Count);
			if (cmd.Length > 0)
			{
				cmd.Data = new byte[cmd.Length];
				t = _ws.ReceiveAsync (
					new ArraySegment<byte> (cmd.Data),
					CancellationToken.None);
				t.Wait ();
				Debug.Assert (t.Result.Count == cmd.Length);
			}
			return cmd;
		}

		public void Send(Command cmd)
		{
			if (_ws.State != WebSocketState.Open)
				throw new WebSocketException ();
			byte[] data = cmd.ToBytes ();
			Task t = _ws.SendAsync (
				new ArraySegment<byte> (data),
				WebSocketMessageType.Text,
				true, CancellationToken.None);
			t.Wait ();
		}
	}
}
