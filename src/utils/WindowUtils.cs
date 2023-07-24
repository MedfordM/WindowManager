using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WindowManager.src.data;
using Windows.ApplicationModel.Store;
using WindowsDesktop;
using static System.Net.Mime.MediaTypeNames;
using static WindowManager.Constants;

namespace WindowManager
{
  internal class WindowUtils
  {
    private SplitMode mode = SplitMode.Vertical;

    private static List<WindowApplication> FetchWindows()
    {
      List<WindowApplication> applications = new();
      EnumWindows(new CallBackPtr((hwnd, lParam) =>
      {
        int processId = 0;
        GetWindowThreadProcessId(hwnd, out processId);
        Process process = Process.GetProcessById(processId);
        if (process.MainWindowHandle == (IntPtr)0) return true;
        if (String.IsNullOrEmpty(process.MainWindowTitle)) return true;
        //if (!VirtualDesktop.IsCurrentVirtualDesktop(process.MainWindowHandle)) return true;

        var maxTitleLength = (int)GetWindowTextLength(hwnd);
        var builder = new StringBuilder(maxTitleLength + 1);
        GetWindowText(hwnd, builder, (uint)builder.Capacity);
        var title = builder.ToString();
        if (title.Length == 0 || String.IsNullOrEmpty(process.MainWindowTitle) || WindowBlacklist.Contains(title)) return true;

        int size = Marshal.SizeOf(typeof(Rect));
        GetWindowRect(hwnd, out WindowPos rect);
        if (rect.Equals(new WindowPos())) return true;

        var windowStyle = GetWindowLong(hwnd, -16);
        if ((windowStyle & (uint) WindowStyles.WS_VISIBLE) == 0) return true;
        if ((windowStyle & (uint) WindowStyles.WS_CAPTION) == 0) return true;
        if ((windowStyle & (uint) WindowStyles.WS_MAXIMIZEBOX) == 0) return true;

        Screen screen = Screen.FromHandle(process.MainWindowHandle);
        int dpi = GetDpiForWindow(process.MainWindowHandle);
        applications.Add(new WindowApplication(process, title, new WindowInfo(rect, screen, dpi)));
        return true;
      }), 0);

      return applications;
    }

    private static ManagedScreen GetActiveScreen()
    {
      var activeScreen = Screen.FromHandle(GetCurrentWindow().Process.MainWindowHandle);
      var managedScreen = new ManagedScreen(activeScreen);
      return managedScreen;
    }

    private static List<WindowApplication> FetchWindowsForScreen(ManagedScreen screen)
    {
      var allWindows = FetchWindows();
      return FetchWindows().Where(w => w.WindowInfo.Screen.Equals(screen)).ToList();
    }

    private static WindowApplication? FindNearestWindow()
    {
      WindowApplication activeApp = GetCurrentWindow();
      WindowApplication? nearestApp = null;
      List<WindowApplication> applications = FetchWindows();

      int activeTop = activeApp.WindowInfo.WindowPos.Top;
      int activeLeft = activeApp.WindowInfo.WindowPos.Left;

      int lowestDelta = int.MaxValue;
      foreach (var app in applications)
      {
        int appTop = app.WindowInfo.WindowPos.Top;
        int appBottom = app.WindowInfo.WindowPos.Bottom;
        int appLeft = app.WindowInfo.WindowPos.Left;
        int appRight = app.WindowInfo.WindowPos.Right;

        int delta = Math.Abs(activeTop - appTop) + Math.Abs(activeLeft - appLeft);
        if (lowestDelta > delta)
        {
          lowestDelta = delta;
          nearestApp = app;
        }
      }
      return nearestApp;
    }

