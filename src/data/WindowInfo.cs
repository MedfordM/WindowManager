using System;
using System.Windows.Forms;
using static WindowManager.Constants;

namespace WindowManager.src.data
{
  internal class WindowInfo
  {
    public WindowPos WindowPos { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public ManagedScreen Screen { get; set; }
    public int Dpi { get; set; }

    public WindowInfo() 
    {
      this.WindowPos = new WindowPos();
      this.Width = 0;
      this.Height = 0;
      this.Screen = new ManagedScreen();
      this.Dpi = 0;
    }

    public WindowInfo(WindowPos windowPos, Screen screen, int dpi)
    {
      //if (windowPos.Left >= 1920 && windowPos.Left <= 1936)
      //{
      //  windowPos.Left = 1920;
      //} else if (windowPos.Left < 10)
      //{
      //  windowPos.Left = 0;
      //}

      //if (windowPos.Bottom >= 2144 && windowPos.Bottom <= 2176)
      //{
      //  windowPos.Bottom = 2160;
      //}

      //if (windowPos.Right >= 1920 && windowPos.Right <= 1936)
      //{
      //  windowPos.Right = 1920;
      //} else if (windowPos.Right >= 3826 && windowPos.Right <= 3856)
      //{
      //  windowPos.Right = 3840;
      //}
      this.WindowPos = windowPos;
      this.Width = Math.Abs(windowPos.Left - windowPos.Right);
      this.Height = Math.Abs(windowPos.Top - windowPos.Bottom);
      this.Screen = new ManagedScreen(screen);
      this.Dpi = dpi;
    }

    public override string? ToString()
    {
      return @$"
        {{
          Left:   {WindowPos.Left},
          Right:  {WindowPos.Right},
          Top:    {WindowPos.Top},
          Bottom: {WindowPos.Bottom},
          Width:  {Width},
          Height: {Height},
          Screen: {Screen.Screen.DeviceName},
          Dpi:    {Dpi}
        }}
      ";
    }
  }
}
