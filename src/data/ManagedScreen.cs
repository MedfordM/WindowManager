using System.Collections.Generic;
using System.Windows.Forms;
using static WindowManager.Constants;

namespace WindowManager.src.data
{
  internal class ManagedScreen
  {
    public Screen Screen { get; }
    public Dictionary<Direction, Screen> AdjacentScreens { get; set; } = new();

    public ManagedScreen()
    {
      this.Screen = Screen.PrimaryScreen;
      this.AdjacentScreens = new();
    }

    public ManagedScreen(Screen screen) 
    {
      this.Screen = screen;
      List<Screen> allScreens = new(Screen.AllScreens);
      List<Screen> otherScreens = allScreens.FindAll(s  => s != screen);
      foreach (Screen otherScreen in otherScreens)
      {
        if (this.Screen.Bounds.Right == otherScreen.Bounds.Left)
        {
          AdjacentScreens.Add(Direction.Right, otherScreen);
        }
        else if (this.Screen.Bounds.Left == otherScreen.Bounds.Right)
        {
          AdjacentScreens.Add(Direction.Left, otherScreen);
        }
        else if (this.Screen.Bounds.Top == otherScreen.Bounds.Bottom)
        {
          AdjacentScreens.Add(Direction.Up, otherScreen);
        }
        else if (this.Screen.Bounds.Bottom == otherScreen.Bounds.Top)
        {
          AdjacentScreens.Add(Direction.Down, otherScreen);
        }
      }
    }

    public override bool Equals(object? obj)
    {
      if (obj is not ManagedScreen s) return false;
      return s.Screen.DeviceName.Equals(this.Screen.DeviceName);
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
  }
}
