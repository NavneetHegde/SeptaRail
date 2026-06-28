using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.ConfigureFunctionsWebApplication();

builder.Services.AddHttpClient("httpClient", client =>
{
    client.BaseAddress = new Uri("http://www3.septa.org/hackathon/NextToArrive/");
});

builder.Build().Run();
