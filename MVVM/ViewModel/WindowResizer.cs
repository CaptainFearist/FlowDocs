using System.Windows;
using System.Windows.Input;

namespace File_Manager.MVVM.ViewModel
{
    public class WindowResizer
    {
        private readonly Window _window;
        private bool isResizing = false;
        private Point lastMousePosition;

        public WindowResizer(Window window)
        {
            _window = window;
            _window.MouseDown += Window_MouseDown;
            _window.MouseMove += Window_MouseMove;
            _window.MouseUp += Window_MouseUp;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var mousePos = e.GetPosition(_window);

                if (mousePos.X >= _window.ActualWidth - 5)
                {
                    isResizing = true;
                    lastMousePosition = mousePos;
                    _window.Cursor = Cursors.SizeWE;
                }
                else if (mousePos.Y >= _window.ActualHeight - 5)
                {
                    isResizing = true;
                    lastMousePosition = mousePos;
                    _window.Cursor = Cursors.SizeNS;
                }
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (isResizing)
            {
                var mousePos = e.GetPosition(_window);
                if (_window.Cursor == Cursors.SizeWE)
                {
                    double widthDifference = mousePos.X - lastMousePosition.X;
                    _window.Width = Math.Max(_window.MinWidth, _window.Width + widthDifference);
                    lastMousePosition = mousePos;
                }
                else if (_window.Cursor == Cursors.SizeNS)
                {
                    double heightDifference = mousePos.Y - lastMousePosition.Y;
                    _window.Height = Math.Max(_window.MinHeight, _window.Height + heightDifference);
                    lastMousePosition = mousePos;
                }
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isResizing = false;
                _window.Cursor = Cursors.Arrow;
            }
        }
    }
}
