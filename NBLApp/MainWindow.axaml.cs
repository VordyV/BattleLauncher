using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using BL;
using BLApp.Controls;
using BLApp.Views;
using Window = Avalonia.Controls.Window;
using Notification = Ursa.Controls.Notification;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;
using Avalonia.Controls.Notifications;

namespace BLApp;

public partial class MainWindow : Window
{
    protected bool IsMaximum = false;
    protected Launcher Launcher;
    protected ViewPresenter<Launcher> ViewPresenter;
    protected WindowNotificationManager NotificationManager;
    
    public MainWindow()
    {
        InitializeComponent();
        //this.DataContext = this;
    }
    
    public MainWindow(Launcher launcher)
    {
        this.Loaded += async (sender, args) => await this.OnLoaded();
        
        this.Launcher = launcher;
        this.ViewPresenter = new ViewPresenter<Launcher>(this.Launcher, views: new()
        {
            {"main", (l, vp, arg) => new MainView(l, vp, arg)},
        });
        
        InitializeComponent();
        this.DataContext = this;
        this.NotificationManager = new WindowNotificationManager(this);
        this.NotificationManager.Position = NotificationPosition.BottomRight;
        Notify.Init(this.NotificationManager);
        
        this.MainContent.Content = this.ViewPresenter.Content;
        
        this.ViewPresenter.LoadView("main", this.Launcher.GetGames()[0]);

        this.PropertyChanged += async (s, e) =>
        {
            if (e.Property == WindowStateProperty)
            {
                var state = (WindowState)e.NewValue!;
                
                switch (WindowState)
                {
                    case WindowState.Maximized:
                    {
                        this.Window.SetMaximizedState();
                        break;
                    }
                    case WindowState.Minimized:
                    {
                        this.Window.SetMinimizedState();
                        break;
                    }
                    case WindowState.Normal:
                    {
                        this.Window.SetMinimizedState();
                        break;
                    }
                }
            }
        };
    }

    private async Task OnLoaded()
    {
        await this.Launcher.Registry.Read(createMissing: true);
    }

    private void Window_OnClickHide(object? sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void Window_OnClickMinMax(object? sender, RoutedEventArgs e)
    {
        if (!this.IsMaximum)
        {
            this.WindowState = WindowState.Maximized;
            this.Window.SetMaximizedState();
        }
        else
        {
            this.WindowState = WindowState.Normal;
            this.Window.SetMinimizedState();
        }
        
        this.IsMaximum = !this.IsMaximum;
    }

    private void Window_OnClickClose(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
}