using System;
using System.Net.WebSockets;
using System.Threading;
using CloudstrypeArray.Lib.Storage;
using CloudstrypeArray.Lib.Network;
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

		public Server(string url, string guid)
		{
			_client = new Client (url, Guid.Parse(guid));
			_store = new Storage ();
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
				// TODO: we block here, so we need a way to cancel blocking when
				// asked to Stop();
				try
				{
					Command cmd = _client.Receive ();
					try
					{
						switch (cmd.Type) {
						case CommandType.Get:
							cmd.Data = _store.Read (cmd.ID);
							break;
						case CommandType.Put:
							_store.Write (cmd.ID, cmd.Data);
							break;
						case CommandType.Delete:
							_store.Delete (cmd.ID);
							break;
						}
						cmd.Status = CommandStatus.Success;
					}
					catch (Exception e)
					{
						Logger.Error(e);
						cmd.Status = CommandStatus.Error;
						cmd.Length = 0;
						cmd.Data = null;
					}
					_client.Send (cmd);
				}
				catch (InvalidCommandException e) {
					Logger.Error (e);
					_client.Reconnect ();
				}
				catch (WebSocketException e) {
					Logger.Error (e);
					_client.Reconnect ();
				}
				catch (Exception e) {
					Logger.Error (e);
				}
			}
		}

		public void Start()
		{
			_client.Connect ();
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
		public static void Main (string[] args)
		{
			Server s = new Server ("ws://localhost:8765", "40dd1e40-544d-db46-9f67-df0cae847909");
			s.Start ();
			// Console.ReadLine() won't work without console. Console breaks
			// debugger.
			while (true)
				Thread.Sleep (1000);
			s.Stop ();
		}
	}
}
