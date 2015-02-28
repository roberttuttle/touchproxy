using System;
using System.Windows;

namespace frog.Windows.TouchProxy
{
    public partial class App : Application
    {
		public App()
		{
			this.DispatcherUnhandledException += (s, e) =>
			{
				MessageBox.Show(string.Format("Error: DispatcherUnhandledException\r\n\r\n{0}\r\n\r\n{1}", e.Exception.Message, e.Exception.StackTrace));
			};
		}
    }
}
