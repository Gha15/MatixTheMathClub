using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Web.WebView2.Core;

namespace MatixMathClub
{
    public partial class MainWindow : Window
    {
        // app.html lives in the 'app' folder next to the .exe (shared with the Electron build)
        private static readonly string AppFileName = "app.html";

        public MainWindow()
        {
            InitializeComponent();
            Loaded += async (s, e) => await StartAsync();
        }

        private string AppHtmlPath
        {
            get
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                return Path.Combine(baseDir, "app", AppFileName);
            }
        }

        private async Task StartAsync()
        {
            ShowSplash("Starting up...");

            string htmlPath = AppHtmlPath;
            if (!File.Exists(htmlPath))
            {
                ShowError(
                    "Couldn't find " + AppFileName + ".\n\nExpected it here:\n" + htmlPath +
                    "\n\nMake sure " + AppFileName + " is in the 'app' folder and that its " +
                    "properties are set to Content / Copy if newer.");
                return;
            }

            try
            {
                SplashText.Text = "Loading the workspace...";

                // Keep the browser profile in AppData so sign-in state persists
                // and so we never try to write into Program Files.
                string userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MatixMathClub", "WebView2");
                Directory.CreateDirectory(userDataFolder);

                CoreWebView2Environment env =
                    await CoreWebView2Environment.CreateAsync(null, userDataFolder);

                await Web.EnsureCoreWebView2Async(env);

                // Paint the gaps in the app's own colour while the browser
                // catches up with a resize, instead of flashing white.
                Web.DefaultBackgroundColor =
                    System.Drawing.Color.FromArgb(255, 247, 247, 249);

                CoreWebView2Settings settings = Web.CoreWebView2.Settings;
                settings.AreDefaultContextMenusEnabled = false;
                settings.IsStatusBarEnabled = false;
                settings.IsSwipeNavigationEnabled = false;
                settings.AreDevToolsEnabled = true; // press F12 if you ever need to debug

                // Let app.html drag the window and draw its own title bar.
                // Set through reflection so this compiles on every WebView2 SDK.
                try
                {
                    var nonClient = settings.GetType().GetProperty("IsNonClientRegionSupportEnabled");
                    if (nonClient != null) nonClient.SetValue(settings, true);
                }
                catch { }

                Web.CoreWebView2.WebMessageReceived += (s, args) =>
                {
                    string msg;
                    try { msg = args.TryGetWebMessageAsString(); }
                    catch { return; }
                    if (msg == "matix:minimize") WindowState = WindowState.Minimized;
                    else if (msg == "matix:maximize")
                        WindowState = (WindowState == WindowState.Maximized)
                            ? WindowState.Normal : WindowState.Maximized;
                    else if (msg == "matix:close") Close();
                };

                // Open target=_blank links in the same window instead of a popup
                Web.CoreWebView2.NewWindowRequested += (s, args) =>
                {
                    args.Handled = true;
                    Web.CoreWebView2.Navigate(args.Uri);
                };

                // Auto-grant camera/microphone so Math Chat calling works without
                // WebView2's own permission bar getting in the way.
                Web.CoreWebView2.PermissionRequested += (s, args) =>
                {
                    if (args.PermissionKind == CoreWebView2PermissionKind.Camera ||
                        args.PermissionKind == CoreWebView2PermissionKind.Microphone)
                    {
                        args.State = CoreWebView2PermissionState.Allow;
                    }
                };

                Web.CoreWebView2.ProcessFailed += (s, args) =>
                {
                    ShowError("The app's browser engine stopped unexpectedly (" +
                              args.ProcessFailedKind + "). Click Try again to restart it.");
                };

                Web.NavigationCompleted += (s, args) =>
                {
                    if (args.IsSuccess)
                    {
                        Splash.Visibility = Visibility.Collapsed;
                        ErrorPanel.Visibility = Visibility.Collapsed;
                        Web.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ShowError("The page failed to load (" + args.WebErrorStatus + ").");
                    }
                };

                Web.Source = new Uri(htmlPath);
            }
            catch (WebView2RuntimeNotFoundException)
            {
                ShowError(
                    "The Microsoft Edge WebView2 Runtime isn't installed on this PC.\n\n" +
                    "Install the free 'Evergreen Standalone Installer' from " +
                    "https://developer.microsoft.com/microsoft-edge/webview2/ and reopen Matix.");
            }
            catch (Exception ex)
            {
                ShowError("Something went wrong while starting up.\n\n" + ex.Message);
            }
        }

