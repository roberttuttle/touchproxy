using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace frog.Windows.TouchProxy.Behaviors
{
	public static class TextBoxAutoScrollBehavior
	{
		public static readonly DependencyProperty ScrollOnTextChangedProperty = DependencyProperty.RegisterAttached("ScrollOnTextChanged", typeof(bool), typeof(TextBoxAutoScrollBehavior), new UIPropertyMetadata(false, OnScrollOnTextChanged));

		public static bool GetScrollOnTextChanged(DependencyObject dependencyObject)
		{
			return (bool)dependencyObject.GetValue(ScrollOnTextChangedProperty);
		}

		public static void SetScrollOnTextChanged(DependencyObject dependencyObject, bool value)
		{
			dependencyObject.SetValue(ScrollOnTextChangedProperty, value);
		}

		private static readonly Dictionary<TextBox, Capture> _associations = new Dictionary<TextBox, Capture>();

		private static void OnScrollOnTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			var textBox = dependencyObject as TextBox;
			if (textBox == null)
			{
				return;
			}
			bool oldValue = (bool)e.OldValue, newValue = (bool)e.NewValue;
			if (newValue.Equals(oldValue))
			{
				return;
			}
			if (newValue)
			{
				textBox.Loaded += TextBoxLoaded;
				textBox.Unloaded += TextBoxUnloaded;
			}
			else
			{
				textBox.Loaded -= TextBoxLoaded;
				textBox.Unloaded -= TextBoxUnloaded;
				if (_associations.ContainsKey(textBox))
				{
					_associations[textBox].Dispose();
				}
			}
		}

		private static void TextBoxLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			var textBox = (TextBox)sender;
			textBox.Loaded -= TextBoxLoaded;
			_associations[textBox] = new Capture(textBox);
		}

		private static void TextBoxUnloaded(object sender, RoutedEventArgs routedEventArgs)
		{
			var textBox = (TextBox)sender;
			_associations[textBox].Dispose();
			textBox.Unloaded -= TextBoxUnloaded;
		}

		private class Capture : IDisposable
		{
			private TextBox _textBox;

			public Capture(TextBox textBox)
			{
				_textBox = textBox;
				_textBox.TextChanged += OnTextBoxOnTextChanged;
			}

			private void OnTextBoxOnTextChanged(object sender, TextChangedEventArgs args)
			{
				_textBox.ScrollToEnd();
			}

			public void Dispose()
			{
				_textBox.TextChanged -= OnTextBoxOnTextChanged;
			}
		}

	}
}