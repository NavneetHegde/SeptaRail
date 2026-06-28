#:sdk Aspire.AppHost.Sdk@13.4.6
#:package Aspire.Hosting.Azure.Functions@13.4.6
#:package Aspire.Hosting.Azure.AppContainers@13.4.6
#:project ../src/GetTrainsFunction/GetTrainsFunction.csproj
#:project ../src/ClientApp/ClientApp.csproj


var builder = DistributedApplication.CreateBuilder(args);

// Azure Container Apps environment hosts the Function when publishing/deploying to Azure.
builder.AddAzureContainerAppEnvironment("septa-env");

var gettrains = builder.AddAzureFunctionsProject<Projects.GetTrainsFunction>("gettrains");

var publicDevTunnel = builder.AddDevTunnel("devtunnel-public")
    .WithAnonymousAccess()
    //.WithReference(weatherApi.GetEndpoint("https"));

var mauiapp = builder.AddMauiProject("mauiapp", "../ClientApp/ClientApp.csproj");

mauiapp.AddWindowsDevice()
    .WithReference(gettrains);

builder.Build().Run();
