using System;
using System.Runtime.InteropServices;

namespace System.Windows.Input
{
    public static class TouchInjection
    {
		public static void Initialize(uint maxContacts, TouchFeedback touchFeedbackMode)
		{
			NativeMethods.InitializeTouchInjection(maxContacts, touchFeedbackMode);
		}

		public static void Send(PointerTouchInfo[] pointerTouchInfos)
		{
			NativeMethods.InjectTouchInput(pointerTouchInfos.Length, pointerTouchInfos);
		}

		internal static class NativeMethods
		{
			[DllImport("User32.dll")]
			internal static extern bool InitializeTouchInjection(uint maxCount = 256, TouchFeedback feedbackMode = TouchFeedback.DEFAULT);

			[DllImport("User32.dll")]
			internal static extern bool InjectTouchInput(int count, [MarshalAs(UnmanagedType.LPArray), In] PointerTouchInfo[] contacts);
		}
    }

	public delegate void TouchInjectedEventHandler(object sender, TouchInjectedEventArgs e);

	public class TouchInjectedEventArgs : EventArgs
	{
		public PointerTouchInfo[] PointerTouchInfos { get; private set; }

		public TouchInjectedEventArgs(PointerTouchInfo[] pointerTouchInfos)
		{
			this.PointerTouchInfos = pointerTouchInfos;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PointerTouchInfo
	{
		public PointerInfo PointerInfo;
		public TouchFlags TouchFlags;
		public TouchMask TouchMasks;
		public ContactArea ContactArea;
		public ContactArea ContactAreaRaw;
		public uint Orientation;
		public uint Pressure;

		public void Move(int x, int y)
		{
			PointerInfo.PtPixelLocation.X += x;
			PointerInfo.PtPixelLocation.Y += y;
			ContactArea.Left += x;
			ContactArea.Right += x;
			ContactArea.Top += y;
			ContactArea.Bottom += y;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PointerInfo
	{
		public PointerInputType PointerInputType;
		public uint PointerId;
		public uint FrameId;
		public PointerFlags PointerFlags;
		internal IntPtr SourceDevice;
		internal IntPtr WindowTarget;
		public PointerTouchPoint PtPixelLocation;
		public PointerTouchPoint PtPixelLocationRaw;
		public PointerTouchPoint PtHimetricLocation;
		public PointerTouchPoint PtHimetricLocationRaw;
		public uint Time;
		public uint HistoryCount;
		public uint InputData;
		public uint KeyStates;
		public ulong PerformanceCount;
		public PointerButtonChangeType ButtonChangeType;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct ContactArea
	{
		[FieldOffset(0)]
		public int Left;
		[FieldOffset(4)]
		public int Top;
		[FieldOffset(8)]
		public int Right;
		[FieldOffset(12)]
		public int Bottom;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PointerTouchPoint
	{
		public int X;
		public int Y;
	}

	public enum PointerInputType
	{
		POINTER = 0x00000001,
		TOUCH = 0x00000002,
		PEN = 0x00000003,
		MOUSE = 0x00000004
	};

	public enum PointerFlags
	{
		NONE = 0x00000000,
		NEW = 0x00000001,
		INRANGE = 0x00000002,
		INCONTACT = 0x00000004,
		FIRSTBUTTON = 0x00000010,
		SECONDBUTTON = 0x00000020,
		THIRDBUTTON = 0x00000040,
		OTHERBUTTON = 0x00000080,
		PRIMARY = 0x00000100,
		CONFIDENCE = 0x00000200,
		CANCELLED = 0x00000400,
		DOWN = 0x00010000,
		UPDATE = 0x00020000,
		UP = 0x00040000,
		WHEEL = 0x00080000,
		HWHEEL = 0x00100000
	}

	public enum PointerButtonChangeType
	{
		NONE,
		FIRSTBUTTON_DOWN,
		FIRSTBUTTON_UP,
		SECONDBUTTON_DOWN,
		SECONDBUTTON_UP,
		THIRDBUTTON_DOWN,
		THIRDBUTTON_UP,
		FOURTHBUTTON_DOWN,
		FOURTHBUTTON_UP,
		FIFTHBUTTON_DOWN,
		FIFTHBUTTON_UP
	}

    public enum TouchFeedback
    {
        DEFAULT = 0x1,
        INDIRECT = 0x2,
        NONE = 0x3
    }

    public enum TouchFlags
    { 
        NONE = 0x00000000
    }

    public enum TouchMask
    {
        NONE = 0x00000000,
        CONTACTAREA = 0x00000001,
        ORIENTATION = 0x00000002,
        PRESSURE = 0x00000004
    }
}
