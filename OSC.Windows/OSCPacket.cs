using System;
using System.Collections.Generic;
using System.Text;

namespace OSC.Windows
{
	abstract public class OSCPacket
	{
		abstract public bool IsBundle();
		abstract public void Append(object value);
		abstract protected void Pack();

		public List<object> Values = new List<object>();

		public string Address;

		private byte[] _binaryData;
		public byte[] BinaryData
		{
			get
			{
				this.Pack();
				return _binaryData;
			}
			set
			{
				_binaryData = value;
			}
		}

		protected static byte[] PackInt(int value)
		{
			byte[] data = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian) { data = SwapEndian(data); }
			return data;
		}

		protected static byte[] PackLong(long value)
		{
			byte[] data = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian) { data = SwapEndian(data); }
			return data;
		}

		protected static byte[] PackFloat(float value)
		{
			byte[] data = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian) { data = SwapEndian(data); }
			return data;
		}

		protected static byte[] PackDouble(double value)
		{
			byte[] data = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian) { data = SwapEndian(data); }
			return data;
		}

		protected static byte[] PackString(string value)
		{
			return System.Text.Encoding.UTF8.GetBytes(value);
		}

		protected static int UnpackInt(byte[] bytes, ref int start)
		{
			byte[] data = new byte[4];
			for (int i = 0; i < 4; i++, start++) { data[i] = bytes[start]; }
			if (BitConverter.IsLittleEndian) { data = SwapEndian(data); }
			return BitConverter.ToInt32(data, 0);
		}

		protected static long UnpackLong(byte[] bytes, ref int start)
		{
			byte[] data = new byte[8];
			for (int i = 0; i < 8; i++, start++) { data[i] = bytes[start]; }
			if (BitConverter.IsLittleEndian) { data = SwapEndian(data); }
			return BitConverter.ToInt64(data, 0);
		}

		protected static float UnpackFloat(byte[] bytes, ref int start)
		{
			byte[] data = new byte[4];
			for (int i = 0; i < 4; i++, start++) { data[i] = bytes[start]; }
			if (BitConverter.IsLittleEndian) { data = SwapEndian(data); }
			return BitConverter.ToSingle(data, 0);
		}

		protected static double UnpackDouble(byte[] bytes, ref int start)
		{
			byte[] data = new byte[8];
			for (int i = 0; i < 8; i++, start++) { data[i] = bytes[start]; }
			if (BitConverter.IsLittleEndian) { data = SwapEndian(data); }
			return BitConverter.ToDouble(data, 0);
		}

		protected static string UnpackString(byte[] bytes, ref int start)
		{
			int count = 0;
			for (int index = start; bytes[index] != 0; index++, count++) { }
			string s = Encoding.UTF8.GetString(bytes, start, count);
			start += count + 1;
			start = (start + 3) / 4 * 4;
			return s;
		}

		public static OSCPacket Unpack(byte[] bytes)
		{
			int start = 0;
			return Unpack(bytes, ref start, bytes.Length);
		}

		public static OSCPacket Unpack(byte[] bytes, ref int start, int end)
		{
			if (bytes[start] == '#')
			{
				return OSCBundle.Unpack(bytes, ref start, end);
			}
			else
			{
				return OSCMessage.Unpack(bytes, ref start);
			}
		}

		protected static void AddBytes(List<object> data, byte[] bytes)
		{
			foreach (byte b in bytes)
			{
				data.Add(b);
			}
		}

		protected static void PadNull(List<object> data)
		{
			byte zero = 0;
			int pad = 4 - (data.Count % 4);
			for (int i = 0; i < pad; i++)
			{
				data.Add(zero);
			}
		}

		protected static byte[] SwapEndian(byte[] data)
		{
			byte[] swapped = new byte[data.Length];
			for (int i = data.Length - 1, j = 0; i >= 0; i--, j++)
			{
				swapped[j] = data[i];
			}
			return swapped;
		}
	}
}
