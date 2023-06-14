using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Uno.UI.Demo
{
	public partial class CodeBox : TextBox
	{
		public CodeBox()
		{
			DefaultStyleKey = typeof(CodeBox);

#if __WASM__ // Remove when Uno.UI automatically calls OnKeyDown(KeyRoutedEventArgs)
			KeyDown += (s, e) => OnKeyDown(e);
#endif
		}

		// Source: https://social.msdn.microsoft.com/Forums/windows/en-US/f31f34ed-8751-4792-99d6-7c080582d899/uwpxaml-make-a-multiline-textbox-accept-tab?forum=wpdevelop
		protected override void OnKeyDown(KeyRoutedEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.Key == Windows.System.VirtualKey.Tab)
			{
				var textBox = (TextBox)e.OriginalSource;
				var originalStartPosition = textBox.SelectionStart;

				// SelectionStart treats "\r\n" as a single character.
				// So if you've a TextBox with just the text "\r\n" and the cursor is at the end, SelectionStart is
				// - for a UWP-app: 1
				// - for a WPF-app: 2
				// => so for a UWP-app, we need to solve this:
				var startPosition = GetRealStartPositionTakingCareOfNewLines(originalStartPosition, textBox.Text);

				var beforeText = textBox.Text.Substring(0, startPosition);
				var afterText = textBox.Text.Substring(startPosition, textBox.Text.Length - startPosition);
				var tabSpaces = 4;
				var tab = new string(' ', tabSpaces);
				textBox.Text = beforeText + tab + afterText;
				textBox.SelectionStart = originalStartPosition + tabSpaces;
				textBox.SelectionLength = 0;

				e.Handled = true;
			}
		}

		private int GetRealStartPositionTakingCareOfNewLines(int startPosition, string text)
		{
			int newStartPosition = startPosition;
			int currentPosition = 0;
			bool previousWasReturn = false;
			foreach (var character in text)
			{
				if (character == '\n')
				{
					if (previousWasReturn)
					{
						newStartPosition++;
					}
				}

				if (newStartPosition <= currentPosition)
				{
					break;
				}

				if (character == '\r')
				{
					previousWasReturn = true;
				}
				else
				{
					previousWasReturn = false;
				}

				currentPosition++;
			}

			return newStartPosition;
		}
	}
}
