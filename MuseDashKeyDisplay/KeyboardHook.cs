using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MuseDashKeyDisplay
{
    public class KeyboardHook
    {
        private static int hHook;

        private static Win32Api.HookProc KeyboardHookDelegate;


        /// <summary>
        /// 安装键盘钩子
        /// </summary>
        public static void SetHook()
        {

            KeyboardHookDelegate = new Win32Api.HookProc(KeyboardHookProc);

            ProcessModule cModule = Process.GetCurrentProcess().MainModule;

            var mh = Win32Api.GetModuleHandle(cModule.ModuleName);

            hHook = Win32Api.SetWindowsHookEx(Win32Api.WH_KEYBOARD_LL, KeyboardHookDelegate, mh, 0);

        }

        /// <summary>
        /// 卸载键盘钩子
        /// </summary>
        public static void UnHook()
        {

            Win32Api.UnhookWindowsHookEx(hHook);

        }

        /// <summary>
        /// 获取键盘消息
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private static int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            // 如果该消息被丢弃（nCode<0
            if (nCode >= 0)
            {

                Win32Api.KeyboardHookStruct KeyDataFromHook = (Win32Api.KeyboardHookStruct)System.Runtime.InteropServices.Marshal.PtrToStructure(lParam, typeof(Win32Api.KeyboardHookStruct));

                int keyData = KeyDataFromHook.vkCode;

                //WM_KEYDOWN和WM_SYSKEYDOWN消息
                if (wParam == Win32Api.WM_KEYDOWN || wParam == Win32Api.WM_SYSKEYDOWN)
                {
                    // 此处触发键盘按下事件
                    // keyData为按下键盘的值,对应 虚拟码
                    Key key = KeyInterop.KeyFromVirtualKey(keyData);
                    MainWindow.instance.OnKeyDown(key);
                }

                //WM_KEYUP和WM_SYSKEYUP消息
                if (wParam == Win32Api.WM_KEYUP || wParam == Win32Api.WM_SYSKEYUP)
                {
                    // 此处触发键盘抬起事件
                    Key key = KeyInterop.KeyFromVirtualKey(keyData);
                    MainWindow.instance.OnKeyRelease(key);
                }

            }

            return Win32Api.CallNextHookEx(hHook, nCode, wParam, lParam);

        }
    }
}
