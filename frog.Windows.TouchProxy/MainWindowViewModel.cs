using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;

using Microsoft.Win32;

using frog.Windows.TouchProxy.Common;
using frog.Windows.TouchProxy.Diagnostics;
using frog.Windows.TouchProxy.Services;

namespace frog.Windows.TouchProxy
{
	public class MainWindowViewModel : BindableBase
	{
		public const int MIN_TOUCH_MARKER_RADIUS = 4;
		public const int MAX_TOUCH_MARKER_DISPLAY_EXTENT = 320;

		private string _settingsFileName;
		
		private int _touchMarkerRadius = MIN_TOUCH_MARKER_RADIUS;
		
		private Dictionary<ScreenTarget, string> _screenTargets = EnumHelper.ToDictionary<ScreenTarget>();
		public Dictionary<ScreenTarget, string> ScreenTargets
		{
			get { return _screenTargets; }
		}

		private ScreenTarget _selectedScreenTarget = ScreenTarget.Primary;
		public ScreenTarget SelectedScreenTarget
		{
			get { return _selectedScreenTarget; }
			set
			{
				_selectedScreenTarget = value;
				SetScreenDimensions();
			}
		}

		private Dictionary<ProtocolTraceCategory, string> _protocolTraceCategories = EnumHelper.ToDictionary<ProtocolTraceCategory>();
		public Dictionary<ProtocolTraceCategory, string> ProtocolTraceCategories
		{
			get { return _protocolTraceCategories; }
		}

		public TextBox ProtocolTraceTextBox { get; set; }

		private bool _isProtocolTraceEnabled = true;
		public bool IsProtocolTraceEnabled 
		{
			get { return _isProtocolTraceEnabled; }
			set
			{
				_isProtocolTraceEnabled = value;
				SetProtocolTrace();
			}
		}

		private ProtocolTraceCategory _selectedProtocolTraceCategory = ProtocolTraceCategory.OSC;
		public ProtocolTraceCategory SelectedProtocolTraceCategory
		{
			get { return _selectedProtocolTraceCategory; }
			set
			{
				_selectedProtocolTraceCategory = value;
				SetProtocolTrace();
			}
		}

		private Size _calibrationPanelSize;
		public Size CalibrationPanelSize
		{
			get { return _calibrationPanelSize; }
			private set { this.SetProperty(ref _calibrationPanelSize, value); }
		}

		public Size TouchMarkerSize { get; private set; }

		private List<Point> _touchMarkers = new List<Point>();
		public List<Point> TouchMarkers
		{
			get { return _touchMarkers; }
			private set { this.SetProperty(ref _touchMarkers, value); }
		}

		private Rect _primaryScreenRect = new Rect();
		public Rect PrimaryScreenRect
		{
			get { return _primaryScreenRect; }
			set { this.SetProperty(ref _primaryScreenRect, value); }
		}

		public NetworkService NetworkService { get; private set; }
		public HostedNetworkService HostedNetworkService { get; private set; }
		public TouchInjectionService TouchInjectionService { get; private set; }

		public MainWindowViewModel()
		{
			_settingsFileName = Process.GetCurrentProcess().ProcessName + ".settings.xml";

			this.NetworkService = new NetworkService();

			this.HostedNetworkService = new HostedNetworkService();

			this.TouchInjectionService = new TouchInjectionService();
			this.TouchInjectionService.IsEnabledChanged += TouchInjectionService_IsEnabledChanged;
			this.TouchInjectionService.TouchInjected += TouchInjectionService_TouchInjected;

			LoadApplicationSettings();

			SetScreenDimensions();

			SystemEvents.DisplaySettingsChanged += (s, e) =>
			{
				SetScreenDimensions();
			};

			Application.Current.MainWindow.Closed += (s, e) =>
			{
				SaveApplicationSettings();
			};
		}

		public void NavigateAbout()
		{
			Process.Start("http://touchproxy.codeplex.com/");
		}

		private void TouchInjectionService_IsEnabledChanged(object sender, EventArgs e)
		{
			if (this.TouchInjectionService.IsEnabled)
			{
				SetProtocolTrace();
			}
		}

		private void TouchInjectionService_TouchInjected(object sender, TouchInjectedEventArgs e)
		{
			Application.Current.Dispatcher.BeginInvoke(new Action(() =>
			{
				List<Point> touchMarkers = new List<Point>();
				for (int i = 0, ic = e.PointerTouchInfos.Length; i < ic; i++)
				{
					PointerTouchInfo pointerTouchInfo = e.PointerTouchInfos[i];
					if (pointerTouchInfo.PointerInfo.PointerFlags.HasFlag(PointerFlags.UP))
					{
						continue;
					}
					touchMarkers.Add(new Point
					(
						(((pointerTouchInfo.PointerInfo.PtPixelLocation.X - this.TouchInjectionService.ScreenRect.Left) / this.TouchInjectionService.ScreenRect.Width) * this.CalibrationPanelSize.Width) - _touchMarkerRadius,
						(((pointerTouchInfo.PointerInfo.PtPixelLocation.Y - this.TouchInjectionService.ScreenRect.Top) / this.TouchInjectionService.ScreenRect.Height) * this.CalibrationPanelSize.Height) - _touchMarkerRadius
					));
				}
				this.TouchMarkers = touchMarkers;
			}), DispatcherPriority.Normal);
		}

