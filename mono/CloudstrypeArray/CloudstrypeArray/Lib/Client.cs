using System;
using System.Net;
using System.Net.Sockets;

namespace CloudstrypeArray.Lib
{
	public class Client
	{
		// Implements a network client.

		// This client connects to the server, but then acts like a
		// server by receiving commands and executing them.

		protected Socket Connection;

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

