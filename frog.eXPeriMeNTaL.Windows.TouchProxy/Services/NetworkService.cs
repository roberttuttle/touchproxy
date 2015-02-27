using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

using frog.eXPeriMeNTaL.Windows.TouchProxy.Common;

namespace frog.eXPeriMeNTaL.Windows.TouchProxy.Services
{
	public class NetworkService : BindableBase
	{
		public struct NetworkInterfaceInfo
		{
			public string Address { get; private set; }
			public string Type { get; private set; }
			public string Description { get; private set; }

			public NetworkInterfaceInfo(NetworkInterface nic, IPAddress address) : this()
			{
				this.Address = address.ToString();
				switch (nic.NetworkInterfaceType)
				{
					case NetworkInterfaceType.Ethernet:
						this.Type = "Ethernet";
						break;
					case NetworkInterfaceType.Wireless80211:
						this.Type = "Wi-Fi";
						break;
					default:
						this.Type = "Other";
						break;
				}
				this.Description = nic.Description;
			}
		}

		private List<NetworkInterfaceInfo> _networkInterfaceInfos;
		public List<NetworkInterfaceInfo> NetworkInterfaceInfos
		{
			get { return _networkInterfaceInfos; }
			set { this.SetProperty(ref _networkInterfaceInfos, value); }
		}

#pragma warning disable 4014

		public NetworkService()
		{
			SetNetworkInterfaceInfos();
			
			NetworkChange.NetworkAddressChanged += (s, e) => 
			{ 
				SetNetworkInterfaceInfos(); 
			};

			NetworkChange.NetworkAvailabilityChanged += (s, e) => 
			{ 
				SetNetworkInterfaceInfos(); 
			};
		}

#pragma warning restore 4014

		private async Task SetNetworkInterfaceInfos()
		{
			await Task.Run(() =>
			{
				this.NetworkInterfaceInfos =
				(
					from nic in NetworkInterface.GetAllNetworkInterfaces()
						where nic.OperationalStatus == OperationalStatus.Up
					from uca in nic.GetIPProperties().UnicastAddresses
						where uca.Address.AddressFamily == AddressFamily.InterNetwork
						&& uca.IsDnsEligible
						&& !IPAddress.IsLoopback(uca.Address)
					select new NetworkInterfaceInfo(nic, uca.Address)
				).ToList<NetworkInterfaceInfo>();
			});
		}
	}
}
