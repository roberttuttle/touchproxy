using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using frog.eXPeriMeNTaL.Windows.TouchProxy.Common;

namespace frog.eXPeriMeNTaL.Windows.TouchProxy.Controls
{
	public class ConstrainedTextBox : TextBox
	{
		public static readonly DependencyProperty ConstraintPredicateProperty = DependencyProperty.Register("ConstraintPredicate", typeof(TextConstraintPredicateDelegate), typeof(ConstrainedTextBox));

		public TextConstraintPredicateDelegate ConstraintPredicate
		{
			get { return (TextConstraintPredicateDelegate)GetValue(ConstraintPredicateProperty); }
			set { SetValue(ConstraintPredicateProperty, value); }
		}

		public ConstrainedTextBox()
		{
			DataObject.AddPastingHandler(this, OnDataObjectPasting);
		}

		protected override void OnPreviewTextInput(TextCompositionEventArgs e)
		{
			base.OnPreviewTextInput(e);

			if (this.ConstraintPredicate != null)
			{
				string input = this.Text + e.Text;
				if (this.ConstraintPredicate(input))
				{
					return;
				}
				e.Handled = true;
			}
		}

		private void OnDataObjectPasting(object sender, DataObjectPastingEventArgs e)
		{
			if (this.ConstraintPredicate != null)
			{
				if (e.DataObject.GetDataPresent(typeof(string)))
				{
					string input = e.DataObject.GetData(typeof(string)) as string;
					if (this.ConstraintPredicate(input))
					{
						return;
					}
				}
				e.CancelCommand();
			}
		}
	}
}