        private void ShowSplash(string message)
        {
            SplashText.Text = message;
            Splash.Visibility = Visibility.Visible;
            ErrorPanel.Visibility = Visibility.Collapsed;
            Web.Visibility = Visibility.Collapsed;
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorPanel.Visibility = Visibility.Visible;
            Splash.Visibility = Visibility.Collapsed;
            Web.Visibility = Visibility.Collapsed;
        }

        private async void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            await StartAsync();
        }

        // ===== custom title bar =====
        // The icons are vector paths drawn in XAML, not font glyphs, so they can
        // never fall back to empty boxes. One square = maximise, two overlapping
        // squares = restore.
        private const string PathMaximize = "M 0.5,0.5 H 9.5 V 9.5 H 0.5 Z";
        private const string PathRestore = "M 2.5,0.5 H 9.5 V 7.5 M 0.5,2.5 H 7.5 V 9.5 H 0.5 Z";

        // Tell the page whether we are maximised so it can swap the icon.
        private void NotifyMaximized()
        {
            try
            {
                if (Web == null || Web.CoreWebView2 == null) return;
                string v = (WindowState == WindowState.Maximized) ? "true" : "false";
                Web.CoreWebView2.ExecuteScriptAsync(
                    "window.mxSetMaximized && window.mxSetMaximized(" + v + ")");
            }
            catch { }
        }

        private void MinBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaxBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Maximized)
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Keep the middle button's icon honest: two squares when the window is
        // maximised (click to restore), one square when it isn't.
        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            NotifyMaximized();

            if (MaxBtn == null || MaxIcon == null)
            {
                return;
            }

            bool maximized = (WindowState == WindowState.Maximized);
            MaxIcon.Data = Geometry.Parse(maximized ? PathRestore : PathMaximize);
            MaxBtn.ToolTip = maximized ? "Restore" : "Maximise";
        }

        // ===== maximise fix =====
        // A WindowStyle="None" window maximises to the whole MONITOR, not the
        // usable work area, and Windows then pads it by the resize border. The
        // result: the top ~8px (our title bar) is pushed off the top of the
        // screen and the bottom is hidden behind the taskbar, so the page looks
        // cut off. Answering WM_GETMINMAXINFO with the work area fixes both.
        private const int WM_GETMINMAXINFO = 0x0024;
        private const int MONITOR_DEFAULTTONEAREST = 0x00000002;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            if (source != null)
            {
                source.AddHook(WindowProc);
            }

            FitToScreen();
        }

        // ===== small-screen fix =====
        // The window asks for 1400x900. On a smaller laptop screen (1366x768 and
        // 1280x800 are very common) a centred window that big hangs off every
        // edge -- and because the title bar is the top 36px of the window, it
        // ends up ABOVE the top of the screen where it cannot be seen or clicked,
        // while the page is clipped left, right and bottom.
        //
        // Shrink to fit the work area (screen minus taskbar) and re-centre.
        private void FitToScreen()
        {
            Rect work = SystemParameters.WorkArea;

            // Never let the minimums push us back off the screen either.
            MinWidth = Math.Min(MinWidth, work.Width);
            MinHeight = Math.Min(MinHeight, work.Height);

            // Leave a small margin so it still looks like a window.
            double maxWidth = Math.Max(MinWidth, work.Width - 40);
            double maxHeight = Math.Max(MinHeight, work.Height - 40);

            if (Width > maxWidth)
            {
                Width = maxWidth;
            }

            if (Height > maxHeight)
            {
                Height = maxHeight;
            }

            // Re-centre in the work area, clamped so the title bar can never sit
            // above the top of the screen.
            Left = work.Left + Math.Max(0, (work.Width - Width) / 2);
            Top = work.Top + Math.Max(0, (work.Height - Height) / 2);
        }

        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_GETMINMAXINFO)
            {
                ClampToWorkArea(hwnd, lParam);
                handled = true;
            }

            return IntPtr.Zero;
        }

        private static void ClampToWorkArea(IntPtr hwnd, IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            // Use whichever monitor the window is mostly on, so this still works
            // correctly on a second screen.
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                MONITORINFO mi = new MONITORINFO();
                mi.cbSize = Marshal.SizeOf(typeof(MONITORINFO));

                if (GetMonitorInfo(monitor, ref mi))
                {
                    RECT work = mi.rcWork;
                    RECT screen = mi.rcMonitor;

                    // Position and size are relative to the monitor's top-left.
                    mmi.ptMaxPosition.x = work.left - screen.left;
                    mmi.ptMaxPosition.y = work.top - screen.top;
                    mmi.ptMaxSize.x = work.right - work.left;
                    mmi.ptMaxSize.y = work.bottom - work.top;
                }
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, int flags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public int dwFlags;
        }
    }
}
