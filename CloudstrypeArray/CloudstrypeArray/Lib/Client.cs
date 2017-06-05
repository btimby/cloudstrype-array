using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using log4net;

namespace CloudstrypeArray.Lib.Network
{
	public enum CommandType : byte
	{
		Get = 0,
		Put = 1,
		Delete = 2,
		Ping = 3,
		Stat = 4
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
		public Guid Name;

		protected Socket _socket;
		protected Stream _stream;
		protected Uri Url;

		public Client(string url, Guid name)
		{
			Url = new Uri (url);
			Name = name;
		}

		public void Connect()
		{
			Logger.DebugFormat ("Opening connection to {0}", Url);
			_socket = new Socket (SocketType.Stream, ProtocolType.IP);
			// Set a long timeout, we just wait for commands. The server will send
			// a keepalive every 30s.
			_socket.ReceiveTimeout = 90000;
			_socket.Connect (Url.Host, Url.Port);
			if (Url.Scheme.ToLower () == "ssl") {
				_stream = new SslStream (new NetworkStream (_socket), false);
				((SslStream)_stream).AuthenticateAsClient (
					Url.Host, null, SslProtocols.Tls12, false);
			}
			else
			{
				_stream = new NetworkStream (_socket);
			}
			Logger.DebugFormat ("Connected, sending name {0}", Name);
			byte[] name = Name.ToByteArray();
			Util.FixGuid (ref name);
			_stream.Write(name, 0, name.Length);
		}

		public void Close()
		{
			_stream.Close ();
			_socket.Close ();
			_stream.Dispose ();
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
			_stream.Read(data, 0, data.Length);

			Command cmd = Command.ParseHeader (data);
			if (cmd.Length > 0) {
				data = new byte[cmd.Length];
				Logger.DebugFormat ("Reading {0} bytes of payload", cmd.Length);
				for (int bytesRead = 0; bytesRead < data.Length;) {
					bytesRead += _stream.Read (data, bytesRead, data.Length - bytesRead);
					Logger.DebugFormat ("Received {0} bytes", bytesRead);
				}

				// Find the length of the ID string.
				int nullPos = Array.IndexOf<byte> (data, 0, cmd.IDLength);
				nullPos = nullPos > -1 ? nullPos : cmd.IDLength;
				nullPos = Math.Min (nullPos, cmd.IDLength);
				cmd.ID = Encoding.ASCII.GetString (data, 0, nullPos );

				// Copy the data buffer
				cmd.Data = new byte[cmd.DataLength];
				Array.Copy (data, cmd.IDLength, cmd.Data, 0, cmd.DataLength);
			}
			Logger.DebugFormat (
				"Recv({0}): {1}({2}), {3}, id {4} bytes, payload {5} bytes",
				Name, cmd.Type.ToString(), cmd.ID, cmd.Status.ToString(),
				cmd.IDLength, cmd.DataLength);
			return cmd;
		}

		public void Send(Command cmd)
		{
			byte[] data = cmd.ToByteArray ();
			Logger.DebugFormat ("Sending {0} bytes", data.Length);
			Logger.DebugFormat (
				"Send({0}): {1}({2}), {3}, id {4} bytes, payload {5} bytes",
				Name, cmd.Type.ToString(), cmd.ID, cmd.Status.ToString(),
				cmd.IDLength, cmd.DataLength);
			_stream.Write (data, 0, data.Length);
		}
	}
}