    private static WindowApplication? FindNearestWindowInDirection(Key key)
    {
      MotionKeyDirection.TryGetValue(key, out Direction direction);
      WindowApplication activeApp = GetCurrentWindow();
      WindowApplication? nearestApp = null;

      var windowsOnCurrScreen = FetchWindowsForScreen(activeApp.WindowInfo.Screen).Where(w => !w.Title.Equals(activeApp.Title)).ToList();
      var adjacentScreens = activeApp.WindowInfo.Screen.AdjacentScreens;

      List<WindowApplication> applications = FetchWindows().Where(a => !a.Title.Equals(activeApp.Title)).ToList();
      activeApp.WindowInfo.Screen.AdjacentScreens.TryGetValue(direction, out Screen? adjScreen);

      foreach (var app in applications)
      {
        if (!app.WindowInfo.Screen.Equals(activeApp.WindowInfo.Screen) && !app.WindowInfo.Screen.Screen.Equals(adjScreen)) continue;

        switch (direction)
        {
          case Direction.Left:
            if (app.IsLeftOf(activeApp) && app.IsRightOf(nearestApp))
            {
              nearestApp = app;
            }
            else if (nearestApp != null && Math.Abs(app.WindowInfo.WindowPos.Left - nearestApp.WindowInfo.WindowPos.Left) <= 8)
            {
              int currentDelta = Math.Abs(app.WindowInfo.WindowPos.Top - activeApp.WindowInfo.WindowPos.Top);
              int nearestDelta = Math.Abs(nearestApp.WindowInfo.WindowPos.Top - activeApp.WindowInfo.WindowPos.Top);
              if (currentDelta < nearestDelta) nearestApp = app;
            }
            break;
          case Direction.Down:
            if (app.IsBelow(activeApp) && app.IsAbove(nearestApp))
            {
              nearestApp = app;
            }
            else if (nearestApp != null && Math.Abs(app.WindowInfo.WindowPos.Left - nearestApp.WindowInfo.WindowPos.Left) <= 8)
            {
              int currentDelta = Math.Abs(app.WindowInfo.WindowPos.Left - activeApp.WindowInfo.WindowPos.Left);
              int nearestDelta = Math.Abs(nearestApp.WindowInfo.WindowPos.Left - activeApp.WindowInfo.WindowPos.Left);
              if (currentDelta < nearestDelta) nearestApp = app;
            }
            break;
          case Direction.Up:
            if (app.IsAbove(activeApp) && app.IsBelow(nearestApp))
            {
              nearestApp = app;
            }
            else if (nearestApp != null && Math.Abs(app.WindowInfo.WindowPos.Left - nearestApp.WindowInfo.WindowPos.Left) <= 8)
            {
              int currentDelta = Math.Abs(app.WindowInfo.WindowPos.Left - activeApp.WindowInfo.WindowPos.Left);
              int nearestDelta = Math.Abs(nearestApp.WindowInfo.WindowPos.Left - activeApp.WindowInfo.WindowPos.Left);
              if (currentDelta < nearestDelta) nearestApp = app;
            }
            break;
          case Direction.Right:
            if (app.IsRightOf(activeApp) && app.IsLeftOf(nearestApp))
            {
              nearestApp = app;
            }
            else if (nearestApp != null && Math.Abs(app.WindowInfo.WindowPos.Right - nearestApp.WindowInfo.WindowPos.Right) <= 8)
            {
              int currentDelta = Math.Abs(app.WindowInfo.WindowPos.Top - activeApp.WindowInfo.WindowPos.Top);
              int nearestDelta = Math.Abs(nearestApp.WindowInfo.WindowPos.Top - activeApp.WindowInfo.WindowPos.Top);
              if (currentDelta < nearestDelta) nearestApp = app;
            }
            break;
        }
      }
      return nearestApp;
    }

    private static void SwapWindows(WindowApplication app1, WindowApplication app2)
    {
      int x1 = app1.WindowInfo.WindowPos.Left;
      int y1 = app1.WindowInfo.WindowPos.Top;
      int width1 = app1.WindowInfo.Width;
      int height1 = app1.WindowInfo.Height;
      int x2 = app2.WindowInfo.WindowPos.Left;
      int y2 = app2.WindowInfo.WindowPos.Top;
      int width2 = app2.WindowInfo.Width;
      int height2 = app2.WindowInfo.Height;

      SetWindowPos(app1.Process.MainWindowHandle, IntPtr.Zero, x2, y2, width2, height2, 0x0004 | 0x0040);
      SetWindowPos(app2.Process.MainWindowHandle, IntPtr.Zero, x1, y1, width1, height1, 0x0004 | 0x0040);
    }

    public static WindowApplication GetCurrentWindow()
    {
      WindowApplication currentApplication = new();
      IntPtr hwnd = GetForegroundWindow();
      GetWindowThreadProcessId(hwnd, out int processId);
      Process process = Process.GetProcessById(processId);
      var maxTitleLength = (int)GetWindowTextLength(hwnd);
      var builder = new StringBuilder(maxTitleLength + 1);
      GetWindowText(hwnd, builder, (uint)builder.Capacity);
      var title = builder.ToString();
      GetWindowRect(hwnd, out WindowPos rect);
      if (!rect.Equals(new WindowPos()))
      {
        Screen screen = Screen.FromHandle(process.MainWindowHandle);
        int dpi = GetDpiForWindow(process.MainWindowHandle);
        currentApplication = new WindowApplication(process, title, new WindowInfo(rect, screen, dpi));
      }
      return currentApplication;
    }

    public static void FocusWindow(WindowApplication app)
    {
      var hwnd = app.Process.Handle;

      if (IsWindow(hwnd))
      {
        SetForegroundWindow(hwnd);
      }
      else
      {
        hwnd = GetHandleFromTitle(app.Title);
        if (!SetForegroundWindow(hwnd)) SetForegroundWindow(app.Process.MainWindowHandle);
      }
    }

    public static void FocusNearestWindow()
    {
      var nearestWindow = FindNearestWindow();
      if (nearestWindow != null) FocusWindow(nearestWindow);
    }

    public static void FocusWindowInDirection(Key direction)
    {
      var nearest = FindNearestWindowInDirection(direction);
      if (nearest != null) FocusWindow(nearest);
    }

