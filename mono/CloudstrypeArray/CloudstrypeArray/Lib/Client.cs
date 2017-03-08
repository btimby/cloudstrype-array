using System;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;

namespace CloudstrypeArray.Lib
{
	public class Client
	{
		// Implements a network client.

		// This client connects to the server, but then acts like a
		// server by receiving commands and executing them.

		protected Socket _sock;
		protected ClientWebSocket _ws;

		public Client()
		{
		}

		public void Connect()
		{
		}

		public void RunOnce()
		{
			// Wait for a command.
			// Dispatch command (event).
		}
	}
}

