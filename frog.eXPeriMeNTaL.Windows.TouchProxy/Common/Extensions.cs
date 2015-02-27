using System;
using System.ComponentModel;

namespace frog.eXPeriMeNTaL.Windows.TouchProxy.Common
{
	public static class CommonExtensions
	{
		public static bool IsBetween(this int value, int startValue, int endValue)
		{
			return (value >= startValue && value <= endValue);
		}

		public static bool IsBetween(this double value, double startValue, double endValue)
		{
			return (value >= startValue && value <= endValue);
		}

		public static bool IsBetween(this char value, char startValue, char endValue)
		{
			return (value >= startValue && value <= endValue);
		}
	}

	public static class EnumExtensions
	{
		public static string GetDescriptionAttribute(this Enum value)
		{
			DescriptionAttribute da = Attribute.GetCustomAttribute(value.GetType().GetField(value.ToString()), typeof(DescriptionAttribute)) as DescriptionAttribute;
			return (da != null) ? da.Description : value.ToString();
		}
	}
}