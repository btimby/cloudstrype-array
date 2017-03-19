using System;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;

namespace CloudstrypeArray.Lib.Network
{
	public class Client
	{
		// Implements a network client.

		// This client connects to the server, but then acts like a
		// server by receiving commands and executing them.
		//
		// The server is an HTTP server, we connect and authenticate
		// then start up our own HTTP server on the connected socket.
		// Meanwhile the other side switches to HTTP client mode and
		// starts sending requests to us.

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

