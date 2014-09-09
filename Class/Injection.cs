/*
    Copyright (C) 2014 LeagueSharp

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

#region

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using LeagueSharp.Loader.Data;

#endregion

namespace LeagueSharp.Loader.Class
{
    public static class Injection
    {
        [ UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode) ]
        public delegate bool InjectDLLDelegate(int processId, string path);

        private static InjectDLLDelegate injectDLL;

        static Injection()
        {
            ResolveInjectDLL();
        }

        [ DllImport("kernel32.dll") ]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [ DllImport("kernel32.dll") ]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [ DllImport("user32.dll", CharSet = CharSet.Auto) ]
        public static extern IntPtr SendMessage(IntPtr hWnd,
            uint Msg,
            IntPtr wParam,
            ref COPYDATASTRUCT lParam);

        [ DllImport("user32.dll", SetLastError = true) ]
        private static extern IntPtr FindWindow(IntPtr ZeroOnly, string lpWindowName);

        private static void ResolveInjectDLL()
        {
            var hModule = LoadLibrary(Path.Combine(Directories.LibrariesDir, "LeagueSharp.Bootstrap.dll"));
            if (!(hModule != IntPtr.Zero))
            {
                return;
            }
            var procAddress = GetProcAddress(hModule, "_InjectDLL@8");
            if (!(procAddress != IntPtr.Zero))
            {
                return;
            }
            injectDLL =
                Marshal.GetDelegateForFunctionPointer(procAddress, typeof(InjectDLLDelegate)) as
                    InjectDLLDelegate;
        }

        public static IntPtr GetLeagueWnd()
        {
            return FindWindow(IntPtr.Zero, "League of Legends (TM) Client");
        }

        public static Process GetLeagueProcess()
        {
            var processesByName = Process.GetProcessesByName("League of Legends");
            if (processesByName.Length > 0)
            {
                return processesByName[0];
            }
            return null;
        }

        public static bool Pulse()
        {
            var leagueProcess = Injection.GetLeagueProcess();
            if (leagueProcess != null)
            {
                var flag = false;
                foreach (ProcessModule processModule in leagueProcess.Modules)
                {
                    if (processModule.ModuleName == "LeagueSharp.Core.dll")
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    var num = injectDLL(
                        leagueProcess.Id,
                        Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) +
                        "\\Assemblies\\System\\LeagueSharp.Core.dll")
                        ? 1
                        : 0;
                    return true;
                }
            }
            return false;
        }

        public static void LoadAssembly(IntPtr wnd, LeagueSharpAssembly assembly)
        {
            if (assembly.Type != AssemblyType.Library)
            {
                var str = string.Format("load \"{0}\"", Path.GetFileName(assembly.PathToBinary));
                var lParam = new COPYDATASTRUCT { cbData = 1, dwData = str.Length * 2 + 2, lpData = str };
                SendMessage(wnd, 74U, IntPtr.Zero, ref lParam);
            }
        }

        public static void UnloadAssembly(IntPtr wnd, LeagueSharpAssembly assembly)
        {
            if (assembly.Type != AssemblyType.Library)
            {
                var str = string.Format("unload \"{0}\"", Path.GetFileName(assembly.PathToBinary));
                var lParam = new COPYDATASTRUCT { cbData = 1, dwData = str.Length * 2 + 2, lpData = str };
                SendMessage(wnd, 74U, IntPtr.Zero, ref lParam);
            }
        }

        public static void SendConfig(IntPtr wnd, Config config)
        {
            var str = string.Format("{0}{1}{2}{3}", (config.Settings.GameSettings[0].SelectedValue == "True") ? (object)"1" : (object)"0",
                (config.Settings.GameSettings[1].SelectedValue == "True") ? (object)"1" : (object)"0",
                (config.Settings.GameSettings[3].SelectedValue == "True") ? "2" : "0",
                (config.Settings.GameSettings[2].SelectedValue == "True") ? (object)"1" : (object)"0");
            
            var lParam = new COPYDATASTRUCT()
            {
                cbData = 2,
                dwData = str.Length * 2 + 2,
                lpData = str
            };
            SendMessage(wnd, 74U, IntPtr.Zero, ref lParam);
        }

        public struct COPYDATASTRUCT
        {
            public int cbData;
            public int dwData;
            [ MarshalAs(UnmanagedType.LPWStr) ] public string lpData;
        }
    }
}