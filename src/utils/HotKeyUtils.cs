using System;
using System.Collections.Generic;
using System.Windows.Input;
using WindowManager.src.data;
using static WindowManager.Constants;

namespace WindowManager
{
  internal class HotKeyUtils
  {
    public static void HandleHotkey(Object? obj)
    {
      if (obj == null) return;
      KeyCombo keyCombo = (KeyCombo)obj;
      if (keyCombo.Key == null || keyCombo.Modifiers == null)
      {
        return;
      }

      Key key = (Key) keyCombo.Key;
      List<Key> mods = new(keyCombo.Modifiers);
 
      if (WorkspaceKeys.Contains(key))
      {
        int desktopIndex = (int)key - 35;
        if (mods.Count == 1)
        {
          DesktopUtils.SwitchToDesktop(desktopIndex);
          WindowUtils.FocusNearestWindow();
        }
        else if (mods.Contains(Key.LeftShift))
        {
          DesktopUtils.MoveWindowToDesktop(desktopIndex);
          WindowUtils.FocusNearestWindow();
        }

      }
      else if (MotionKeys.Contains(key))
      {
        if (mods.Count == 1) WindowUtils.FocusWindowInDirection(key);
        else if (mods.Contains(Key.LeftShift)) WindowUtils.MoveWindowInDirection(key);
      }
      else if (WindowFunctionKeys.Contains(key))
      {
        WindowUtils.ExecuteFunction(keyCombo);
      }
    }
  }
}
