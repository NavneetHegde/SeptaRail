#:sdk Aspire.AppHost.Sdk@13.4.6
#:package Aspire.Hosting.Azure.Functions@13.4.6
#:package Aspire.Hosting.Azure.AppContainers@13.4.6
#:package Aspire.Hosting.DevTunnels@13.4.6
#:package Aspire.Hosting.Maui@13.4.6-preview.1.26319.6
#:project ../src/GetTrainsFunction
#:project ../src/ClientApp

var builder = DistributedApplication.CreateBuilder(args);

// Azure Container Apps environment hosts the Function when publishing/deploying to Azure.
builder.AddAzureContainerAppEnvironment("septa-env");

var gettrains = builder.AddAzureFunctionsProject<Projects.GetTrainsFunction>("gettrains")
    .WithHttpHealthCheck("/api/health");

var publicDevTunnel = builder.AddDevTunnel("devtunnel-public")
    .WithAnonymousAccess()
    .WithReference(gettrains.GetEndpoint("https"));

var mauiapp = builder.AddMauiProject("mauiapp", "../src/ClientApp/ClientApp.csproj");

mauiapp.AddiOSSimulator()
    .WithOtlpDevTunnel()
    .WithReference(gettrains, publicDevTunnel);

builder.Build().Run();
