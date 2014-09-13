#region

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using LeagueSharp.Loader.Class;
using Microsoft.Win32;

#endregion

namespace LeagueSharp.Loader
{

    public partial class App : Application {
        private Mutex _mutex;

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            _mutex = new Mutex(true, @"LeagueSharp.Loader.Mutex", out createdNew);
            if (!createdNew)
            {
                if (e.Args.Length > 0)
                {
                    var wnd = Injection.FindWindow(IntPtr.Zero, "LeagueSharp");
                    if (wnd != IntPtr.Zero)
                    {
                        Clipboard.SetText(e.Args[0]);
                        ShowWindow(wnd, 5);
                        SetForegroundWindow(wnd);
                    }
                }

                _mutex = null;
                Environment.Exit(0);
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_mutex != null)
                _mutex.ReleaseMutex();
            base.OnExit(e);
        }
    }
}