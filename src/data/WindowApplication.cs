using System;
using System.Diagnostics;
using static WindowManager.Constants;

namespace WindowManager.src.data
{
  internal class WindowApplication
  {
    public Process Process { get; set; }
    public String Title { get; set; }
    public WindowInfo WindowInfo { get; set; }


    public WindowApplication() 
    { 
      this.Process = new Process();
      this.Title = "";
      this.WindowInfo = new WindowInfo();
    }

    public WindowApplication(Process process, String windowTitle, WindowInfo windowInfo)
    {
      this.Process = process;
      this.Title = windowTitle;
      this.WindowInfo = windowInfo;
    }

    public override string ToString()
    {
      return @$"
        {{
          Id:     {Process.Id},
          Handle: {Process.Handle},
          Title:  {Title},
            {WindowInfo}
        }}
      ";
    }

    public bool IsLeftOf(WindowApplication? application)
    {
      if (application == null) return true;
      return WindowInfo.WindowPos.Left < application.WindowInfo.WindowPos.Left;
    }

    public bool IsRightOf(WindowApplication? application)
    {
      if (application == null) return true;
      return WindowInfo.WindowPos.Right > application.WindowInfo.WindowPos.Right;
    }

    public bool IsAbove(WindowApplication? application)
    {
      if (application == null) return true;
      return WindowInfo.WindowPos.Top < application.WindowInfo.WindowPos.Top;
    }

    public bool IsBelow(WindowApplication? application)
    {
      if (application == null) return true;
      return WindowInfo.WindowPos.Top > application.WindowInfo.WindowPos.Top;
    }
  }
}
