using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using TUIO;

using frog.eXPeriMeNTaL.Windows.TouchProxy.Common;

namespace frog.eXPeriMeNTaL.Windows.TouchProxy.Services
{
	public class TouchInjectionService : BindableBase, ITuioListener, IDisposable
	{
		private bool _isDisposed = false;

		public const int DEFAULT_PORT = 3333;
		public const uint MAX_CONTACTS = 256;
		public const int CONTACT_AREA_RADIUS = 24;

		private const uint TOUCH_ORIENTATION = 0;
		private const uint TOUCH_PRESSURE = 1024;
		private const int DEFAULT_WINDOWS_KEY_PRESS_TOUCH_COUNT = 5;
		private const double CALIBRATION_BUFFER_MAXLENGTH = 30;

		private static volatile bool _isTouchInjectionSuspended = false;

		private TuioClient _tuioClient;

		private List<PointerTouchInfo> _pointerTouchInfos = new List<PointerTouchInfo>();
		
		private DispatcherTimer _refreshTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(250) };

		private int _port = DEFAULT_PORT;
		public int Port 
		{
			get { return _port; }
			set
			{
				if (IsPortValid(value))
				{
					_port = value;
				}
			}
		}

		public static TextConstraintPredicateDelegate IsPortValidPredicate
		{
			get
			{
				return delegate(string input)
				{
					int value = Int32.TryParse(input, out value) ? value : Int32.MinValue;
					return IsPortValid(value);
				};
			}
		}

		private static bool IsPortValid(int port)
		{
			return port.IsBetween(1, 65535);
		}

		private bool _isContactEnabled = true;
		public bool IsContactEnabled 
		{
			get { return _isContactEnabled; }
			set { _isContactEnabled = value; } 
		}

		private bool _isContactVisible = true;
		public bool IsContactVisible 
		{
			get { return _isContactVisible; }
			set { _isContactVisible = value; }  
		}

		private bool _isWindowsKeyPressEnabled = false;
		public bool IsWindowsKeyPressEnabled 
		{
			get { return _isWindowsKeyPressEnabled; }
			set { _isWindowsKeyPressEnabled = value; }  
		}

		private int _windowsKeyPressTouchCount = DEFAULT_WINDOWS_KEY_PRESS_TOUCH_COUNT;
		public int WindowsKeyPressTouchCount 
		{
			get { return _windowsKeyPressTouchCount; }
			set
			{
				if (this.WindowsKeyPressTouchCounts.Contains(value))
				{
					_windowsKeyPressTouchCount = value;
				}
			}
		}

		private List<int> _windowsKeyPressTouchCounts = new List<int> { 3, 4, 5 };
		public List<int> WindowsKeyPressTouchCounts
		{
			get { return _windowsKeyPressTouchCounts; }
		}

		private Rect _screenRect = new Rect();
		public Rect ScreenRect
		{
			get { return _screenRect; }
			set { this.SetProperty(ref _screenRect, value); }
		}

		public double CalibrationBufferMaxLength
		{
			get { return CALIBRATION_BUFFER_MAXLENGTH; }
		}

		public double CalibrationBufferMinLength
		{
			get { return -(CALIBRATION_BUFFER_MAXLENGTH); }
		}

		private double _calibrationBufferLeft = 0;
		public double CalibrationBufferLeft
		{
			get { return _calibrationBufferLeft; }
			set 
			{
				_calibrationBufferLeft = (value.IsBetween(-(CALIBRATION_BUFFER_MAXLENGTH), CALIBRATION_BUFFER_MAXLENGTH)) ? value : 0;
				SetCalibrationBuffer();
			}
		}

		private double _calibrationBufferTop = 0;
		public double CalibrationBufferTop
		{
			get { return _calibrationBufferTop; }
			set
			{
				_calibrationBufferTop = (value.IsBetween(-(CALIBRATION_BUFFER_MAXLENGTH), CALIBRATION_BUFFER_MAXLENGTH)) ? value : 0;
				SetCalibrationBuffer();
			}
		}

		private double _calibrationBufferRight = 0;
		public double CalibrationBufferRight
		{
			get { return _calibrationBufferRight; }
			set
			{
				_calibrationBufferRight = (value.IsBetween(-(CALIBRATION_BUFFER_MAXLENGTH), CALIBRATION_BUFFER_MAXLENGTH)) ? value : 0;
				SetCalibrationBuffer();
			}
		}

		private double _calibrationBufferBottom = 0;
		public double CalibrationBufferBottom
		{
			get { return _calibrationBufferBottom; }
			set
			{
				_calibrationBufferBottom = (value.IsBetween(-(CALIBRATION_BUFFER_MAXLENGTH), CALIBRATION_BUFFER_MAXLENGTH)) ? value : 0;
				SetCalibrationBuffer();
			}
		}

