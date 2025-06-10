using ReCall___.ViewModel;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Diagnostics;
using TextCopy;

namespace ReCall___
{
    public partial class MainWindow : Window
    {
        public BoardManager BM { get; set; }

        private const int HOTKEY_ID = 9000;
        private const uint MOD_CONTROL = 0x2;
        private const uint MOD_SHIFT = 0x4;
        private const uint VK_R = 0x52;

        public MainWindow ()
        {
            InitializeComponent();
            BM = new BoardManager();
            this.DataContext = BM;
            _ = BM.Main();
        }

        #region Перетаскивание окна + ресайз
        private const int WM_NCHITTEST = 0x0084;
        private const int HTCLIENT = 1;
        private const int HTCAPTION = 2;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;

        private IntPtr WndProc ( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
        {
            if (msg == WM_NCHITTEST)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    handled = false;
                    return IntPtr.Zero;
                }

                Point pos = GetMousePosition();
                int resizeBorder = 8;

                Point windowPos = this.PointToScreen(new Point(0, 0));
                Rect windowRect = new Rect(windowPos.X, windowPos.Y, this.ActualWidth, this.ActualHeight);

                bool isLeft = pos.X >= windowRect.Left && pos.X < windowRect.Left + resizeBorder;
                bool isRight = pos.X <= windowRect.Right && pos.X > windowRect.Right - resizeBorder;
                bool isTop = pos.Y >= windowRect.Top && pos.Y < windowRect.Top + resizeBorder;
                bool isBottom = pos.Y <= windowRect.Bottom && pos.Y > windowRect.Bottom - resizeBorder;

                if (isTop && isLeft)
                {
                    handled = true;
                    return (IntPtr)HTTOPLEFT;
                }
                else if (isTop && isRight)
                {
                    handled = true;
                    return (IntPtr)HTTOPRIGHT;
                }
                else if (isBottom && isLeft)
                {
                    handled = true;
                    return (IntPtr)HTBOTTOMLEFT;
                }
                else if (isBottom && isRight)
                {
                    handled = true;
                    return (IntPtr)HTBOTTOMRIGHT;
                }
                else if (isTop)
                {
                    handled = true;
                    return (IntPtr)HTTOP;
                }
                else if (isBottom)
                {
                    handled = true;
                    return (IntPtr)HTBOTTOM;
                }
                else if (isLeft)
                {
                    handled = true;
                    return (IntPtr)HTLEFT;
                }
                else if (isRight)
                {
                    handled = true;
                    return (IntPtr)HTRIGHT;
                }
            }
            return IntPtr.Zero;
        }

        private Point GetMousePosition ()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos ( ref Win32Point pt );

        [StructLayout(LayoutKind.Sequential)]
        public struct Win32Point
        {
            public int X;
            public int Y;
        }

        private void Border_MouseDown ( object sender, MouseButtonEventArgs e )
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;

                    Point mousePos = GetMousePosition();
                    this.Left = mousePos.X - this.Width / 2;
                    this.Top = mousePos.Y - 30;
                }

                this.DragMove();
            }
        }

        private void Border_MouseDoubleClick ( object sender, MouseButtonEventArgs e )
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.WindowState = this.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            }
        }

        private void InitializeWindowBehavior ()
        {
            var source = PresentationSource.FromVisual(this) as HwndSource;
            source?.AddHook(WndProc);

            this.MinWidth = 300;
            this.MinHeight = 400;
        }

        protected override void OnClosed ( EventArgs e )
        {
            var source = PresentationSource.FromVisual(this) as HwndSource;
            source?.RemoveHook(WndProc);
            base.OnClosed(e);
        }
        #endregion

        #region Хоткей + скрытие окна при закрытии

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey ( IntPtr hWnd, int id, uint fsModifiers, uint vk );

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey ( IntPtr hWnd, int id );

        protected override void OnSourceInitialized ( EventArgs e )
        {
            base.OnSourceInitialized(e);

            IntPtr handle = new WindowInteropHelper(this).Handle;
            HwndSource source = HwndSource.FromHwnd(handle);
            source.AddHook(WndProc);
            source.AddHook(HwndHook);

            RegisterHotKey(handle, HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, VK_R);
        }

        private IntPtr HwndHook ( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                if (!this.IsVisible)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                    this.Activate();
                }
                else
                {
                    this.Hide();
                }
                handled = true;
            }
            return IntPtr.Zero;
        }
        #endregion

        #region Кнопки окна

        private void HideWindow ( object sender, RoutedEventArgs e )
        {
            this.Hide();
        }

        private void CloseWindow ( object sender, RoutedEventArgs e )
        {
            BM?.StopChecker();
            Application.Current.Shutdown();
        }

        public void ShowWindowFromTray ()
        {
            this.Show();
            this.Activate();
        }

        protected override void OnClosing ( CancelEventArgs e )
        {
            e.Cancel = true;
            this.Hide();
        }
        #endregion

        private void ListBoxItem_MouseLeftButtonUp ( object sender, MouseButtonEventArgs e )
        {
            var item = (ListBoxItem)sender;
            var data = item.DataContext;
            string selectedItem = data?.ToString();
        }

        private void GitHubOpener ( object sender, RoutedEventArgs e )
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/JasonKewnayFJ/ReCall",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть сайт: {ex.Message}");
            }
        }

        private void QuickPaste ( object sender, RoutedEventArgs e )
        {
            SearchBox.Text = ClipboardService.GetText();
        }


        private void CopyThis ( object sender, MouseButtonEventArgs e )
        {
            if (sender is TextBlock tb)
            {
                ClipboardService.SetText(tb.Text);
                CopySnackbar.MessageQueue?.Enqueue("Скопировано");

            }
        }

    }
}

