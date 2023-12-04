using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace GetTrainsFunction;

public class NextThreeTrainFunction
{
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _httpClientFactory = null!;


    public NextThreeTrainFunction(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory)
    {
        _logger = loggerFactory.CreateLogger<NextThreeTrainFunction>();
        _httpClientFactory = httpClientFactory;
    }

    [Function("NextThreeTrainFunction")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation($"GetRegionalRailRequest Start");

        //1. read incoming request and parse it
        if (req == null)
            return req.CreateResponse(HttpStatusCode.BadRequest);

        try
        {
            using var streamReader = new StreamReader(req.Body, encoding: Encoding.UTF8);
            string? jsonContent = streamReader.ReadToEnd();
            if (string.IsNullOrWhiteSpace(jsonContent))
                return req.CreateResponse(HttpStatusCode.BadRequest);

            ApiRequest? apiRequest = JsonSerializer.Deserialize<ApiRequest>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            _logger.LogInformation($"Request received for :: From {apiRequest?.From} To  {apiRequest?.To}");

            if (apiRequest is null || string.IsNullOrWhiteSpace(apiRequest.From) || string.IsNullOrWhiteSpace(apiRequest.To))
                return req.CreateResponse(HttpStatusCode.BadRequest);

            //2. call septa api to fetch the latest details
            var FromStation = apiRequest?.From ?? "30th Street Station"; // default/fallback station
            var ToStation = apiRequest?.To ?? "30th Street Station";  // default/fallback station

            var responseMsg = await CallSeptaApi(apiRequest.From, apiRequest.To);

            List<ApiResponse>? apiResponse = JsonSerializer.Deserialize<List<ApiResponse>>(responseMsg);

            //3. response
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(apiResponse);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error :: Message : {ex}");
            return req.CreateResponse(HttpStatusCode.ServiceUnavailable);
        }
        finally
        {
            _logger.LogInformation($"GetRegionalRailRequest End");
        }
    }

    /// <summary>
    /// Makes a rest call  to Septa api
    /// </summary>
    /// <param name="log">ILogger</param>
    /// <param name="sourceStation">origination station name</param>
    /// <param name="destStation">Destination station name</param>
    /// <returns>Next three station timing with any delay</returns>
    private async Task<string> CallSeptaApi(string sourceStation, string destStation)
    {
        _logger.LogInformation($"Calling Septa API start. from {sourceStation} to {destStation}");

        // create the client
        using HttpClient client = _httpClientFactory.CreateClient("httpClient");

        try
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "NextThreeTrainFunction");

            // septa train api
            var uri = client.BaseAddress + $"{sourceStation}/{destStation}/3";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);

            HttpResponseMessage response = await client.SendAsync(request);
            _logger.LogInformation($"Calling Septa API complete. Result {response?.StatusCode}");

            return response?.Content.ReadAsStringAsync().Result ?? string.Empty;
        }
        catch
        {
            _logger.LogError($"Error Calling Septa API complete");
            throw;
        }
        finally
        {
            _logger.LogInformation($"Calling Septa API end. from {sourceStation} to {destStation}");
        }
    }
}
