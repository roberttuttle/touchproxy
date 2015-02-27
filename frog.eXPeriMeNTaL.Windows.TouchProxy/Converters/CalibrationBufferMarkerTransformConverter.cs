using System;
using System.Globalization;
using System.Windows.Data;

namespace frog.eXPeriMeNTaL.Windows.TouchProxy.Converters
{
	public class CalibrationBufferMarkerTransformConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			try
			{
				double panelHeight = (double)values[0];
				double screenHeight = (double)values[1];
				double buffer = (double)values[2];
				bool isInverted = Boolean.Parse(values[3].ToString());

				return (panelHeight / screenHeight) * buffer * (isInverted ? -1 : 1);
			}
			catch
			{
				return 0;
			}
		}

		public object[] ConvertBack(object values, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
