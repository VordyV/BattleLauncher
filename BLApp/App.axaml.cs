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
        
        this.Launcher.RegisterGame(gameId: "bf2142", name: "Battlefield 2142", shortName: "BF2142");
        
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow(launcher: this.Launcher);
        }

        base.OnFrameworkInitializationCompleted();
    }
}