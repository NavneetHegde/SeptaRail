using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.ConfigureFunctionsWebApplication();

builder.Services.AddHttpClient("httpClient", client =>
{
    // Base URL is configurable per environment; HTTPS by default.
    var baseUrl = builder.Configuration["Septa:BaseUrl"]
                  ?? "https://www3.septa.org/hackathon/NextToArrive/";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("User-Agent", "SeptaRail-NextThreeTrainFunction");
});

builder.Build().Run();
