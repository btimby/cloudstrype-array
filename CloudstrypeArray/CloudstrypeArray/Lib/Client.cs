using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Sockets;
using log4net;

namespace CloudstrypeArray.Lib.Network
{
	public enum CommandType : byte
	{
		Get = 0,
		Put = 1,
		Delete = 2,
		Ping = 3
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
		public int IDLength = 0;
		public int DataLength = 0;

		protected byte[] _data = null;
		protected string _id = null;

		private static readonly ILog Logger = LogManager.GetLogger(typeof(Command));

		public Command(CommandType type, CommandStatus status, int idLength, int dataLength)
		{
			Type = type;
			Status = status;
			IDLength = idLength;
			DataLength = dataLength;
		}

		public int Length{
			get {
				return IDLength + DataLength;
			}
		}

		public byte[] Data {
			get {
				return _data;
			}
			set {
				DataLength = (value == null) ? 0 : value.Length;
				_data = value;
			}
		}

		public string ID {
			get {
				return _id;
			}
			set {
				IDLength = (value == null) ? 0 : value.Length;
				_id = value;
			}
		}

		public byte[] ToByteArray()
		{
			byte[] data = new byte[10 + Length];
			// These two are simple.
			data [0] = (byte)Type;
			data [1] = (byte)Status;
			// Convert our ints to 4 byte arrays and pack them.
			byte[] idLength = BitConverter.GetBytes (IDLength);
			Array.Copy(idLength, 0, data, 2, idLength.Length);
			byte[] dataLength = BitConverter.GetBytes (DataLength);
			Array.Copy(dataLength, 0, data, 6, dataLength.Length);
			// Convert our variable length buffers and pack them.
			if (ID != null) {
				byte[] idBytes = Encoding.ASCII.GetBytes (ID);
				Debug.Assert (idBytes.Length == IDLength);
				Array.Copy (idBytes, 0, data, 10, idBytes.Length);
			}
			if (Data != null) {
				Debug.Assert (Data.Length == DataLength);
				Array.Copy (Data, 0, data, 10+IDLength, Data.Length); 
			}
			return data;
		}

		public static Command ParseHeader(byte[] data)
		{
			// Parse the fix-sized portion of the command.
			int idLength = BitConverter.ToInt32 (data, 2);
			int dataLength = BitConverter.ToInt32 (data, 6);
			Command cmd = new Command (
				(CommandType)data[0],
				(CommandStatus)data[1],
				idLength, dataLength);
			return cmd;
		}
	}

	public class Client
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(Client));

		// Implements a network client.
		protected Socket _socket;
		protected Guid ID;
		protected Uri Url;

		public Client(string url, Guid id)
		{
			Url = new Uri (url);
			ID = id;
		}

		public void Connect()
		{
			Logger.DebugFormat ("Opening connection to {0}", Url);
			byte[] name = ID.ToByteArray();
			_socket = new Socket (SocketType.Stream, ProtocolType.IP);
			_socket.Connect (Url.Host, Url.Port);
			Debug.Assert(_socket.Send (name) == name.Length);
		}

		public void Close()
		{
			_socket.Close ();
			_socket.Dispose ();
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
			byte[] data = new byte[10];
			Debug.Assert(_socket.Receive (data) == data.Length);
			Command cmd = Command.ParseHeader (data);
			if (cmd.Length > 0) {
				data = new byte[cmd.Length];
				Debug.Assert(_socket.Receive (data) == cmd.Length);

				// Find the length of the ID string.
				int nullPos = Array.IndexOf<byte> (data, 0, 0);
				nullPos = nullPos > -1 ? nullPos : cmd.IDLength;
				nullPos = Math.Min (nullPos, cmd.IDLength);
				cmd.ID = Encoding.ASCII.GetString (data, 0, nullPos );

				// Copy the data buffer.
				cmd.Data = new byte[cmd.DataLength];
				Array.Copy (data, cmd.IDLength, cmd.Data, 0, cmd.DataLength);
			}
			Logger.DebugFormat ("Received {0} bytes", cmd.Length + 10);
			Logger.DebugFormat (
				"Recv({0}): {1}({2}), {3}, id {4} bytes, payload {5} bytes",
				ID, cmd.Type.ToString(), cmd.ID, cmd.Status.ToString(),
				cmd.IDLength, cmd.DataLength);
			return cmd;
		}

		public void Send(Command cmd)
		{
			byte[] data = cmd.ToByteArray ();
			Logger.DebugFormat ("Sending {0} bytes", data.Length);
			Logger.DebugFormat (
				"Send({0}): {1}({2}), {3}, id {4} bytes, payload {5} bytes",
				ID, cmd.Type.ToString(), cmd.ID, cmd.Status.ToString(),
				cmd.IDLength, cmd.DataLength);
			Debug.Assert(_socket.Send (data) == data.Length);
		}
	}
}
