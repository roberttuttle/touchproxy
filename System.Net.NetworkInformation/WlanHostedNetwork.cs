using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Net.NetworkInformation
{
	public class WlanHostedNetwork : IDisposable
	{
		private bool _isDisposed = false;

		private IntPtr _clientHandle;
		private WlanHostedNetworkReason _reason;
		private WlanNotificationEventHandler _wlanNotificationEventHandler;
		private uint _maxNumberOfPeers;

		public bool IsStarted { get; private set; }
		public string SSID { get; private set; }
		public string SecondaryKey { get; private set; }
		public bool IsSecondaryKeyPersistent { get; private set; }
		public string StatusInfo { get; private set; }

		public event EventHandler StateChanged;
		private void OnStateChanged()
		{
			if (this.StateChanged != null)
			{
				this.StateChanged(this, EventArgs.Empty);
			}
		}

		public WlanHostedNetwork()
        {
			uint version;
			NativeMethods.WlanOpenHandle((uint)WlanAPIClientVersion.VISTA, IntPtr.Zero, out version, ref _clientHandle);

			NativeMethods.WlanHostedNetworkInitSettings(_clientHandle, out _reason, IntPtr.Zero);

			if (_reason == WlanHostedNetworkReason.Success)
			{
				SetWlanHostedNetworkInfo();

				_wlanNotificationEventHandler = new WlanNotificationEventHandler(HandleNotificationEvent);
				WlanNotificationSource notificationSource;
				NativeMethods.WlanRegisterNotification(_clientHandle, WlanNotificationSource.All, true, _wlanNotificationEventHandler, IntPtr.Zero, IntPtr.Zero, out notificationSource);	
			}
			else
			{
				NativeMethods.WlanCloseHandle(_clientHandle, IntPtr.Zero);
			}
        }

		~WlanHostedNetwork()
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
				if (isDisposing) { }

				if (_clientHandle != IntPtr.Zero)
				{
					NativeMethods.WlanCloseHandle(_clientHandle, IntPtr.Zero);
					_clientHandle = IntPtr.Zero;
				}
			}
			_isDisposed = true;
		}

		public void Start()
		{
			if (!this.IsStarted)
			{
				CheckElevationRequired();

				this.IsStarted = true;
				NativeMethods.WlanHostedNetworkStartUsing(_clientHandle, out _reason, IntPtr.Zero);
			}
		}

		public void Stop()
		{
			if (this.IsStarted)
			{
				this.IsStarted = false;
				NativeMethods.WlanHostedNetworkForceStop(_clientHandle, out _reason, IntPtr.Zero);
			}
		}

		public void UpdateSettings(string ssid, string secondaryKey, bool isSecondaryKeyPersistent)
		{
			CheckElevationRequired();

			WlanHostedNetworkConnectionSettings settings = new WlanHostedNetworkConnectionSettings();
			settings.SSID = ssid.ToDot11SSID();
			settings.MaxNumberOfPeers = _maxNumberOfPeers;

			IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(settings));
			Marshal.StructureToPtr(settings, ptr, false);
			NativeMethods.WlanHostedNetworkSetProperty(_clientHandle, WlanHostedNetworkOpCode.ConnectionSettings, (uint)Marshal.SizeOf(settings), ptr, out _reason, IntPtr.Zero);
			NativeMethods.WlanFreeMemory(ptr);

			NativeMethods.WlanHostedNetworkSetSecondaryKey(_clientHandle, (uint)(secondaryKey.Length + 1), secondaryKey, true, isSecondaryKeyPersistent, out _reason, IntPtr.Zero);		
		}

		private void CheckElevationRequired()
		{
			IntPtr ptr = IntPtr.Zero;

			uint size;
			WlanOpCodeValueType opcode;
			ptr = IntPtr.Zero;
			NativeMethods.WlanHostedNetworkQueryProperty(_clientHandle, WlanHostedNetworkOpCode.Enable, out size, out ptr, out opcode, IntPtr.Zero);
			bool isEnabled = (bool)Marshal.PtrToStructure(ptr, typeof(bool));
			NativeMethods.WlanFreeMemory(ptr);

			if (!isEnabled)
			{
				ptr = Marshal.AllocHGlobal(Marshal.SizeOf(1));
				Marshal.WriteInt32(ptr, 1);
				NativeMethods.WlanHostedNetworkSetProperty(_clientHandle, WlanHostedNetworkOpCode.Enable, (uint)Marshal.SizeOf(1), ptr, out _reason, IntPtr.Zero);
				NativeMethods.WlanFreeMemory(ptr);
				if (_reason == WlanHostedNetworkReason.ElevationRequired)
				{
					throw new UnauthorizedAccessException("Access to manage the wireless hosted network is currently disallowed. You can choose to run this application with elevated privleges (\"Run as administrator\") or open up a command prompt as an administrator and run \"netsh wlan set hostednetwork mode=allow\".");
				}
			}
		}

		private void HandleNotificationEvent(ref WlanNotificationData data, IntPtr context)
		{
			switch (data.Code)
			{
				case (int)WlanHostedNetworkNotificationCode.StateChange:
				case (int)WlanHostedNetworkNotificationCode.PeerStateChange:
					SetWlanHostedNetworkInfo();
					OnStateChanged();
					break;
				default:
					break;
			}	
		}

		public void SetWlanHostedNetworkInfo()
		{
			try
			{
				IntPtr ptr = IntPtr.Zero;

				uint size;
				WlanOpCodeValueType opcode;

				ptr = IntPtr.Zero;
				NativeMethods.WlanHostedNetworkQueryProperty(_clientHandle, WlanHostedNetworkOpCode.Enable, out size, out ptr, out opcode, IntPtr.Zero);
				bool isEnabled = (bool)Marshal.PtrToStructure(ptr, typeof(bool));
				NativeMethods.WlanFreeMemory(ptr);

				ptr = IntPtr.Zero;
				NativeMethods.WlanHostedNetworkQueryProperty(_clientHandle, WlanHostedNetworkOpCode.ConnectionSettings, out size, out ptr, out opcode, IntPtr.Zero);
				WlanHostedNetworkConnectionSettings settings = (WlanHostedNetworkConnectionSettings)Marshal.PtrToStructure(ptr, typeof(WlanHostedNetworkConnectionSettings));
				NativeMethods.WlanFreeMemory(ptr);
				this.SSID = settings.SSID.Value;
				_maxNumberOfPeers = settings.MaxNumberOfPeers;

				uint length;
				string key;
				bool isPassPhrase;
				bool isPersistent;
				NativeMethods.WlanHostedNetworkQuerySecondaryKey(_clientHandle, out length, out key, out isPassPhrase, out isPersistent, out _reason, IntPtr.Zero);
				this.SecondaryKey = key;
				this.IsSecondaryKeyPersistent = isPersistent;

				IntPtr pPtr = IntPtr.Zero;
				NativeMethods.WlanHostedNetworkQueryStatus(_clientHandle, out pPtr, IntPtr.Zero);
				ptr = new IntPtr(pPtr.ToInt32());
				WlanHostedNetworkStatus status = (WlanHostedNetworkStatus)Marshal.PtrToStructure(ptr, typeof(WlanHostedNetworkStatus));
				NativeMethods.WlanFreeMemory(ptr);
				this.IsStarted = (status.State == WlanHostedNetworkState.Active);
				string radioType = string.Empty;
				switch (status.PhyType)
				{
					case Dot11PhyType.ERP:
					case Dot11PhyType.HRDSSS:
						radioType = "802.11g";
						break;
					case Dot11PhyType.OFDM:
					case Dot11PhyType.IRBaseband:
						radioType = "802.11a";
						break;
					case Dot11PhyType.HT:
						radioType = "802.11n";
						break;
					default:
						radioType = "Unknown";
						break;
				}

				StringBuilder sb = new StringBuilder();
				sb.AppendFormat("Hosted network settings\r\n");
				sb.AppendFormat("----------------------------\r\n");
				sb.AppendFormat("Mode\t\t\t: {0}\r\n", (isEnabled ? "Allowed" : "Disallowed"));
				sb.AppendFormat("SSID\t\t\t: {0}\r\n", this.SSID);
				sb.AppendFormat("Key\t\t\t: {0}\r\n", this.SecondaryKey);
				sb.AppendFormat("Authentication\t: WPA2-Personal\r\n");
				sb.AppendFormat("\r\n");
				sb.AppendFormat("Hosted network status\r\n");
				sb.AppendFormat("----------------------------\r\n");
				sb.AppendFormat("Status\t\t\t: {0}\r\n", (this.IsStarted ? "Started" : "Not started"));
				if (this.IsStarted)
				{
					sb.AppendFormat("Radio type\t\t: {0}\r\n", radioType);
					sb.AppendFormat("Channel\t\t: {0}\r\n", status.Channel);
					sb.AppendFormat("BSSID\t\t\t: {0}\r\n", status.BSSID.ToMacAddressString());
					sb.AppendFormat("Connected clients\t: {0}\r\n", status.NumberOfPeers);
				}
				this.StatusInfo = sb.ToString();
			}
			catch (NullReferenceException) {}
		}

		internal static class NativeMethods
		{
			[DllImport("Wlanapi.dll")]
			internal static extern uint WlanOpenHandle(uint dwClientVersion, IntPtr pReserved, [Out] out uint pdwNegotiatedVersion, ref IntPtr hClientHandle);

			[DllImport("Wlanapi.dll")]
			internal static extern uint WlanCloseHandle([In] IntPtr hClientHandle, IntPtr pReserved);

			[DllImport("Wlanapi.dll")]
			internal static extern uint WlanHostedNetworkInitSettings(IntPtr hClientHandle, [Out] out WlanHostedNetworkReason pFailReason, IntPtr pReserved);

			[DllImport("Wlanapi.dll")]
			internal static extern uint WlanRegisterNotification(IntPtr hClientHandle, WlanNotificationSource dwNotifSource, bool bIgnoreDuplicate, WlanNotificationEventHandler funcCallback, IntPtr pCallbackContext, IntPtr pReserved, [Out] out WlanNotificationSource pdwPrevNotifSource);

			[DllImport("Wlanapi.dll")]
			internal static extern uint WlanHostedNetworkQueryStatus(IntPtr hClientHandle, [Out] out IntPtr ppWlanHostedNetworkStatus, IntPtr pReserved);

			[DllImport("Wlanapi.dll")]
			internal static extern uint WlanHostedNetworkQueryProperty(IntPtr hClientHandle, WlanHostedNetworkOpCode pOpCode, [Out] out uint pDataSize, [Out] out IntPtr ppvData, [Out] out WlanOpCodeValueType pWlanOpcodeValueType, IntPtr pReserved);

			[DllImport("Wlanapi.dll")]
			internal static extern uint WlanHostedNetworkSetProperty(IntPtr hClientHandle, WlanHostedNetworkOpCode pOpCode, uint dwDataSize, IntPtr pvData, [Out] out WlanHostedNetworkReason pFailReason, IntPtr pReserved);

			[DllImport("Wlanapi.dll")]
			internal static extern uint WlanHostedNetworkQuerySecondaryKey(IntPtr hClientHandle, [Out] out uint pKeyLength, [Out, MarshalAs(UnmanagedType.LPStr)] out string ppucKeyData, [Out] out bool pbIsPassPhrase, [Out] out bool pbPersistent, [Out] out WlanHostedNetworkReason pFailReason, IntPtr pReserved);

			[DllImport("Wlanapi.dll", CharSet = CharSet.Unicode, BestFitMapping = false, ThrowOnUnmappableChar = true)]
			internal static extern uint WlanHostedNetworkSetSecondaryKey(IntPtr hClientHandle, uint dwKeyLength, [In, MarshalAs(UnmanagedType.LPStr)] string pucKeyData, bool bIsPassPhrase, bool bPersistent, [Out] out WlanHostedNetworkReason pFailReason, IntPtr pReserved);

			[DllImport("Wlanapi.dll")]
			internal static extern uint WlanHostedNetworkStartUsing(IntPtr hClientHandle, [Out] out WlanHostedNetworkReason pFailReason, IntPtr pReserved);

			[DllImport("Wlanapi.dll")]
			internal static extern uint WlanHostedNetworkForceStop(IntPtr hClientHandle, [Out] out WlanHostedNetworkReason pFailReason, IntPtr pReserved);

			[DllImport("Wlanapi.dll")]
			internal static extern void WlanFreeMemory([In] IntPtr pMemory);
		}
	}

	internal delegate void WlanNotificationEventHandler(ref WlanNotificationData notificationData, IntPtr context);

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct WlanHostedNetworkStatus
	{
		public WlanHostedNetworkState State;
		public Guid IPDeviceID;
		public Dot11MacAddress BSSID;
		public Dot11PhyType PhyType;
		public uint Channel;
		public uint NumberOfPeers;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct WlanNotificationData
	{
		public WlanNotificationSource Source;
		public int Code;
		public Guid InterfaceGuid;
		public int DataSize;
		public IntPtr DataPtr;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct WlanHostedNetworkConnectionSettings
	{
		public Dot11SSID SSID;
		public UInt32 MaxNumberOfPeers;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct Dot11MacAddress
	{
		public byte Set1;
		public byte Set2;
		public byte Set3;
		public byte Set4;
		public byte Set5;
		public byte Set6;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct Dot11SSID
	{
		public int Length;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string Value;
	}

	internal enum WlanAPIClientVersion : uint
	{
		XP = 1,
		VISTA = 2
	}

	[Flags]
	internal enum WlanNotificationSource : uint
	{
		None = 0,
		All = 0X0000FFFF,
		ACM = 0X00000008,
		MSM = 0X00000010,
		Security = 0X00000020,
		IHV = 0X00000040
	}

	internal enum WlanHostedNetworkNotificationCode
	{
		StateChange = 0x00001000,
		PeerStateChange,
		RadioStateChange
	}

	internal enum WlanHostedNetworkState
	{
		Unavailable,
		Idle,
		Active
	}

	internal enum WlanHostedNetworkPeerAuthState
	{
		Invalid,
		Authenticated
	}

	internal enum Dot11PhyType
	{
		Unknown,
		Any,
		FHSS,
		DSSS,
		IRBaseband,
		OFDM,
		HRDSSS,
		ERP,
		HT,
		IHVStart,
		IHVEnd
	}

	internal enum WlanHostedNetworkReason
	{
		Success = 0,
		Unspecified,
		BadParameters,
		ServiceShuttingDown,
		InsufficientResources,
		ElevationRequired,
		ReadOnly,
		PersistenceFailed,
		CryptError,
		Impersonation,
		StopBeforeStart,
		InterfaceAvailable,
		InterfaceUnavailable,
		MiniportStopped,
		MiniportStarted,
		IncompatibleConnectionStarted,
		IncompatibleConnectionStopped,
		UserAction,
		ClientAbort,
		ApStartFailed,
		PeerArrived,
		PeerDeparted,
		PeerTimeout,
		GPDenied,
		ServiceUnavailable,
		DeviceChange,
		PropertiesChange,
		VirtualStationBlockingUse,
		ServiceAvailableOnVirtualStation
	}

	internal enum WlanHostedNetworkOpCode
	{
		ConnectionSettings,
		SecuritySettings,
		StationProfile,
		Enable
	}

	internal enum WlanOpCodeValueType
	{
		QueryOnly = 0,
		SetByGroupPolicy = 1,
		SetByUser = 2,
		Invalid = 3
	}

	internal static class WlanExtensions
	{
		internal static string ToMacAddressString(this Dot11MacAddress value)
		{
			return string.Format("{0}:{1}:{2}:{3}:{4}:{5}", value.Set1.ToHexString(), value.Set2.ToHexString(), value.Set3.ToHexString(), value.Set4.ToHexString(), value.Set5.ToHexString(), value.Set6.ToHexString());
		}

		internal static string ToHexString(this byte value)
		{
			return Convert.ToString(value, 0x10).PadLeft(2, '0').ToUpper();
		}

		internal static Dot11SSID ToDot11SSID(this string value)
		{
			return new Dot11SSID()
			{
				Value = value,
				Length = value.Length
			};
		}
	}
}
