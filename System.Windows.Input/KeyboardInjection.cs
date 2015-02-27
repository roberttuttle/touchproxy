using System;
using System.Runtime.InteropServices;

namespace System.Windows.Input
{
	public static class KeyboardInjection
	{
		public static void Send(KeyboardInputSequence keyboardInputSequence)
		{
			switch (keyboardInputSequence)
			{
				case KeyboardInputSequence.WindowsKeyPress:
					NativeMethods.SendInput
					(
						2,
						new InputInfo[] 
						{ 
							new InputInfo { Type = InputInfoType.KEYBOARD, Union = new InputUnion { KeyboardInput = new KeyboardInput { VirtualKeyCode = VirtualKeyCode.LWIN, Flags = KeyboardFlag.KEYDOWN } } },
							new InputInfo { Type = InputInfoType.KEYBOARD, Union = new InputUnion { KeyboardInput = new KeyboardInput { VirtualKeyCode = VirtualKeyCode.LWIN, Flags = KeyboardFlag.KEYUP } } }
						},
						Marshal.SizeOf(typeof(InputInfo))
					);
					break;
				default:
					break;
			}
		}

		internal static class NativeMethods
		{
			[DllImport("User32.dll")]
			internal static extern uint SendInput(uint count, [MarshalAs(UnmanagedType.LPArray), In] InputInfo[] inputs, int size);
		}
	}

	public enum KeyboardInputSequence
	{
		WindowsKeyPress
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct InputInfo
	{
		internal InputInfoType Type;
		internal InputUnion Union;
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct InputUnion
	{
		[FieldOffset(0)]
		internal MouseInput MouseInput;

		[FieldOffset(0)]
		internal KeyboardInput KeyboardInput;

		[FieldOffset(0)]
		internal HardwareInput HardwareInput;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct KeyboardInput
	{
		internal VirtualKeyCode VirtualKeyCode;
		internal ushort ScanCode;
		internal KeyboardFlag Flags;
		internal uint Time;
		internal IntPtr ExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct MouseInput
	{
		internal int X;
		internal int Y;
		internal uint MouseData;
		internal uint Flags;
		internal uint Time;
		internal IntPtr ExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct HardwareInput
	{
		internal uint Msg;
		internal ushort ParamL;
		internal ushort ParamH;
	}

	internal enum InputInfoType : uint
	{
		KEYBOARD = 1
	}

	internal enum VirtualKeyCode : ushort
	{
		LWIN = 0x5B
	}

	internal enum KeyboardFlag : uint
	{
		KEYDOWN = 0x0000,
		KEYUP = 0x0002,
	}
}
