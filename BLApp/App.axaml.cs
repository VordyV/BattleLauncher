using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using BL;

namespace BLApp;

public partial class App : Application
{
    public Launcher Launcher;
    
    public override void Initialize()
    {
        this.Launcher = new Launcher(pathConfig: "bnconfig.registry");

        ILaunchParam[] launchParams = new ILaunchParam[]
        {
            new LaunchParamBool() {Id = "fullscreen", Name = "Full screen"},
            new LaunchParamDict() {Id = "resolution", Name = "Resolution", Dictionary = new()
            {
                {"1920x1080", "+szx 1920 +szy 1080"},
                {"1280x720", "+szx 1280 +szy 720"}
            }}
            //new LaunchParamDict() {Id = "resolution", Name = "Resolution"}
        };
        this.Launcher.RegisterGame(gameId: "bf2142", name: "Battlefield 2142", shortName: "BF2142", determinants: new []{"bf2142.exe"}, launchParams: launchParams);
        
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow(launcher: this.Launcher);
        }

        base.OnFrameworkInitializationCompleted();
    }
}