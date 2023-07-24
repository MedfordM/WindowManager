using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WindowManager.src.data;
using WindowsDesktop;
using WindowsDesktop.Properties;
using static WindowManager.Constants;
using static WindowManager.WindowUtils;

namespace WindowManager
{
  internal class DesktopUtils
  {

    private static List<VirtualDesktop> desktops = new();

    public static void Initialize()
    {
      VirtualDesktop.Configure();
      desktops = VirtualDesktop.GetDesktops().ToList();
      while (desktops.Count < WorkspaceKeys.Count)
      {
        desktops.Add(VirtualDesktop.Create());
      }
    }

    public static void SwitchToDesktop(int index)
    {
      VirtualDesktop requestedDesktop = desktops.ElementAt(index);
      requestedDesktop?.Switch();
    }

    public static void MoveWindowToDesktop(int index)
    {
      VirtualDesktop.Configure();

      WindowApplication window = GetCurrentWindow();
      var handle = GetHandleFromTitle(window.Title);
      VirtualDesktop requestedDesktop = desktops.ElementAt(index);
      try
      {
        VirtualDesktop.MoveToDesktop(handle, requestedDesktop);
      }
      catch (Exception ex)
      {
        Trace.WriteLine(ex);
      }
    }
  }
}