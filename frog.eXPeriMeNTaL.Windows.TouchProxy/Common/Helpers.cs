using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace frog.eXPeriMeNTaL.Windows.TouchProxy.Common
{
	public static class EnumHelper
	{
		public static Dictionary<T, string> ToDictionary<T>() where T : struct, IConvertible
		{
			if (typeof(T).BaseType != typeof(Enum))
			{
				throw new InvalidCastException();
			}
			return Enum.GetValues(typeof(T)).Cast<T>().ToDictionary(e => e, e => ((Enum)Enum.ToObject(typeof(T), e)).GetDescriptionAttribute());
		}
	}
}
