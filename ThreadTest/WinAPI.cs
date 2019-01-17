/*************************************************
  Copyright (C), 2010-2011, CQ Ebos. Co., Ltd.
  File name: WinAPI.cs    
  Version: 1.0   
  Description: 系统API
  Others:
  Function List:
*************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ThreadTest
{
    /// <summary>
    /// 系统API
    /// </summary>
    class WinAPI
    {
        /// <summary>
        /// 窗口隐藏
        /// </summary>
        public const int WS_HIDE = 0;
        /// <summary>
        /// 窗口处于正常状态
        /// </summary>
        public const int WS_SHOWNORMAL = 1;
        /// <summary>
        /// 窗口处于最小化状态
        /// </summary>
        public const int WS_SHOWMIN = 2;
        /// <summary>
        /// 窗口处于最大化状态
        /// </summary>
        public const int WS_SHOWMAX = 3;
        /// <summary>
        /// 最小化按钮
        /// </summary>
        private const int SC_MINIMIZE = 0xF020;
        /// <summary>
        /// 最大化按钮
        /// </summary>
        private const int SC_MAXIMIZE = 0xF030;
        /// <summary>
        /// 关闭按钮
        /// </summary>
        public const int SC_CLOSE = 0xF060;
        /// <summary>
        /// 对象可用
        /// </summary>
        public const int MF_ENABLED = 0x00000000;
        /// <summary>
        /// 置灰标识
        /// </summary>
        public const int MF_GRAYED = 0x00000001;
        /// <summary>
        /// 对象禁用
        /// </summary>
        public const int MF_DISABLED = 0x00000002;

        /// <summary>
        /// 设置UTC时间
        /// </summary>
        /// <param name="sysTime">时间</param>
        [DllImport("Kernel32.dll")]
        public static extern bool SetSystemTime(ref SystemTime sysTime);
        /// <summary>
        /// 设置本地时间
        /// </summary>
        /// <param name="sysTime">时间</param>
        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern bool SetLocalTime(ref SystemTime sysTime);
        /// <summary>
        /// 获取UTC时间
        /// </summary>
        /// <param name="sysTime">时间</param>
        [DllImport("Kernel32.dll")]
        public static extern void GetSystemTime(ref SystemTime sysTime);
        /// <summary>
        /// 获取本地时间
        /// </summary>
        /// <param name="sysTime">时间</param>
        [DllImport("Kernel32.dll")]
        public static extern void GetLocalTime(ref SystemTime sysTime);

        [DllImport("User32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
        [DllImport("User32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern int FindWindow(string className, string titleName);
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindowEx(string className, string titleName);

        [DllImportAttribute("user32.dll")]
        public static extern bool AnimateWindow(IntPtr hwnd, int dwTime, int dwFlags);
        /// <summary>
        /// 获取对象句柄
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "GetSystemMenu")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, int bRevert);
        /// <summary>
        /// 可用性设置
        /// </summary>
        [DllImport("User32.dll")]
        public static extern bool EnableMenuItem(IntPtr hMenu, int uIDEnableItem, int uEnable);

        public const int USER = 0x0400;
        public const int UM_1 = USER + 1;
        [DllImport("user32.dll")]
        public static extern void PostMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr wnd, int msg, IntPtr wP, IntPtr lP);
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(
            int hWnd, // handle to destination window
            int Msg, // message
            int wParam, // first message parameter
            ref COPYDATASTRUCT lParam // second message parameter
        );
        public const int WM_COPYDATA = 0x004A;

        [DllImport("Iphlpapi.dll")]
        public static extern int SendARP(Int32 dest, Int32 host, ref Int64 mac, ref Int32 length);
        [DllImport("Ws2_32.dll")]
        public static extern Int32 inet_addr(string ip);
    }
    public struct COPYDATASTRUCT
    {
        public IntPtr dwData;
        public int cbData;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpData;

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SystemTime
    {
        public ushort wYear;
        public ushort wMonth;
        public ushort wDayOfWeek;
        public ushort wDay;
        public ushort wHour;
        public ushort wMinute;
        public ushort wSecond;
        public ushort wMiliseconds;
    }

    public enum AnimateEffect
    {
        /// <summary>
        /// 从左到右
        /// </summary>
        AW_HOR_POSITIVE = 0x00000001,
        /// <summary>
        /// 自右向左
        /// </summary>
        AW_HOR_NEGATIVE = 0x00000002,
        /// <summary>
        /// 从上到下
        /// </summary>
        AW_VER_POSITIVE = 0x00000004,
        /// <summary>
        /// 从下到上
        /// </summary>
        AW_VER_NEGATIVE = 0x00000008,
        AW_CENTER = 0x00000010,
        AW_HIDE = 0x00010000,
        AW_ACTIVATE = 0x00020000,
        AW_SLIDE = 0x00040000,
        AW_BLEND = 0x00080000
    }
}
