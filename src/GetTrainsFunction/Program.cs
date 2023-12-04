using GetTrainsFunction;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddHttpClient("httpClient", client =>
        {
            client.BaseAddress = new Uri("http://www3.septa.org/hackathon/NextToArrive/");
        });
        services.AddTransient<NextThreeTrainFunction>();
    })
    .Build();

host.Run();
