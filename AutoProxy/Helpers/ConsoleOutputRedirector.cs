using System;
using System.Text;
using System.IO;
using System.Windows.Controls;


namespace AutoProxy.Helpers
{
    public class ConsoleOutputRedirector : TextWriter
    {
        private readonly TextBlock _textBlock;
        private readonly ScrollViewer _scroller;

        public ConsoleOutputRedirector(TextBlock textBlock, ScrollViewer scroller)
        {
            _textBlock = textBlock;
            // Set the console output to this TextWriter
            Console.SetOut(this);
            _scroller = scroller;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine(string value)
        {
            // Append the string to the TextBlock
            AppendText(value + Environment.NewLine);
        }


        private void AppendText(string text)
        {
            // Use the dispatcher to update the TextBlock on the UI thread
            if (_textBlock.Dispatcher.CheckAccess())
            {
                DoUpdate(text); // Update directly if on the UI thread
            }
            else
            {
                _textBlock.Dispatcher.Invoke(() => DoUpdate(text)); // Use dispatcher if not on the UI thread
            }
        }

        private void DoUpdate(string text)
        {
            _textBlock.Text += text;
            _scroller.ScrollToBottom();
        }
    }

}
