using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static WindowManager.Constants;
using static WindowManager.src.utils.KeyboardHook;

namespace WindowManager.src.utils
{
  internal class WindowHook
  {
    private const int WH_SHELL = 10;

    private delegate IntPtr WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
    private const uint EVENT_OBJECT_CREATE = 0x8000;
    private IntPtr hook;
    private readonly WinEventProc winEventCallback;

    public WindowHook() 
    {
      winEventCallback = WinEventCallback;
    }

    public void HookWindows()
    {
      hook = SetHook(winEventCallback);
    }

    private static IntPtr SetHook(WinEventProc callback)
    {
      return SetWinEventHook(EVENT_OBJECT_CREATE, EVENT_OBJECT_CREATE, IntPtr.Zero, callback, 0, 0, 0);
    }

    public void UnhookWindows()
    {
      UnhookWinEvent(hook);
    }

    private static IntPtr WinEventCallback(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
      if (eventType == EVENT_OBJECT_CREATE && hwnd != (IntPtr)null && idObject == 0 && idChild == 0)
      {

        int processId;
        GetWindowThreadProcessId(hwnd, out processId);
        Process process = Process.GetProcessById(processId);
        if (process.ProcessName != Process.GetCurrentProcess().ProcessName)
        {
          WindowUtils.ArrangeWindows();
        }
      }
      return IntPtr.Zero;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool UnhookWinEvent(IntPtr hWinEventHook);
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out WindowPos lpRect);
    [DllImport("user32.dll")]
    private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
  }
}
