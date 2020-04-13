﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Net.Mail;
using System.Net;
using System.Windows.Forms;
using System.Timers;
using System.Reflection;

namespace DeleteDoubleSpaceBar
{
    class Program
    {

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static string strDerniereTouche = "";
        private static NotifyIcon notifyIcon1 = new NotifyIcon();

        public static void Main()
        {
            Icon appIcon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            notifyIcon1.Icon = appIcon;
            notifyIcon1.Visible = true;
            notifyIcon1.BalloonTipTitle = "Delete Double Space";
            notifyIcon1.BalloonTipText = "Delete Double Space is running in the background.";
            notifyIcon1.ShowBalloonTip(100);

            var handle = GetConsoleWindow();

            // Hide
            ShowWindow(handle, SW_HIDE);

            _hookID = SetHook(_proc);

            Application.Run();
            UnhookWindowsHookEx(_hookID);

        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static Stopwatch stopwatch = new Stopwatch();

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                stopwatch.Start();

                int vkCode = Marshal.ReadInt32(lParam);

                if ((Keys)vkCode == Keys.Space)
                {
                    stopwatch.Stop();
                    if (strDerniereTouche == "SPACE" && stopwatch.ElapsedMilliseconds < 500)
                    {
                        SendKeys.Send("{BACKSPACE}");
                        strDerniereTouche = "";
                        stopwatch.Reset();
                    }
                    else
                    {
                        strDerniereTouche = "SPACE";
                        stopwatch.Reset();
                    }
                }
                else
                {
                    strDerniereTouche = "";
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
    }
}
