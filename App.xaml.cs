#region

using System;
using System.Threading;
using System.Windows;
using LeagueSharp.Loader.Class;

#endregion

namespace LeagueSharp.Loader
{

    public partial class App : Application {
        private Mutex _mutex;

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
                        var str = e.Args[0];
                        if (str != null)
                        {
                            var lParam = new Injection.COPYDATASTRUCT { cbData = 1, dwData = str.Length * 2 + 2, lpData = str };
                            Injection.SendMessage(wnd, 74U, IntPtr.Zero, ref lParam);
                        }
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