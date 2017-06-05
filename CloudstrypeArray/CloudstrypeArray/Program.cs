using System;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Configuration;
using CloudstrypeArray.Lib;
using CloudstrypeArray.Lib.Storage;
using CloudstrypeArray.Lib.Network;
using DocoptNet;
using log4net;

namespace CloudstrypeArray
{
	class Server
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(Server));

		protected object _lock = new object ();
		protected bool _running = true;
		protected Thread _thread;

		protected Client _client;
		protected Storage _store;

		public Server(string url, Guid name, string path, long size)
		{
			_client = new Client (url, name);
			_store = new Storage (path, size, name);
			_thread = new Thread (Run);
		}

		public bool Running
		{
			get {
				lock (_lock) {
					return _running;
				}
			}
			set {
				lock (_lock) {
					_running = value;
				}
			}
		}

		public void Run(object arg)
		{
			while (Running)
			{
				try
				{
					Command cmd = _client.Receive ();
					// Null return indicates a timeout, which allows us to
					// periodically check Running state.
					if (cmd == null)
						continue;
					try
					{
						switch (cmd.Type) {
						case CommandType.Get:
							cmd.Data = _store.Read (cmd.ID);
							break;
						case CommandType.Put:
							_store.Write (cmd.ID, cmd.Data);
							cmd.Data = null;
							break;
						case CommandType.Delete:
							_store.Delete (cmd.ID);
							break;
						case CommandType.Stat:
							byte[] data = new byte[16];
							byte[] size = BitConverter.GetBytes (_store.Size);
							Array.Copy(size, 0, data, 0, size.Length);
							byte[] used = BitConverter.GetBytes (_store.Used);
							Array.Copy(used, 0, data, 8, used.Length);
							cmd.Data = data;
							break;
						}
						cmd.Status = CommandStatus.Success;
					}
					catch (Exception e)
					{
						// Error executing the requested command, set the cmd up for
						// sending an error reply to the server.
						Logger.Error(e);
						cmd.Status = CommandStatus.Error;
						cmd.Data = null;
					}
					// Send our confirmation/error back to server.
					_client.Send (cmd);
				}
				catch (InvalidCommandException e) {
					// Usually due to a read error or we have become descynchronized
					// from the server. Reconnect after logging.
					Logger.Error (e);
					_client.Reconnect ();
				}
				catch (SocketException e) {
					// Communication error, reconnect after logging.
					Logger.Error (e);
					_client.Reconnect ();
				}
				catch (IOException e){
					Logger.Error (e);
					_client.Reconnect ();
				}
				catch (ObjectDisposedException e) {
					Logger.Error (e);
					_client.Reconnect ();
				}
				catch (Exception e) {
					// An exception, just log it and continue.
					Logger.Error (e);
				}
			}
		}

		public void Start()
		{
			try
			{
				_client.Connect ();
			}
			catch (Exception e) {
				Logger.Error (e);
				throw;
			}
			_thread.Start ();
		}

		public void Stop()
		{
			Running = false;
			_thread.Join ();
		}
	}

	class MainClass
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(MainClass));

		private const string USAGE = @"CloudstrypeArray.exe

	Connects to Cloudstrype server and provides additional storage for striping
	files. Any storage presented by attached array clients will be added to the
	pool of available storage which may also include cloud storage providers.

	Usage:
		cloudstrypearray.exe [--server=<url> --name=<uuid> --path=<path> --size=<size>]

	Options:
		-s --server		Server to connect to [default: ssl://cloudstrype.io:8766]
		-n --name		Name for this node. Hex form of UUID/GUID.
		-p --path		Path at which to store data. [default: $HOME/.cloudstrype]
		-z --size		Maximum size of data to provide. [default: 10GB]
";
		
		public static void Main (string[] args)
		{
			log4net.Config.XmlConfigurator.Configure();

			string server, name, path;
			long size;

			server = ConfigurationManager.AppSettings["server"].ToString();
			name = ConfigurationManager.AppSettings["name"].ToString();
			path = ConfigurationManager.AppSettings["path"].ToString();
			size = Util.ParseFileSize(ConfigurationManager.AppSettings["size"].ToString());

			var env = Environment.GetEnvironmentVariables ();

			if (env["ARRAY_SERVER"] != null) {
				Logger.DebugFormat (
					"Overridding configured server {0} with environment {1}", server, env["ARRAY_SERVER"]);
				server = (string)env["ARRAY_SERVER"].ToString();
			}

			if (env["ARRAY_NAME"] != null) {
				Logger.DebugFormat (
					"Overridding configured name {0} with environment {1}", name, env["ARRAY_NAME"]);
				name = (string)env["ARRAY_NAME"].ToString();
			}

			if (env["ARRAY_PATH"] != null) {
				Logger.DebugFormat (
					"Overridding configured path {0} with environment {1}", path, env["ARRAY_PATH"]);
				path = (string)env["ARRAY_PATH"].ToString();
			}

			if (env["ARRAY_SIZE"] != null) {
				Logger.DebugFormat (
					"Overridding configured size {0} with environment {1}", size, env["ARRAY_SIZE"]);
				size = Util.ParseFileSize((string)env["ARRAY_SIZE"].ToString());
			}

			var arguments = new Docopt ().Apply (
				USAGE, args, version: "CloudstrypeArray.exe 1.0", exit: true);

			if (arguments["--server"] != null) {
				Logger.DebugFormat (
					"Overridding configured server {0} with command line {1}", server, arguments["--server"]);
				server = (string)arguments ["--server"].ToString();
			}
			if (arguments["--name"] != null) {
				Logger.DebugFormat (
					"Overridding configured name {0} with command line {1}", name, arguments["--name"]);
				name = (string)arguments ["--name"].ToString();
			}
			if (arguments["--path"] != null) {
				Logger.DebugFormat (
					"Overridding configured path {0} with command line {1}", path, arguments["--path"]);
				path = (string)arguments ["--path"].ToString();
			}
			if (arguments["--size"] != null) {
				Logger.DebugFormat (
					"Overridding configured size {0} with command line {1}", size, arguments["--size"]);
				size = Util.ParseFileSize((string)arguments ["--size"].ToString());
			}

			path = path.Replace ("$HOME", Util.GetHomeDirectory ());

			Server s = new Server (server, Guid.Parse(name), path, size);
			s.Start ();
			// Console.ReadLine() won't work without console. Console breaks
			// debugger.
			while (true)
				Thread.Sleep (1000);
			s.Stop ();
		}
	}
}