		private struct CalibrationBuffer
		{
			public double Left { get; private set; }
			public double Top { get; private set; }
			public double Width { get; private set; }
			public double Height { get; private set; }

			public CalibrationBuffer(double left, double top, double right, double bottom) : this()
			{
				double oX = Math.Abs(left - right);
				double oY = Math.Abs(top - bottom);

				this.Left = (left > right) ? -(oX) : oX;
				this.Top = (top > bottom) ? -(oY) : oY;
				this.Width = left + right;
				this.Height = top + bottom;
			}
		}

		private CalibrationBuffer _calibrationBuffer = new CalibrationBuffer();

		private void SetCalibrationBuffer()
		{
			_calibrationBuffer = new CalibrationBuffer
			(
				_calibrationBufferLeft, 
				_calibrationBufferTop, 
				_calibrationBufferRight, 
				_calibrationBufferBottom
			);
		}

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
						Start();
					}
					else
					{
						Stop();
					}
					this.OnIsEnabledChanged(null);
				}
			}
		}

		public event EventHandler IsEnabledChanged;
		public virtual void OnIsEnabledChanged(EventArgs e)
		{
			if (IsEnabledChanged != null)
			{
				IsEnabledChanged(this, e);
			}
		}

		public event TouchInjectedEventHandler TouchInjected;
		public virtual void OnTouchInjected(TouchInjectedEventArgs e)
		{
			if (TouchInjected != null)
			{
				TouchInjected(this, e);
			}
		}

		public TouchInjectionService()
		{
			_refreshTimer.Tick += (s, e) => 
			{ 
				InjectPointerTouchInfos(); 
			};
		}

		~TouchInjectionService()
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
					_tuioClient.Dispose();
				}
			}
			_isDisposed = true;
		}

		private void Start()
		{
			if (_tuioClient != null) 
			{ 
				Stop(); 
			}

			TouchInjection.Initialize(MAX_CONTACTS, this.IsContactVisible ? TouchFeedback.INDIRECT : TouchFeedback.NONE);

			_tuioClient = new TuioClient(this.Port);
			_tuioClient.addTuioListener(this);
			try
			{
				_tuioClient.connect();
			}
			catch (Exception e)
			{
				this.IsEnabled = false;
				if (e is SocketException)
				{
					SocketException se = (SocketException)e;
					MessageBox.Show(string.Format("{0}\r\n\r\nError Code: {1} ({2})", se.Message, se.ErrorCode, se.SocketErrorCode), "Error: SocketException", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private void Stop()
		{
			_pointerTouchInfos.Clear();
			InjectPointerTouchInfos();

			if (_tuioClient != null)
			{
				_tuioClient.disconnect();
				_tuioClient.removeTuioListener(this);
				_tuioClient = null;
			}
		}

		public void AddTuioCursor(TuioCursor tuioCursor)
		{
			_refreshTimer.Stop();

			int pid = tuioCursor.getCursorID();

			int i = _pointerTouchInfos.FindIndex(pti => pti.PointerInfo.PointerId == pid);
			if (i != -1)
			{
				_pointerTouchInfos.RemoveAt(i);
			}

			int x = (int)((tuioCursor.getX() * (_screenRect.Width + _calibrationBuffer.Width)) + _calibrationBuffer.Left + _screenRect.Left);
			int y = (int)((tuioCursor.getY() * (_screenRect.Height + _calibrationBuffer.Height)) + _calibrationBuffer.Top + _screenRect.Top);

			_pointerTouchInfos.Add
			(
				new PointerTouchInfo()
				{
					TouchFlags = TouchFlags.NONE,
					Orientation = TOUCH_ORIENTATION,
					Pressure = TOUCH_PRESSURE,
					TouchMasks = TouchMask.CONTACTAREA | TouchMask.ORIENTATION | TouchMask.PRESSURE,
					PointerInfo = new PointerInfo
					{
						PointerInputType = PointerInputType.TOUCH,
						PointerFlags = PointerFlags.DOWN | PointerFlags.INRANGE | ((this.IsContactEnabled) ? PointerFlags.INCONTACT : PointerFlags.NONE),
						PtPixelLocation = new PointerTouchPoint { X = x, Y = y },
						PointerId = (uint)pid
					},
					ContactArea = new ContactArea
					{
						Left = x - CONTACT_AREA_RADIUS,
						Right = x + CONTACT_AREA_RADIUS,
						Top = y - CONTACT_AREA_RADIUS,
						Bottom = y + CONTACT_AREA_RADIUS
					}
				}
			);

			Trace.WriteLine(string.Format("add cur {0} ({1}) {2} {3}", pid, tuioCursor.getSessionID(), x, y), "TUIO");
		}

		public void UpdateTuioCursor(TuioCursor tuioCursor)
		{
			_refreshTimer.Stop();

			int pid = tuioCursor.getCursorID();

			int i = _pointerTouchInfos.FindIndex(pti => pti.PointerInfo.PointerId == pid);
			if (i != -1)
			{
				int x = (int)((tuioCursor.getX() * (_screenRect.Width + _calibrationBuffer.Width)) + _calibrationBuffer.Left + _screenRect.Left);
				int y = (int)((tuioCursor.getY() * (_screenRect.Height + _calibrationBuffer.Height)) + _calibrationBuffer.Top + _screenRect.Top);

				PointerTouchInfo pointerTouchInfo = _pointerTouchInfos[i];
				pointerTouchInfo.PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INRANGE | ((this.IsContactEnabled) ? PointerFlags.INCONTACT : PointerFlags.NONE);
				pointerTouchInfo.PointerInfo.PtPixelLocation = new PointerTouchPoint { X = x, Y = y };
				pointerTouchInfo.ContactArea = new ContactArea
				{
					Left = x - CONTACT_AREA_RADIUS,
					Right = x + CONTACT_AREA_RADIUS,
					Top = y - CONTACT_AREA_RADIUS,
					Bottom = y + CONTACT_AREA_RADIUS
				};
				_pointerTouchInfos[i] = pointerTouchInfo;

				Trace.WriteLine(string.Format("set cur {0} ({1}) {2} {3} {4} {5}", pid, tuioCursor.getSessionID(), x, y, tuioCursor.getMotionSpeed(), tuioCursor.getMotionAccel()), "TUIO");
			}	
		}

		public void RemoveTuioCursor(TuioCursor tuioCursor)
		{
			_refreshTimer.Stop();

			int pid = tuioCursor.getCursorID();

			int i = _pointerTouchInfos.FindIndex(pti => pti.PointerInfo.PointerId == pid);
			if (i != -1)
			{
				PointerTouchInfo pointerTouchInfo = _pointerTouchInfos[i];
				pointerTouchInfo.PointerInfo.PointerFlags = PointerFlags.UP;
				_pointerTouchInfos[i] = pointerTouchInfo;

				Trace.WriteLine(string.Format("del cur {0} ({1})", pid, tuioCursor.getSessionID()), "TUIO");
			}
		}

		public void AddTuioObject(TuioObject tuioObject) {}
		public void UpdateTuioObject(TuioObject tuioObject) { }
		public void RemoveTuioObject(TuioObject tuioObject) { }

		public void Refresh(TuioTime frameTime)
		{
			Trace.WriteLine(string.Format("refresh {0}", frameTime.getTotalMilliseconds()), "TUIO");

			_refreshTimer.Stop();

			if (this.IsContactEnabled && this.IsWindowsKeyPressEnabled)
			{
				if (_pointerTouchInfos.Count.Equals(this.WindowsKeyPressTouchCount))
				{
#pragma warning disable 4014
					InjectWindowsKeyPress();
#pragma warning restore 4014
					return;
				}
			}

			InjectPointerTouchInfos();

			if (_pointerTouchInfos.Count > 0)
			{
				for (int i = _pointerTouchInfos.Count - 1; i >= 0; i--)
				{
					if (_pointerTouchInfos[i].PointerInfo.PointerFlags.HasFlag(PointerFlags.UP))
					{
						_pointerTouchInfos.RemoveAt(i);
					}
				}

				if (_pointerTouchInfos.Count > 0)
				{
					for (int i = 0, ic = _pointerTouchInfos.Count; i < ic; i++)
					{
						if (_pointerTouchInfos[i].PointerInfo.PointerFlags.HasFlag(PointerFlags.DOWN))
						{
							PointerTouchInfo pointerTouchInfo = _pointerTouchInfos[i];
							pointerTouchInfo.PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INRANGE | ((this.IsContactEnabled) ? PointerFlags.INCONTACT : PointerFlags.NONE);
							_pointerTouchInfos[i] = pointerTouchInfo;
						}
					}

					_refreshTimer.Start();
				}
			}	
		}

		private void InjectPointerTouchInfos()
		{
			PointerTouchInfo[] pointerTouchInfos = _pointerTouchInfos.ToArray();

			if (pointerTouchInfos.Length == 0)
			{
				_refreshTimer.Stop();
			}

			if (!_isTouchInjectionSuspended)
			{
				TouchInjection.Send(pointerTouchInfos);
				this.OnTouchInjected(new TouchInjectedEventArgs(pointerTouchInfos));
			}
		}

		private async Task InjectWindowsKeyPress()
		{
			if (!_isTouchInjectionSuspended)
			{
				_isTouchInjectionSuspended = true;

				_pointerTouchInfos.Clear();
				InjectPointerTouchInfos();

				KeyboardInjection.Send(KeyboardInputSequence.WindowsKeyPress);

				await Task.Delay(TimeSpan.FromMilliseconds(750));

				_isTouchInjectionSuspended = false;

				_pointerTouchInfos.Clear();
				InjectPointerTouchInfos();
			}
		}
	}
}
