using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BL;
using BLApp.Controls;

namespace BLApp.Views;

public partial class MainGeneralView : View
{
    public MainGeneralView()
    {
        InitializeComponent();
    }
    
    public MainGeneralView(Launcher launcher, ViewPresenter<Launcher> viewPresenter, object? arg) : base(launcher, viewPresenter, arg)
    {
        InitializeComponent();
        //this.TextBlockName.Text = (string)arg;
    }
}