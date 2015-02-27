using System;
using System.Net;
using System.Net.Sockets;

namespace OSC.Windows
{
	public class OSCReceiver : IDisposable
	{
		private bool _isDisposed = false;

		protected UdpClient _udpClient;
		protected int _localPort;

		public OSCReceiver(int localPort)
		{
			_localPort = localPort;
			Connect();
		}

		~OSCReceiver()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool isDisposing)
		{
			if (!_isDisposed)
			{
				if (isDisposing) 
				{
					Close();
				}
			}
			_isDisposed = true;
		}

		public void Connect()
		{
			if (_udpClient != null) { Close(); }
			_udpClient = new UdpClient(_localPort);
		}

		public void Close()
		{
			if (_udpClient != null) 
			{ 
				_udpClient.Close();
				_udpClient = null;
			}
		}

		public OSCPacket Receive()
		{
            try
            {
                IPEndPoint ip = null;
                byte[] bytes = _udpClient.Receive(ref ip);
				if (bytes != null && bytes.Length > 0)
				{
					return OSCPacket.Unpack(bytes);
				}
            } 
			catch 
			{ 
                return null;
            }
			return null;
		}
	}
}
