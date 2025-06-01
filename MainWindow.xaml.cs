using ReCall___.Model;
using ReCall___.ViewModel;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace ReCall___
{
    public partial class MainWindow : Window
    {
        public BoardManager BM { get; set; }

        // Хоткей константы
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
                Point pos = GetMousePosition();
                int border = 8;

                Rect windowRect = new Rect(this.Left, this.Top, this.Width, this.Height);

                if (pos.Y >= windowRect.Top && pos.Y < windowRect.Top + border)
                {
                    if (pos.X >= windowRect.Left && pos.X < windowRect.Left + border)
                    {
                        handled = true;
                        return (IntPtr)HTTOPLEFT;
                    }
                    else if (pos.X <= windowRect.Right && pos.X > windowRect.Right - border)
                    {
                        handled = true;
                        return (IntPtr)HTTOPRIGHT;
                    }
                    else
                    {
                        handled = true;
                        return (IntPtr)HTTOP;
                    }
                }
                else if (pos.Y <= windowRect.Bottom && pos.Y > windowRect.Bottom - border)
                {
                    if (pos.X >= windowRect.Left && pos.X < windowRect.Left + border)
                    {
                        handled = true;
                        return (IntPtr)HTBOTTOMLEFT;
                    }
                    else if (pos.X <= windowRect.Right && pos.X > windowRect.Right - border)
                    {
                        handled = true;
                        return (IntPtr)HTBOTTOMRIGHT;
                    }
                    else
                    {
                        handled = true;
                        return (IntPtr)HTBOTTOM;
                    }
                }
                else if (pos.X >= windowRect.Left && pos.X < windowRect.Left + border)
                {
                    handled = true;
                    return (IntPtr)HTLEFT;
                }
                else if (pos.X <= windowRect.Right && pos.X > windowRect.Right - border)
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
                this.DragMove();
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

        protected override void OnClosing ( CancelEventArgs e )
        {
            e.Cancel = true; // отменяем закрытие
            this.Hide();     // скрываем окно
        }

        protected override void OnClosed ( EventArgs e )
        {
            var handle = new WindowInteropHelper(this).Handle;
            UnregisterHotKey(handle, HOTKEY_ID);
            base.OnClosed(e);
        }

        #endregion

        #region Кнопки окна

        private void CloseWindow ( object sender, RoutedEventArgs e )
        {
            this.Close();
        }

        private void MaximizeWindow ( object sender, RoutedEventArgs e )
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        private void HideWindow ( object sender, RoutedEventArgs e )
        {
            this.WindowState = WindowState.Minimized;
        }

        #endregion

        private void ABSOLUTEClose ( object sender, RoutedEventArgs e )
        {
            this.Close();
        }

        private void ListBoxItem_MouseLeftButtonUp ( object sender, MouseButtonEventArgs e )
        {
            var item = (ListBoxItem)sender;
            var data = item.DataContext; // это элемент из NotesList
                                         // Тут твоя логика
            string selectedItem = data?.ToString();
            BM.ReUseNote(selectedItem);
        }

    }
}