    public static IntPtr GetHandleFromTitle(String title)
    {
      IntPtr handle = IntPtr.Zero;
      EnumWindows((hwnd, lParam) =>
      {
        int length = (int)GetWindowTextLength(hwnd);
        if (length == 0) return true;

        StringBuilder sb = new StringBuilder(length + 1);
        GetWindowText(hwnd, sb, (uint)sb.Capacity);

        if (sb.ToString().Contains(title))
        {
          GetWindowThreadProcessId(hwnd, out int processId);
          Process process = Process.GetProcessById(processId);
          GetWindowRect(hwnd, out WindowPos rect);
          Screen screen = Screen.FromHandle(process.MainWindowHandle);
          int dpi = GetDpiForWindow(process.MainWindowHandle);
          handle = hwnd;
        }
        return true;
      }, 0);

      return handle;
    }

    public static void ExecuteFunction(KeyCombo keyCombo)
    {
      if (keyCombo.Key == KillWindowKey)
      {
        GetCurrentWindow().Process.Kill();
      }
      else if (keyCombo.Key == ArrangeWindowsKey)
      {
        ArrangeWindows();
      }
    }

    public static void MoveWindowInDirection(Key direction)
    {
      WindowApplication activeApp = GetCurrentWindow();
      var nearestApp = FindNearestWindowInDirection(direction);
      if (nearestApp != null) SwapWindows(activeApp, nearestApp);
    }

    public static void ArrangeWindows()
    {
      int gap = 30;
      ManagedScreen screen = GetCurrentWindow().WindowInfo.Screen;
      List<WindowApplication> apps = FetchWindowsForScreen(screen);
      if (apps.Count == 0)
      {
        return;
      }
      else if (apps.Count == 1)
      {
        var app = apps[0];
        var handle = GetHandleFromTitle(app.Title);
        var width = screen.Screen.WorkingArea.Width - (gap * 2);
        var height = screen.Screen.WorkingArea.Height - (gap * 2);
        SetWindowPos(handle, IntPtr.Zero, gap, gap, width, height, 0x0004 | 0x0040);
      }
      else if (apps.Count % 2 == 0)
      {
        for (int i = 0; i < apps.Count; i++)
        {
          WindowApplication app = apps[i];
          var handle = GetHandleFromTitle(app.Title);
          var x = gap;
          var y = gap;
          var width = (screen.Screen.WorkingArea.Width - (gap * 2)) / apps.Count;
          var height = (screen.Screen.WorkingArea.Height - (gap * 2)) / (apps.Count / 2);
          if (i % 2 == 1)
          {
            x = width + (30 * 2);
            //y = y * 2;
          }
          SetWindowPos(handle, IntPtr.Zero, x, y, width, height, 0x0004 | 0x0040);
        }
      }
      else
      {
        for (int i = 0; i < apps.Count; i++)
        {
          WindowApplication app = apps[i];
          var handle = GetHandleFromTitle(app.Title);
          var x = gap;
          var y = gap;
          var width = (screen.Screen.WorkingArea.Width - (gap * 2)) / apps.Count;
          var height = (screen.Screen.WorkingArea.Height - (gap * 2)) / (apps.Count / 2);
          if (i % 2 == 1)
          {
            x = width + 30;
            //y = y * 2;
          }
          else if (i % 2 == 0)
          {
            y = height + (30 * 2);
          }
          SetWindowPos(handle, IntPtr.Zero, x, y, width, height, 0x0004 | 0x0040);
        }
      }
    }

    #region ExternalFunctions
    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();
    
    [DllImport("user32.dll")]
    private static extern int EnumWindows(CallBackPtr callPtr, int lPar);
    public delegate bool CallBackPtr(IntPtr hwnd, int lParam);

    [DllImport("user32.dll")]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int processId);

    [DllImport("user32.dll")]
    static extern bool GetWindowRect(IntPtr hWnd, out WindowPos lpRect);

    [DllImport("user32.dll")]
    static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int width, int height, bool repaint);
    
    [DllImport("user32.dll")]
    static extern int GetDpiForWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    static extern int AdjustWindowRectExForDpi(WindowPos inWindowPos, out WindowPos outWindowPos, uint style, bool hasMenu, uint extendedStyle, int dpi);

    [DllImport("user32.dll", SetLastError = true)]
    static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("dwmapi.dll")]
    static extern int DwmGetWindowAttribute(IntPtr hwnd, DwmWindowAttribute att, out WindowPos pvAttribute, int cbAttribute);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);
    [DllImport("user32.dll")]
    private static extern uint GetWindowText(IntPtr hWnd, StringBuilder lpString, uint nMaxCount);

    [DllImport("user32.dll")]
    private static extern uint GetWindowTextLength(IntPtr hWnd);
    [DllImport("user32.dll")]
    private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern IntPtr FindWindowEx(IntPtr parentHandle, string windowTitle);
    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hWnd);
    #endregion
  }
}
