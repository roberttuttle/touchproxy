using System;
using System.Net;
using System.Net.Sockets;

namespace OSC.Windows
{
	public class OSCTransmitter : IDisposable
	{
		private bool _isDisposed = false;

		protected UdpClient _udpClient;
		protected string _remoteHost;
		protected int _remotePort;

		public OSCTransmitter(string remoteHost, int remotePort)
		{
			_remoteHost = remoteHost;
			_remotePort = remotePort;
			Connect();
		}

		~OSCTransmitter()
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
			_udpClient = new UdpClient(_remoteHost, _remotePort);
		}

		public void Close()
		{
			if (_udpClient != null) 
			{ 
				_udpClient.Close();
				_udpClient = null;
			}
		}

		public int Send(OSCPacket packet)
		{
			int byteNum = 0;
			byte[] data = packet.BinaryData;
			try 
			{
				byteNum = _udpClient.Send(data, data.Length);
			}
			catch {}
			return byteNum;
		}
	}
}
