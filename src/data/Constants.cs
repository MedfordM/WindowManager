using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace WindowManager
{
  internal class Constants
  {
    public static readonly List<Key> WindowFunctionKeys = new()
    {
      Key.Q, // Kill Window
      Key.A  // Arrange Windows on Monitor
    };

    public static readonly Key KillWindowKey = WindowFunctionKeys.ElementAt(0);
    public static readonly Key ArrangeWindowsKey = WindowFunctionKeys.ElementAt(1);


    public static readonly List<Key> WorkspaceKeys = new()
    {
      Key.D1,
      Key.D2,
      Key.D3,
      Key.D4,
      Key.D5
    };

    public static readonly List<Key> MotionKeys = new()
    {
      Key.H,
      Key.N,
      Key.E,
      Key.I
    };

    public enum Direction
    {
      Left,
      Down,
      Up,
      Right
    };

    public static readonly Dictionary<Key, Direction> MotionKeyDirection = new()
    {
      { MotionKeys.ElementAt(0), Direction.Left },
      { MotionKeys.ElementAt(1), Direction.Down },
      { MotionKeys.ElementAt(2), Direction.Up },
      { MotionKeys.ElementAt(3), Direction.Right }
    };

    public static readonly List<Key> ModifierKeys = new()
    {
      Key.LWin,
      Key.RWin,
      Key.LeftShift,
      Key.RightShift,
      Key.LeftCtrl,
      Key.RightCtrl,
      Key.LeftAlt,
      Key.RightAlt
    };

    public static readonly List<Key> AllKeys = WindowFunctionKeys.Concat(WorkspaceKeys).Concat(MotionKeys).ToList();


    public enum SplitMode
    {
      Horizontal,
      Vertical
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WindowPos
    {
      public int Left;   // x start
      public int Top;    // y start
      public int Right;  // x end
      public int Bottom; // y end
    }

    public enum DwmWindowAttribute : uint
    {
      NCRenderingEnabled = 1,
      NCRenderingPolicy,
      TransitionsForceDisabled,
      AllowNCPaint,
      CaptionButtonBounds,
      NonClientRtlLayout,
      ForceIconicRepresentation,
      Flip3DPolicy,
      ExtendedFrameBounds,
      HasIconicBitmap,
      DisallowPeek,
      ExcludedFromPeek,
      Cloak,
      Cloaked,
      FreezeRepresentation,
      PassiveUpdateMode,
      UseHostBackdropBrush,
      UseImmersiveDarkMode = 20,
      WindowCornerPreference = 33,
      BorderColor,
      CaptionColor,
      TextColor,
      VisibleFrameBorderThickness,
      SystemBackdropType,
      Last
    }

    public enum WindowStyles : UInt64
    {
      WS_VISIBLE = 0x10000000L,
      WS_CAPTION = 0x00C00000L,
      WS_MAXIMIZEBOX = 0x00010000L
    }

    public static readonly List<String> WindowBlacklist = new()
    {
      "NVIDIA GeForce Overlay",
      "Windows Input Experience",
      "Settings",
      "Setup"
    };
  }
}
