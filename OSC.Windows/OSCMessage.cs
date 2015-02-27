using System;
using System.Collections.Generic;
using System.Text;

namespace OSC.Windows
{
	public class OSCMessage : OSCPacket
	{
		protected const char INTEGER	= 'i';
		protected const char FLOAT		= 'f';
		protected const char LONG		= 'h';
		protected const char DOUBLE		= 'd';
		protected const char STRING		= 's';
		protected const char SYMBOL		= 'S';

		private StringBuilder _typeTag = new StringBuilder(",");

		public OSCMessage(string address)
		{
			this.Address = address;
		}

		override public bool IsBundle() { return false; }

		override protected void Pack()
		{
			List<object> data = new List<object>();

			AddBytes(data, PackString(this.Address));
			PadNull(data);
			AddBytes(data, PackString(_typeTag.ToString()));
			PadNull(data);
			
			foreach (object value in this.Values)
			{
				if (value is int) AddBytes(data, PackInt((int)value));
				else if (value is long) AddBytes(data, PackLong((long)value));
				else if (value is float) AddBytes(data, PackFloat((float)value));
				else if (value is double) AddBytes(data, PackDouble((double)value));
				else if (value is string)
				{
					AddBytes(data, PackString((string)value));
					PadNull(data);
				}
				else { }
			}

			byte[] bData = (byte[])Array.CreateInstance(typeof(byte), data.Count);
			for (int i = 0, ic = data.Count; i < ic; i++)
			{
				bData.SetValue((byte)data[i], i);
			}
			this.BinaryData = bData;
		}

		public static OSCMessage Unpack(byte[] bytes, ref int start)
		{
			string address = UnpackString(bytes, ref start);
			OSCMessage msg = new OSCMessage(address);

			char[] tags = UnpackString(bytes, ref start).ToCharArray();
			foreach(char tag in tags)
			{
				if (tag == ',') continue;
				else if (tag == INTEGER) msg.Append(UnpackInt(bytes, ref start));
				else if (tag == LONG) msg.Append(UnpackLong(bytes, ref start));
				else if (tag == DOUBLE) msg.Append(UnpackDouble(bytes, ref start));
				else if (tag == FLOAT) msg.Append(UnpackFloat(bytes, ref start));
				else if (tag == STRING || tag == SYMBOL) msg.Append(UnpackString(bytes, ref start));
				else { }
			}

			return msg;
		}

		override public void Append(object value)
		{
			if (value is int)
			{
				AppendTag(INTEGER);
			}
			else if (value is long)
			{
				AppendTag(LONG);
			}
			else if (value is float)
			{
				AppendTag(FLOAT);
			}
			else if (value is double)
			{
				AppendTag(DOUBLE);
			}
			else if (value is string)
			{
				AppendTag(STRING);
			}
			else { }

			this.Values.Add(value);
		}

		protected void AppendTag(char typeTag)
		{
			_typeTag.Append(typeTag);
		}	
	}
}