		private void SetScreenDimensions()
		{
			int primaryScreenWidth = (int)SystemParameters.PrimaryScreenWidth;
			int primaryScreenHeight = (int)SystemParameters.PrimaryScreenHeight;
			int virtualScreenWidth = (int)SystemParameters.VirtualScreenWidth;
			int virtualScreenHeight = (int)SystemParameters.VirtualScreenHeight;

			bool isScreenTargetVirtual = this.SelectedScreenTarget.Equals(ScreenTarget.Virtual);

			int width = isScreenTargetVirtual ? virtualScreenWidth : primaryScreenWidth;
			int height = isScreenTargetVirtual ? virtualScreenHeight : primaryScreenHeight;
			int left = isScreenTargetVirtual ? (int)SystemParameters.VirtualScreenLeft : 0;
			int top = isScreenTargetVirtual ? (int)SystemParameters.VirtualScreenTop : 0;

			this.TouchInjectionService.ScreenRect = new Rect(left, top, width, height);

			double aspectRatio = (double)height / width;

			this.CalibrationPanelSize = new Size
			(
				(aspectRatio > 1) ? (MAX_TOUCH_MARKER_DISPLAY_EXTENT / aspectRatio) : MAX_TOUCH_MARKER_DISPLAY_EXTENT,
				(aspectRatio < 1) ? (MAX_TOUCH_MARKER_DISPLAY_EXTENT * aspectRatio) : MAX_TOUCH_MARKER_DISPLAY_EXTENT
			);

			if (isScreenTargetVirtual && !(virtualScreenWidth.Equals(primaryScreenWidth) && virtualScreenHeight.Equals(primaryScreenHeight)))
			{
				this.PrimaryScreenRect = new Rect
				(
					(((double)-left / width) * this.CalibrationPanelSize.Width),
					(((double)-top / height) * this.CalibrationPanelSize.Height),
					((double)primaryScreenWidth / width) * this.CalibrationPanelSize.Width,
					((double)primaryScreenHeight / height) * this.CalibrationPanelSize.Height
				);
			}
			else
			{
				this.PrimaryScreenRect = new Rect(0, 0, 0, 0);
			}

			int radius = (int)Math.Ceiling(TouchInjectionService.CONTACT_AREA_RADIUS * (this.CalibrationPanelSize.Width / width));
			_touchMarkerRadius = (radius >= MIN_TOUCH_MARKER_RADIUS) ? radius : MIN_TOUCH_MARKER_RADIUS;
			int diameter = _touchMarkerRadius * 2;
			this.TouchMarkerSize = new Size(diameter, diameter);
		}

		private void SetProtocolTrace()
		{
			if (this.ProtocolTraceTextBox != null)
			{
				foreach (TraceListener traceListener in Trace.Listeners)
				{
					if (traceListener is TextBoxTraceListener)
					{
						if (((TextBoxTraceListener)traceListener).TextBox.Equals(this.ProtocolTraceTextBox))
						{
							Trace.Listeners.Remove(traceListener);
							break;
						}
					}
				}

				if (this.IsProtocolTraceEnabled)
				{
					Trace.Listeners.Add(new TextBoxTraceListener(this.ProtocolTraceTextBox, this.SelectedProtocolTraceCategory.ToString()));
				}
			}
		}

