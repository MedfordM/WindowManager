using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using WindowManager.src.data;
using static WindowManager.HotKeyUtils;

namespace WindowManager.src.utils
{
  internal class KeyboardHook
  {
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYUP = 0x0101;
    private const int WM_KEYDOWN = 0x0100;
    private readonly Key modKey;
    private readonly LowLevelKeyboardProc proc;
    private IntPtr _hookID = IntPtr.Zero;
    private KeyCombo keyCombo = new();

    public KeyboardHook()
    {
      proc = HookCallback;
      modKey = Key.LWin;
    }

    public void HookKeyboard()
    {
      _hookID = SetHook(proc);
    }

    public void UnhookKeyboard()
    {
      UnhookWindowsHookEx(_hookID);
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
      using Process curProcess = Process.GetCurrentProcess();
      using var curModule = curProcess.MainModule;
      if (curModule != null && curModule.ModuleName != null)
      {
        return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
      } 
      else
      {
        return IntPtr.Zero;
      }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
      Thread workerThread = new(HandleHotkey);
      if (!workerThread.IsAlive) keyCombo.Key = null;
      if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
      {
        int vkCode = Marshal.ReadInt32(lParam);
        var key = KeyInterop.KeyFromVirtualKey(vkCode);
        if (keyCombo.Modifiers == null && key.Equals(modKey))
        {
          keyCombo.AddModifier(key);
        }
        else if (keyCombo.Modifiers != null && Constants.ModifierKeys.Contains(key))
        {
          keyCombo.AddModifier(key);
          return (IntPtr)1;
        }
        else if (keyCombo.Modifiers != null && Constants.AllKeys.Contains(key))
        {
          keyCombo.Key = key;
          workerThread.Start(keyCombo);
          return (IntPtr)1;
        }
      } 
      else if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP && keyCombo.Key == null)
      {
        int vkCode = Marshal.ReadInt32(lParam);
        var key = KeyInterop.KeyFromVirtualKey(vkCode);
        if (key.Equals(modKey))
        {
          keyCombo = new();
        }
        else if (keyCombo.Modifiers != null && keyCombo.Modifiers.Contains(key))
        {
          keyCombo.RemoveModifier(key);
          return (IntPtr)1;
        }
      }
      return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

  }
}
