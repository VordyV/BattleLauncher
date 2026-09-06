using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BL;
using Irihi.Avalonia.Shared.Contracts;

namespace BLApp.Forms;

public partial class SimpInst : UserControl
{
    protected Game Game;
    
    public SimpInst()
    {
        InitializeComponent();
    }
    
    public SimpInst(Game game)
    {
        this.Game = game;
        InitializeComponent();
    }

    private void ButtonCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is IDialogContext ctx) ctx.Close();
    }

    protected void ShowError(string text)
    {
        this.TextBlockError.IsVisible = true;
        this.TextBlockError.Text = text;
    }

    private async void ButtonSave_OnClick(object? sender, RoutedEventArgs e)
    {
        string? path = this.PathPickerGamePath.SelectedPathsText;
        if (path == null)
        {
            this.ShowError("Path is not specified");
            return;
        }
        
        if (!this.Game.IdentifyGame(path))
        {
            this.ShowError("There is no folder with the game at that path");
            return;
        }
        
        this.Game.AddGameRegistry(path);

        try
        {
            ClientData client = await this.Game.GenerateDefaultClient();
            
            this.Game.SetReferenceClient(Launcher.UnknownClientId);
            this.Game.SetCurrentClient(Launcher.UnknownClientId);

            Dictionary<string, string> files = new();
            foreach (var file in client.Files)
            {
                files.Add(file, Launcher.UnknownClientId);
            }
        
            await this.Game.SetStateSnapshot(new StateSnapshot() {Files = files});
        }
        catch (Exception exception)
        {
            this.ShowError(exception.Message);
            Console.WriteLine(exception.StackTrace);
            return;
        }

        if (DataContext is IDialogContext ctx) ctx.Close();
    }
}