		private void LoadApplicationSettings()
		{
			try
			{
				if (File.Exists(_settingsFileName))
				{
					XElement settings = XElement.Load(_settingsFileName);

					if (settings.HasElements)
					{
						foreach (XElement xe in settings.Elements("setting"))
						{
							if (xe.HasAttributes)
							{
								XAttribute key = xe.Attribute("key");
								XAttribute value = xe.Attribute("value");
								if (key != null & value != null)
								{
									switch (key.Value)
									{
										case "Port":
											int port;
											if (Int32.TryParse(value.Value, out port))
											{
												this.TouchInjectionService.Port = port;
											}
											break;
										case "SelectedScreenTarget":
											ScreenTarget selectedScreenTarget;
											if (Enum.TryParse(value.Value, true, out selectedScreenTarget))
											{
												this.SelectedScreenTarget = selectedScreenTarget;
											}
											break;
										case "IsContactEnabled":
											bool isContactEnabled;
											if (Boolean.TryParse(value.Value, out isContactEnabled))
											{
												this.TouchInjectionService.IsContactEnabled = isContactEnabled;
											}
											break;
										case "IsContactVisible":
											bool isContactVisible;
											if (Boolean.TryParse(value.Value, out isContactVisible))
											{
												this.TouchInjectionService.IsContactVisible = isContactVisible;
											}
											break;
										case "IsWindowsKeyPressEnabled":
											bool isWindowsKeyPressEnabled;
											if (Boolean.TryParse(value.Value, out isWindowsKeyPressEnabled))
											{
												this.TouchInjectionService.IsWindowsKeyPressEnabled = isWindowsKeyPressEnabled;
											}
											break;
										case "WindowsKeyPressTouchCount":
											int windowsKeyPressTouchCount;
											if (Int32.TryParse(value.Value, out windowsKeyPressTouchCount))
											{
												this.TouchInjectionService.WindowsKeyPressTouchCount = windowsKeyPressTouchCount;
											}
											break;
										case "CalibrationBuffer":
											string[] calibrationBuffer = value.Value.Split(',');
											switch (calibrationBuffer.Length)
											{
												case 4:
													double l = Double.TryParse(calibrationBuffer[0], out l) ? l : 0;
													double t = Double.TryParse(calibrationBuffer[1], out t) ? t : 0;
													double r = Double.TryParse(calibrationBuffer[2], out r) ? r : 0;
													double b = Double.TryParse(calibrationBuffer[3], out b) ? b : 0;

													this.TouchInjectionService.CalibrationBufferLeft = l;
													this.TouchInjectionService.CalibrationBufferTop = t;
													this.TouchInjectionService.CalibrationBufferRight = r;
													this.TouchInjectionService.CalibrationBufferBottom = b;
													break;
												case 2:
													double lr = Double.TryParse(calibrationBuffer[0], out lr) ? lr : 0;
													double tb = Double.TryParse(calibrationBuffer[1], out tb) ? tb : 0;

													this.TouchInjectionService.CalibrationBufferLeft = lr;
													this.TouchInjectionService.CalibrationBufferTop = tb;
													this.TouchInjectionService.CalibrationBufferRight = lr;
													this.TouchInjectionService.CalibrationBufferBottom = tb;
													break;
												case 1:
													double ltrb = Double.TryParse(calibrationBuffer[0], out ltrb) ? ltrb : 0;

													this.TouchInjectionService.CalibrationBufferLeft = ltrb;
													this.TouchInjectionService.CalibrationBufferTop = ltrb;
													this.TouchInjectionService.CalibrationBufferRight = ltrb;													
													this.TouchInjectionService.CalibrationBufferBottom = ltrb;
													break;
												default:
													break;
											}
											break;
										case "IsProtocolTraceEnabled":
											bool isProtocolTraceEnabled = Boolean.TryParse(value.Value, out isProtocolTraceEnabled) ? isProtocolTraceEnabled : this.IsProtocolTraceEnabled;
											this.IsProtocolTraceEnabled = isProtocolTraceEnabled;
											break;
										case "SelectedProtocolTraceCategory":
											ProtocolTraceCategory selectedProtocolTraceCategory = Enum.TryParse(value.Value, true, out selectedProtocolTraceCategory) ? selectedProtocolTraceCategory : this.SelectedProtocolTraceCategory;
											this.SelectedProtocolTraceCategory = selectedProtocolTraceCategory;
											break;
										default:
											break;
									}
								}
							}
						}
					}
				}
			}
			catch
			{
				// HACK: swallowed exception is OK here as loading settings saved in the background is a convenience option and not a requirement. No need to bug the user about missing or invalid settings XML file that cannot be loaded.
			}
		}

		private void SaveApplicationSettings()
		{
			try
			{
				new XElement
				(
					"settings",
					new XElement("setting", new XAttribute("key", "Port"), new XAttribute("value", this.TouchInjectionService.Port)),
					new XElement("setting", new XAttribute("key", "SelectedScreenTarget"), new XAttribute("value", this.SelectedScreenTarget)),
					new XElement("setting", new XAttribute("key", "IsContactEnabled"), new XAttribute("value", this.TouchInjectionService.IsContactEnabled)),
					new XElement("setting", new XAttribute("key", "IsContactVisible"), new XAttribute("value", this.TouchInjectionService.IsContactVisible)),
					new XElement("setting", new XAttribute("key", "IsWindowsKeyPressEnabled"), new XAttribute("value", this.TouchInjectionService.IsWindowsKeyPressEnabled)),
					new XElement("setting", new XAttribute("key", "WindowsKeyPressTouchCount"), new XAttribute("value", this.TouchInjectionService.WindowsKeyPressTouchCount)),
					new XElement("setting", new XAttribute("key", "CalibrationBuffer"), new XAttribute("value", string.Format("{0},{1},{2},{3}", this.TouchInjectionService.CalibrationBufferLeft, this.TouchInjectionService.CalibrationBufferTop, this.TouchInjectionService.CalibrationBufferRight, this.TouchInjectionService.CalibrationBufferBottom))),
					new XElement("setting", new XAttribute("key", "IsProtocolTraceEnabled"), new XAttribute("value", this.IsProtocolTraceEnabled)),
					new XElement("setting", new XAttribute("key", "SelectedProtocolTraceCategory"), new XAttribute("value", this.SelectedProtocolTraceCategory))
				).Save(_settingsFileName);
			}
			catch
			{
				// HACK: swallowed exception is OK here as saving current settings in the background is a convenience option and not a requirement. No need to bug the user about being unable to save settings XML file due to permissions or access issue.
			}
		}
	}
}
