using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using BL;
using Irihi.Avalonia.Shared.Contracts;
using Ursa.Controls;

namespace BLApp.Forms;

public partial class LaunchSettings : UserControl
{
    protected Game Game;
    protected ILaunchParam[] LaunchParams;
    protected Dictionary<string, Control> Controls = new();
    
    public LaunchSettings()
    {
        this.DataContext = this;

    }
    
    public LaunchSettings(Game game)
    {
        this.Game = game;
        this.LaunchParams = game.LaunchParams.Values.ToArray();
        InitializeComponent();
        this.DataContext = this;

        Grid grid = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("*, *")
        };
        
        this.MainContent.Content = grid;
        
        byte row = 0;
        byte col = 0;

        foreach (var param in this.LaunchParams)
        {
            Control control;

            if (param is LaunchParamBool)
            {
                control = new CheckBox { Content = param.Name };
                this.Controls.Add(param.Id, control);
            }
            else if (param is LaunchParamDict p)
            {
                List<ComboBoxItem> items = new();
                foreach (var key in p.Dictionary.Keys)
                {
                    items.Add(new ComboBoxItem() {Content = key});
                }
                
                control = new ComboBox() {ItemsSource = items, SelectedIndex = 0};
                this.Controls.Add(param.Id, control);
            }
            else
            {
                throw new InvalidOperationException($"Launch parameter '{param.Name}' type not specified");
            }
            
            this.EnsureRow(grid, row);
            
            if (param.FullFormat)
            {
                if (col != 0)
                {
                    row++;
                    col = 0;
                    this.EnsureRow(grid, row);
                }

                Grid.SetRow(control, row);
                Grid.SetColumn(control, 0);
                Grid.SetColumnSpan(control, 2);
                grid.Children.Add(control);

                row++;
                col = 0;
            }
            else
            {
                Grid.SetRow(control, row);
                Grid.SetColumn(control, col);
                grid.Children.Add(control);

                if (col == 0)
                {
                    col = 1;
                }
                else
                {
                    row++;
                    col = 0;
                }
            }
        }
    }
    
    private void EnsureRow(Grid grid, byte index)
    {
        while (grid.RowDefinitions.Count <= index)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }
    }
    

    private void ButtonCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is IDialogContext ctx) ctx.Close();
    }

    private void ButtonSave_OnClick(object? sender, RoutedEventArgs e)
    {
        Dictionary<string, object> parameters = new();

        foreach (var launchParam in this.LaunchParams)
        {
            if (!this.Controls.TryGetValue(launchParam.Id, out var control))
                continue;

            if (launchParam is LaunchParamBool)
            {
                parameters[launchParam.Id] =
                    ((CheckBox)control).IsChecked ?? false;
            }
            else if (launchParam is LaunchParamDict)
            {
                ComboBox combo = (ComboBox)control;

                if (combo.SelectedItem is ComboBoxItem item)
                {
                    parameters[launchParam.Id] =
                        item.Content?.ToString() ?? "";
                }
            }
        }
        
        string[] args = this.Game.BuildLaunchParams(parameters);
    }
}