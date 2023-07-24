using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using WindowManager.src.utils;

namespace WindowManager
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {

    private KeyboardHook? keyboardHook;
    private WindowHook? windowHook;

    public MainWindow()
    {
      InitializeComponent();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
      base.OnSourceInitialized(e);
      keyboardHook = new KeyboardHook();
      keyboardHook.HookKeyboard();
      windowHook = new WindowHook();
      windowHook.HookWindows();
      DesktopUtils.Initialize();
      this.Visibility = Visibility.Hidden;
    }

    protected override void OnStateChanged(EventArgs e)
    {
      base.OnStateChanged(e);
      if (WindowState.Equals(WindowState.Minimized))
      {
        var trayIcon = (Hardcodet.Wpf.TaskbarNotification.TaskbarIcon)App.Current.MainWindow.FindName("NotifyIcon");
        trayIcon.Visibility = Visibility.Visible;
        this.Visibility = Visibility.Hidden;
      }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
      Process.GetCurrentProcess().Kill();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      keyboardHook?.UnhookKeyboard();
      windowHook?.UnhookWindows();
    }
  }

  public class TrayDoubleClick : ICommand
  {
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
      return true;
    }
    
    public void Execute(object? parameter)
    {
      var trayIcon = (Hardcodet.Wpf.TaskbarNotification.TaskbarIcon)App.Current.MainWindow.FindName("NotifyIcon");
      trayIcon.Visibility = Visibility.Hidden;
      App.Current.MainWindow.Visibility = Visibility.Visible;
    }
  }

}
