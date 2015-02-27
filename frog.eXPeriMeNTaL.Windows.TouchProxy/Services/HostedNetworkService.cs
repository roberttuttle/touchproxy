using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;

using frog.eXPeriMeNTaL.Windows.TouchProxy.Common;

namespace frog.eXPeriMeNTaL.Windows.TouchProxy.Services
{
	public class HostedNetworkService : BindableBase, IDisposable
	{
		private bool _isDisposed = false;

		private WlanHostedNetwork _wlanHostedNetwork;

		private bool _isSSIDValid = true;
		private bool _isSecurityKeyValid = true;

		private bool _hasValidationErrors = false;
		public bool HasValidationErrors
		{
			get { return _hasValidationErrors; }
			set { this.SetProperty(ref _hasValidationErrors, value); }
		}

		private string _ssid;
		public string SSID
		{
			get { return _ssid; }
			set 
			{
				string validationError = string.Empty;

				if (!value.Length.IsBetween(1, 32))
				{
					validationError += "SSID must be between 1 and 32 characters in length.";
				}

				if (!value.ToCharArray().All(c => c.IsBetween((char)32, (char)126)))
				{
					if (!string.IsNullOrEmpty(validationError))
					{
						validationError += Environment.NewLine;
					}
					validationError += "SSID must only contain valid ASCII printable characters.";
				}

				if (!string.IsNullOrEmpty(validationError))
				{
					_isSSIDValid = false;
					this.HasValidationErrors = true;
					throw new ArgumentException(validationError);
				}

				this.SetProperty(ref _ssid, value);

				_isSSIDValid = true;
				this.HasValidationErrors = (!_isSSIDValid || !_isSecurityKeyValid);
			}
		}

		private string _securityKey;
		public string SecurityKey
		{
			get { return _securityKey; }
			set 
			{
				string validationError = string.Empty;

				if (!value.Length.IsBetween(8, 63))
				{
					validationError += "Key must be between 8 and 63 characters in length.";
				}

				if (!value.ToCharArray().All(c => c.IsBetween((char)32, (char)126)))
				{
					if (!string.IsNullOrEmpty(validationError))
					{
						validationError += Environment.NewLine;
					}
					validationError += "Key must only contain valid ASCII printable characters.";
				}

				if (!string.IsNullOrEmpty(validationError))
				{
					_isSecurityKeyValid = false;
					this.HasValidationErrors = true;
					throw new ArgumentException(validationError);
				}

				this.SetProperty(ref _securityKey, value);

				_isSecurityKeyValid = true;
				this.HasValidationErrors = (!_isSSIDValid || !_isSecurityKeyValid);
			}
		}

		private bool _isSecurityKeyPersistent = true;
		public bool IsSecurityKeyPersistent
		{
			get { return _isSecurityKeyPersistent; }
			set { this.SetProperty(ref _isSecurityKeyPersistent, value); }
		}

		private string _statusInfo;
		public string StatusInfo
		{
			get { return _statusInfo; }
			set { this.SetProperty(ref _statusInfo, value); }
		}

#pragma warning disable 4014

		private bool _isEnabled = false;
		public bool IsEnabled
		{
			get { return _isEnabled; }
			set
			{
				if (this.SetProperty(ref _isEnabled, value))
				{
					if (_isEnabled)
					{
						try
						{
							_wlanHostedNetwork.UpdateSettings(this.SSID, this.SecurityKey, this.IsSecurityKeyPersistent);
							_wlanHostedNetwork.Start();
						}
						catch (UnauthorizedAccessException ex)
						{
							this.IsEnabled = false;
							MessageBox.Show
							(
								ex.Message,
								"Error: UnauthorizedAccessException",
								MessageBoxButton.OK,
								MessageBoxImage.Error
							);
						}	
					}
					else
					{
						_wlanHostedNetwork.Stop();
					}
				}
			}
		}

		public HostedNetworkService()
		{
			_wlanHostedNetwork = new WlanHostedNetwork();

			SetHostedNetworkInfo();

			_wlanHostedNetwork.StateChanged += (s, e) =>
			{
				SetHostedNetworkInfo();
			};
		}

		~HostedNetworkService()
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
					_wlanHostedNetwork.Dispose();
				}	
			}
			_isDisposed = true;
		}

#pragma warning restore 4014

		private async Task SetHostedNetworkInfo()
		{
			await Task.Run(() =>
			{
				this.SetProperty(ref _isEnabled, _wlanHostedNetwork.IsStarted, "IsEnabled");
				this.SSID = _wlanHostedNetwork.SSID;
				this.SecurityKey = _wlanHostedNetwork.SecondaryKey;
				this.IsSecurityKeyPersistent = _wlanHostedNetwork.IsSecondaryKeyPersistent;
				this.StatusInfo = _wlanHostedNetwork.StatusInfo;
			});
		}
	}
}
