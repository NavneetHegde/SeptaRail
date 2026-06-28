using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GetTrainsFunction;

public class NextThreeTrainFunction
{
    private readonly ILogger<NextThreeTrainFunction> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public NextThreeTrainFunction(ILogger<NextThreeTrainFunction> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    [Function("NextThreeTrainFunction")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        _logger.LogInformation("GetRegionalRailRequest Start");

        try
        {
            //1. read incoming request and parse it
            using var streamReader = new StreamReader(req.Body, Encoding.UTF8);
            string? jsonContent = await streamReader.ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(jsonContent))
                return new BadRequestResult();

            ApiRequest? apiRequest = JsonSerializer.Deserialize<ApiRequest>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            _logger.LogInformation("Request received for :: From {From} To {To}", apiRequest?.From, apiRequest?.To);

            if (apiRequest is null || string.IsNullOrWhiteSpace(apiRequest.From) || string.IsNullOrWhiteSpace(apiRequest.To))
                return new BadRequestResult();

            //2. call septa api to fetch the latest details
            var responseMsg = await CallSeptaApi(apiRequest.From, apiRequest.To);

            List<ApiResponse>? apiResponse = JsonSerializer.Deserialize<List<ApiResponse>>(responseMsg);

            //3. response
            return new OkObjectResult(apiResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error :: Message : {Message}", ex);
            return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
        }
        finally
        {
            _logger.LogInformation("GetRegionalRailRequest End");
        }
    }

    /// <summary>
    /// Makes a rest call  to Septa api
    /// </summary>
    /// <param name="sourceStation">origination station name</param>
    /// <param name="destStation">Destination station name</param>
    /// <returns>Next three station timing with any delay</returns>
    private async Task<string> CallSeptaApi(string sourceStation, string destStation)
    {
        _logger.LogInformation("Calling Septa API start. from {Source} to {Dest}", sourceStation, destStation);

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
            _logger.LogInformation("Calling Septa API complete. Result {StatusCode}", response?.StatusCode);

            if (!response!.IsSuccessStatusCode)
            {
                throw new Exception($"{response.StatusCode} {response.ReasonPhrase} ");
            }

            return await response.Content.ReadAsStringAsync() ?? string.Empty;
        }
        catch
        {
            _logger.LogError("Error Calling Septa API complete");
            throw;
        }
        finally
        {
            _logger.LogInformation("Calling Septa API end. from {Source} to {Dest}", sourceStation, destStation);
        }
    }
}
