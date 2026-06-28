using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SeptaRail.ClientApp.Services;
using SeptaRail.ClientApp.ViewModel;
using SeptaRail.ClientApp.Views.Pages;


namespace SeptaRail.ClientApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<IHttpsClientHandlerService, HttpsClientHandlerService>();
        builder.Services.AddSingleton<IRestService, RestService>();
        builder.Services.AddSingleton<INextTrainFunction, NextTrainFunction>();

        builder.Services.AddSingleton<HomePageViewModel>();
        builder.Services.AddSingleton<HomePage>();
        builder.Services.AddSingleton<AboutPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
