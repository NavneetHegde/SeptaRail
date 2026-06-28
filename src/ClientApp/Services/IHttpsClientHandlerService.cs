namespace SeptaRail.ClientApp.Services;

public interface IHttpsClientHandlerService
{
    HttpMessageHandler GetPlatformMessageHandler();
}
