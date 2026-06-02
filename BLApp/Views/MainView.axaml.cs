using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BL;
using BLApp.Controls;

namespace BLApp.Views;

public partial class MainView : View
{
    protected ViewPresenter<Launcher> Pages;
    protected Game Game;
    
    public MainView()
    {
        InitializeComponent();
    }
    
    public MainView(Launcher launcher, ViewPresenter<Launcher> viewPresenter, object? arg) : base(launcher, viewPresenter, arg)
    {
        this.Game = launcher.GetGame((string)arg);
        
        this.Pages = new ViewPresenter<Launcher>(arg: this.Launcher, new()
        {
            {"general", (l, vp, arg) => new MainGeneralView(l, vp, arg)}
        });
        
        InitializeComponent();
        this.DataContext = this;

        this.MainContent.Content = this.Pages.Content;
        this.Pages.ShowView("general", this.Game.Id);

        this.Loaded += (sender, args) =>
        {
            
        };
    }

    private async void Button_MainAction_OnClick(object? sender, RoutedEventArgs e)
    {
        
    }
}