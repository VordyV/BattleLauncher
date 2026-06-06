using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BL;
using BLApp.Controls;
using BLApp.Forms;
using Ursa.Controls;

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
        this.Loaded += async (sender, args) => await this.OnLoaded();
        this.Game = launcher.GetGame((string)arg);

        this.Game.OnChangeStatus += this.UpdateButtonMA;
        
        this.Pages = new ViewPresenter<Launcher>(arg: this.Launcher, new()
        {
            {"general", (l, vp, arg) => new MainGeneralView(l, vp, arg)}
        });
        
        InitializeComponent();
        this.DataContext = this;

        this.MainContent.Content = this.Pages.Content;
        this.Pages.ShowView("general", this.Game.Id);
    }

    protected async Task OnLoaded()
    {
        if (await this.Game.CheckInstall() && !this.Game.CheckPathInstall()) Console.WriteLine("Game not found at the installed path");
        Console.WriteLine(this.Game.Status);
        if (this.Game.IsInstall)
        { 
            // TODO: Fix the program crash if there is no 'client' option in the config section
            this.Game.GetCurrentClient();
            await this.UpdateClients(await this.Game.GetClients());
        }
        //await this.UpdateButtonMA(this.Game.Status);
    }

    protected async Task UpdateClients(ClientData[] clients)
    {
        this.ComboBoxClients.Items.Clear();
        short i = -1;
        foreach (var client in clients)
        {
            this.ComboBoxClients.Items.Add(new ComboBoxItem() {Content = client.Name, Name = $"ComboBoxItemClients_{client.ID}"});
            i++;
        }

        this.ComboBoxClients.SelectedIndex = i;
    }

    protected async Task UpdateButtonMA(GameStatus status)
    {
        switch (status)
        {
            case GameStatus.NotInstalled:
            {
                this.Button_MainAction.Content = "УСТАНОВИТЬ ИГРУ";
                break;
            }
            case GameStatus.NotRunning:
            {
                this.Button_MainAction.Content = "ЗАПУСТИТЬ ИГРУ";
                break;
            }
            case GameStatus.Running:
            {
                this.Button_MainAction.Content = "ИГРАЕТ";
                break;
            }
            case GameStatus.Stopping:
            {
                this.Button_MainAction.Content = "ОСТАНАВЛИВАЕТСЯ";
                break;
            }
        }
    }

    protected async void Button_MainAction_OnClick(object? sender, RoutedEventArgs e)
    {
        switch (this.Game.Status)
        {
            case GameStatus.NotInstalled:
            {
                var context = new DialogContext();
                await OverlayDialog.ShowCustomModal<SimpInst>( new SimpInst(this.Game) {DataContext = context}, context, hostId: "main", new OverlayDialogOptions() {CanResize = false});
                await this.Game.CheckInstall();
                break;
            }
            case GameStatus.NotRunning:
            {
                
                break;
            }
            case GameStatus.Running:
            {
                
                break;
            }
            case GameStatus.Stopping:
            {
                
                break;
            }
        }
    }
}