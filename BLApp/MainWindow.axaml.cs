using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using BL;
using BLApp.Controls;
using BLApp.Views;
using Window = Avalonia.Controls.Window;

namespace BLApp;

public partial class MainWindow : Window
{
    protected bool IsMaximum = false;
    protected Launcher Launcher;
    protected ViewPresenter<Launcher> ViewPresenter;
    
    public MainWindow()
    {
        InitializeComponent();
        //this.DataContext = this;
    }
    
    public MainWindow(Launcher launcher)
    {
        this.Launcher = launcher;
        this.ViewPresenter = new ViewPresenter<Launcher>(this.Launcher, views: new()
        {
            {"main", (l, vp, arg) => new MainView(l, vp, arg)},
        });
        
        InitializeComponent();
        this.DataContext = this;
        
        this.MainContent.Content = this.ViewPresenter.Content;
        
        this.ViewPresenter.LoadView("main", this.Launcher.GetGames()[0]);
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
            //this.ImageIconMin.IsVisible = false;
            //this.ImageIconMax.IsVisible = true;
        }
        else
        {
            this.WindowState = WindowState.Normal;
            //this.ImageIconMin.IsVisible = true;
            //this.ImageIconMax.IsVisible = false;
        }
        
        this.IsMaximum = !this.IsMaximum;
    }

    private void Window_OnClickClose(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
}