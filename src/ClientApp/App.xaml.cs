using Microsoft.Maui.Controls;

namespace SeptaRail.ClientApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new Window(new AppShell());


}
