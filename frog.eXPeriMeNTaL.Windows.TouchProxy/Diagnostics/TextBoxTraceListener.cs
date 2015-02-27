using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Threading;

namespace frog.eXPeriMeNTaL.Windows.TouchProxy.Diagnostics
{
	public class TextBoxTraceListener : TraceListener
	{
		public TextBox TextBox { get; private set; }
		public string Category { get; private set; }

		public TextBoxTraceListener(TextBox textBox, string category)
		{
			this.TextBox = textBox;
			this.Category = category;

			Initialize();
		}

		public override void Write(string message) {}

		public override void WriteLine(string message) {}

		public override void WriteLine(string message, string category)
		{
			if (category.Equals(this.Category))
			{
				this.TextBox.Dispatcher.BeginInvoke(new Action(() =>
				{
					this.TextBox.AppendText(message + Environment.NewLine);
					if (this.TextBox.LineCount >= this.TextBox.MaxLines)
					{
						this.TextBox.Clear();
					}
				}), DispatcherPriority.Background);
			}
		}

		private void Initialize()
		{
			this.TextBox.Dispatcher.BeginInvoke(new Action(() =>
			{
				if (this.TextBox.LineCount > 1)
				{
					this.TextBox.AppendText(Environment.NewLine);
				}
				this.TextBox.AppendText(string.Format("{0} @ {1} ({2})\n=============================================", this.Category, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff"), DateTime.Now.Ticks) + Environment.NewLine);
			}), DispatcherPriority.Background);
		}
	}
}
