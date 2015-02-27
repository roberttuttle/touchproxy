using System;
using System.Collections.Generic;

namespace OSC.Windows
{
	public class OSCBundle : OSCPacket
	{
		protected const string BUNDLE = "#bundle";
		
		private long _timestamp = 0;

		public OSCBundle() : this(0) {}

		public OSCBundle(long ts)
		{
			this.Address = BUNDLE;
			_timestamp = ts;
		}

		override protected void Pack()
		{
			List<object> data = new List<object>();

			AddBytes(data, PackString(this.Address));
			PadNull(data);
			AddBytes(data, PackLong(0));
			
			foreach (object value in this.Values)
			{
				if (value is OSCPacket)
				{
					byte[] bs = ((OSCPacket)value).BinaryData;
					AddBytes(data, PackInt(bs.Length));
					AddBytes(data, bs);
				}
			}

			byte[] bData = (byte[])Array.CreateInstance(typeof(byte), data.Count);
			for (int i = 0, ic = data.Count; i < ic; i++)
			{
				bData.SetValue((byte)data[i], i);
			}
			this.BinaryData = bData;
		}

		public static new OSCBundle Unpack(byte[] bytes, ref int start, int end)
		{
			string address = UnpackString(bytes, ref start);
			if (!address.Equals(BUNDLE)) return null;

			long timestamp = UnpackLong(bytes, ref start);
			OSCBundle bundle = new OSCBundle(timestamp);
			
			while (start < end)
			{
				int length = UnpackInt(bytes, ref start);
				bundle.Append(OSCPacket.Unpack(bytes, ref start, start + length));
			}

			return bundle;
		}

		override public void Append(object value)
		{
			if (value is OSCPacket) 
			{
				this.Values.Add(value);
			}
		}

		override public bool IsBundle() { return true; }
	}
}

