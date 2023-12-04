using SeptaRail.ClientApp.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace SeptaRail.ClientApp.Services;

public class RestService : IRestService
{
    HttpClient _client;
    JsonSerializerOptions _serializerOptions;
    IHttpsClientHandlerService _httpsClientHandlerService;

    public List<NextTrain> Items { get; private set; }

    public RestService(IHttpsClientHandlerService service)
    {
#if DEBUG
        _httpsClientHandlerService = service;
        HttpMessageHandler handler = _httpsClientHandlerService.GetPlatformMessageHandler();
        if (handler != null)
            _client = new HttpClient(handler);
        else
            _client = new HttpClient();
#else
        _client = new HttpClient();
#endif
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public async Task<List<NextTrain>> RefreshDataAsync(NextTrainRequest request)
    {
        Items = new List<NextTrain>();

        Uri uri = new Uri(string.Format(Constants.RestUrl));
        try
        {
            string json = JsonSerializer.Serialize<NextTrainRequest>(request, _serializerOptions);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync(uri, content);
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                Items = JsonSerializer.Deserialize<List<NextTrain>>(responseContent, _serializerOptions);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(@"\tERROR {0}", ex.Message);
        }

        return Items;
    }
}
