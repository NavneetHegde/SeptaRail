using Microsoft.Maui.Controls;

namespace SeptaRail.ClientApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();

    }


}
