#region

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using LeagueSharp.Loader.Data;

#endregion

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

namespace LeagueSharp.Loader.Class
{
    internal static class KeyboardHook
    {
        private static readonly KeyboardProc _proc = HookProc;
        private static IntPtr _hHook = IntPtr.Zero;

        public delegate void OnKeyUp(int vKeyCode);
        public static event OnKeyUp OnKeyUpTrigger;

        [ DllImport("user32.dll") ]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            KeyboardProc callback,
            IntPtr hInstance,
            uint threadId);

        [ DllImport("user32.dll") ]
        private static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [ DllImport("user32.dll") ]
        private static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, int wParam, IntPtr lParam);

        [ DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true) ]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public static void SetHook()
        {
            using (var curModule = Process.GetCurrentProcess().MainModule)
            {
                _hHook = SetWindowsHookEx(13, _proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        public static void UnHook()
        {
            if (_hHook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hHook);
            }
        }

        public static IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code >= 0 && wParam == (IntPtr)0x101)
            {

                if (OnKeyUpTrigger != null)
                {
                    OnKeyUpTrigger(Marshal.ReadInt32(lParam));
                }

            }

            return CallNextHookEx(_hHook, code, (int)wParam, lParam);
        }

        private delegate IntPtr KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    }
}