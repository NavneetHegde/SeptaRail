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
            .ReturnsAsync(septaResponse ?? new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") });

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

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("", null)]
    [InlineData(null, "")]
    public async Task Returns_BadRequest_For_Invalid_Station_Names(string? from, string? to)
    {
        var sut = CreateSut();
        var req = CreateRequest(JsonSerializer.Serialize(new ApiRequest { From = from!, To = to! }));

        var result = await sut.Run(req);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Returns_BadRequest_For_Empty_Body()
    {
        var sut = CreateSut();
        var req = CreateRequest(string.Empty);

        var result = await sut.Run(req);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Returns_503_For_Malformed_Body()
    {
        var sut = CreateSut();
        var req = CreateRequest("{ not valid json");

        var result = await sut.Run(req);

        var status = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, status.StatusCode);
    }

    [Fact]
    public async Task Returns_503_When_Septa_Call_Fails()
    {
        var sut = CreateSut(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var req = CreateRequest(JsonSerializer.Serialize(new ApiRequest { From = "A", To = "B" }));

        var result = await sut.Run(req);

        var status = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, status.StatusCode);
    }
}
