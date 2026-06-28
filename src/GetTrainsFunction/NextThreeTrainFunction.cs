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
    private const string SeptaClientName = "httpClient";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ILogger<NextThreeTrainFunction> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public NextThreeTrainFunction(ILogger<NextThreeTrainFunction> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    [Function("NextThreeTrainFunction")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetRegionalRailRequest Start");

        try
        {
            //1. read incoming request and parse it
            using var streamReader = new StreamReader(req.Body, Encoding.UTF8);
            string? jsonContent = await streamReader.ReadToEndAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(jsonContent))
                return Problem(StatusCodes.Status400BadRequest, "Request body is empty.");

            ApiRequest? apiRequest;
            try
            {
                apiRequest = JsonSerializer.Deserialize<ApiRequest>(jsonContent, SerializerOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Request body is not valid JSON.");
                return Problem(StatusCodes.Status400BadRequest, "Request body is not valid JSON.");
            }

            if (apiRequest is null || string.IsNullOrWhiteSpace(apiRequest.From) || string.IsNullOrWhiteSpace(apiRequest.To))
                return Problem(StatusCodes.Status400BadRequest, "Both 'from' and 'to' station names are required.");

            _logger.LogInformation("Request received for :: From {From} To {To}", apiRequest.From, apiRequest.To);

            //2. call septa api to fetch the latest details
            List<ApiResponse>? apiResponse;
            try
            {
                var responseMsg = await CallSeptaApi(apiRequest.From, apiRequest.To, cancellationToken);
                apiResponse = JsonSerializer.Deserialize<List<ApiResponse>>(responseMsg, SerializerOptions);
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException
                                       && !cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "SEPTA upstream call failed.");
                return Problem(StatusCodes.Status502BadGateway, "Unable to reach the SEPTA service. Please try again.");
            }

            //3. response
            return new OkObjectResult(apiResponse ?? new List<ApiResponse>());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Request was cancelled by the client.");
            // 499 Client Closed Request — the caller went away, nothing to return.
            return new StatusCodeResult(499);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error processing request.");
            return Problem(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
        finally
        {
            _logger.LogInformation("GetRegionalRailRequest End");
        }
    }

    /// <summary>
    /// Makes a rest call to the SEPTA API.
    /// </summary>
    /// <param name="sourceStation">Origination station name.</param>
    /// <param name="destStation">Destination station name.</param>
    /// <param name="cancellationToken">Token tied to the inbound request lifetime.</param>
    /// <returns>Next three train timings with any delay, as the raw SEPTA JSON payload.</returns>
    private async Task<string> CallSeptaApi(string sourceStation, string destStation, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Calling Septa API start. from {Source} to {Dest}", sourceStation, destStation);

        // Pooled client — do not dispose; handlers/headers are configured in Program.cs.
        HttpClient client = _httpClientFactory.CreateClient(SeptaClientName);

        try
        {
            // Escape user-supplied station names so they can't break out of the URL path.
            var relativeUri = $"{Uri.EscapeDataString(sourceStation)}/{Uri.EscapeDataString(destStation)}/3";
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(client.BaseAddress!, relativeUri));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using HttpResponseMessage response = await client.SendAsync(request, cancellationToken);
            _logger.LogInformation("Calling Septa API complete. Result {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"SEPTA returned {(int)response.StatusCode} {response.ReasonPhrase}");

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        finally
        {
            _logger.LogInformation("Calling Septa API end. from {Source} to {Dest}", sourceStation, destStation);
        }
    }

    private static ObjectResult Problem(int statusCode, string detail) =>
        new(new ProblemDetails { Status = statusCode, Detail = detail })
        {
            StatusCode = statusCode
        };
}
