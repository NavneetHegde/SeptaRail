using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;

namespace GetTrainsFunction.Tests;

public class NextThreeTrainFunctionTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock = new(MockBehavior.Default);
    private readonly Mock<IHttpClientFactory> _httpClientFactory = new(MockBehavior.Default);

    // Captures the request the function actually sends to SEPTA so tests can assert on it.
    private HttpRequestMessage? _capturedRequest;

    private static HttpRequest CreateRequest(string? body)
    {
        var context = new DefaultHttpContext();
        context.Request.ContentType = "application/json";
        var bytes = Encoding.UTF8.GetBytes(body ?? string.Empty);
        context.Request.Body = new MemoryStream(bytes);
        context.Request.ContentLength = bytes.Length;
        return context.Request;
    }

    private NextThreeTrainFunction CreateSut(HttpResponseMessage? septaResponse = null)
    {
        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => _capturedRequest = request)
            .ReturnsAsync(septaResponse ?? new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") });

        return BuildSut();
    }

    // Sets up the SEPTA handler to throw, simulating a transport failure / cancellation.
    private NextThreeTrainFunction CreateSutThatThrows(Exception exception)
    {
        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => _capturedRequest = request)
            .ThrowsAsync(exception);

        return BuildSut();
    }

    private NextThreeTrainFunction BuildSut()
    {
        var client = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("http://septa.test/") };
        _httpClientFactory.Setup(f => f.CreateClient("httpClient")).Returns(client);
        return new NextThreeTrainFunction(NullLogger<NextThreeTrainFunction>.Instance, _httpClientFactory.Object);
    }

    [Fact]
    public async Task Returns_Ok_With_Trains_On_Success()
    {
        var septa = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""[{"orig_train":"4370","orig_line":"West Trenton","orig_departure_time":"11:27PM","orig_arrival_time":"12:00AM","orig_delay":"On time","isdirect":"true"}]""")
        };
        var sut = CreateSut(septa);
        var req = CreateRequest(JsonSerializer.Serialize(new ApiRequest { From = "StationOne", To = "StationTwo" }));

        var result = await sut.Run(req);

        var ok = Assert.IsType<OkObjectResult>(result);
        var trains = Assert.IsAssignableFrom<List<ApiResponse>>(ok.Value);
        Assert.Single(trains);
        Assert.Equal("4370", trains[0].orig_train);
    }

    [Fact]
    public async Task Returns_Ok_With_Empty_List_When_Septa_Returns_No_Trains()
    {
        var sut = CreateSut(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") });
        var req = CreateRequest(JsonSerializer.Serialize(new ApiRequest { From = "A", To = "B" }));

        var result = await sut.Run(req);

        var ok = Assert.IsType<OkObjectResult>(result);
        var trains = Assert.IsAssignableFrom<List<ApiResponse>>(ok.Value);
        Assert.Empty(trains);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("", null)]
    [InlineData(null, "")]
    [InlineData("  ", "B")]
    [InlineData("A", "  ")]
    public async Task Returns_BadRequest_For_Invalid_Station_Names(string? from, string? to)
    {
        var sut = CreateSut();
        var req = CreateRequest(JsonSerializer.Serialize(new ApiRequest { From = from!, To = to! }));

        var result = await sut.Run(req);

        AssertStatusCode(result, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Returns_BadRequest_For_Empty_Body()
    {
        var sut = CreateSut();
        var req = CreateRequest(string.Empty);

        var result = await sut.Run(req);

        AssertStatusCode(result, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Returns_BadRequest_For_Malformed_Body()
    {
        var sut = CreateSut();
        var req = CreateRequest("{ not valid json");

        var result = await sut.Run(req);

        AssertStatusCode(result, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Returns_BadGateway_When_Septa_Returns_Error_Status()
    {
        var sut = CreateSut(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var req = CreateRequest(JsonSerializer.Serialize(new ApiRequest { From = "A", To = "B" }));

        var result = await sut.Run(req);

        AssertStatusCode(result, StatusCodes.Status502BadGateway);
    }

    [Fact]
    public async Task Returns_BadGateway_When_Septa_Returns_Invalid_Json()
    {
        var sut = CreateSut(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("<html>not json</html>") });
        var req = CreateRequest(JsonSerializer.Serialize(new ApiRequest { From = "A", To = "B" }));

        var result = await sut.Run(req);

        AssertStatusCode(result, StatusCodes.Status502BadGateway);
    }

    [Fact]
    public async Task Returns_BadGateway_When_Septa_Transport_Fails()
    {
        var sut = CreateSutThatThrows(new HttpRequestException("connection refused"));
        var req = CreateRequest(JsonSerializer.Serialize(new ApiRequest { From = "A", To = "B" }));

        var result = await sut.Run(req);

        AssertStatusCode(result, StatusCodes.Status502BadGateway);
    }

    [Fact]
    public async Task Escapes_Station_Names_In_Septa_Request_Url()
    {
        var sut = CreateSut();
        var req = CreateRequest(JsonSerializer.Serialize(new ApiRequest { From = "30th Street Station", To = "Suburban Station" }));

        await sut.Run(req);

        Assert.NotNull(_capturedRequest);
        var uri = _capturedRequest!.RequestUri!.AbsoluteUri;
        Assert.Contains("30th%20Street%20Station/Suburban%20Station/3", uri);
        // Raw spaces must never reach the upstream URL.
        Assert.DoesNotContain(' ', uri);
    }

    [Fact]
    public async Task Sends_Accept_Json_Header_To_Septa()
    {
        var sut = CreateSut();
        var req = CreateRequest(JsonSerializer.Serialize(new ApiRequest { From = "A", To = "B" }));

        await sut.Run(req);

        Assert.NotNull(_capturedRequest);
        Assert.Contains(_capturedRequest!.Headers.Accept, h => h.MediaType == "application/json");
    }

    [Fact]
    public async Task Returns_499_When_Client_Cancels()
    {
        var sut = CreateSutThatThrows(new OperationCanceledException());
        var req = CreateRequest(JsonSerializer.Serialize(new ApiRequest { From = "A", To = "B" }));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await sut.Run(req, cts.Token);

        AssertStatusCode(result, 499);
    }

    private static void AssertStatusCode(IActionResult result, int expected)
    {
        var statusCode = result switch
        {
            ObjectResult obj => obj.StatusCode,
            StatusCodeResult status => status.StatusCode,
            _ => null
        };
        Assert.Equal(expected, statusCode);
    }
}
