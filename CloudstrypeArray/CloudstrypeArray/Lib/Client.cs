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

		public byte[] ToByteArray()
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

		public static Command Parse(byte[] data)
		{
			if (data.Length != 30)
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
			_socket = new Socket (SocketType.Stream, ProtocolType.IP);
		}

		public void Connect()
		{
			byte[] name = ID.ToByteArray();
			_socket.Connect (Url.Host, Url.Port);
			Debug.Assert(_socket.Send (name) == name.Length);
		}

		public void Close()
		{
			_socket.Close ();
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
			byte[] data = new byte[30];
			Debug.Assert(_socket.Receive (data) == data.Length);
			Command cmd = Command.Parse (data);
			if (cmd.Length > 0) {
				cmd.Data = new byte[cmd.Length];
				Debug.Assert(_socket.Receive (cmd.Data) == cmd.Length);
			}
			return cmd;
		}

		public void Send(Command cmd)
		{
			byte[] data = cmd.ToByteArray ();
			Debug.Assert(_socket.Send (data) == data.Length);
		}
	}
}
