using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace GetTrainsFunction.Tests;

public class NextThreeTrainFunctionTests
{
    private MockRepository _mockRepository;
    private Mock<HttpMessageHandler> _handlerMock;
    private Mock<ILogger<NextThreeTrainFunction>> _mockLogger;
    private HttpClient _magicHttpClient;
    private Mock<ILoggerFactory> _mockLoggerFactory;
    private Mock<IHttpClientFactory> _mockHttpClientFactory;
    private Mock<FunctionContext> _mockContext;


    public NextThreeTrainFunctionTests()
    {
        _mockRepository = new(MockBehavior.Default);
        _handlerMock = _mockRepository.Create<HttpMessageHandler>();
        _mockLogger = _mockRepository.Create<ILogger<NextThreeTrainFunction>>();
        _mockLoggerFactory = _mockRepository.Create<ILoggerFactory>();
        _mockHttpClientFactory = _mockRepository.Create<IHttpClientFactory>();
        _mockContext = new Mock<FunctionContext>();
    }

    [Fact(Skip = "TODO Mock WriteAsJsonAsync")]
    public async Task TestNextThreeTrainFunctionSuccess()
    {
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockContext = new Mock<FunctionContext>();

        var input = new ApiRequest { From = "StationOne", To = "StationTwo" };
        var body = JsonSerializer.Serialize(input);

        var mockRequest = new MockHttpRequestData(body);

        var mockResponse = new MockHttpResponseData(mockContext.Object);

        var mockLogger = new Mock<ILogger<NextThreeTrainFunction>>();
        mockLogger.Setup(m => m.Log(LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()));

        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockLoggerFactory.Setup(m => m.CreateLogger(It.IsAny<string>())).Returns(() => mockLogger.Object);

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
                )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("""[{"orig_train":"4370","orig_line":"West Trenton","orig_departure_time":"11:27PM","orig_arrival_time":"12:00AM","orig_delay":"On time","isdirect":"true"}]""")
            })
            .Verifiable();

        var nextThreeTrainFunction = CreateNextThreeTrainFunction();
        var result = await nextThreeTrainFunction.Run(mockRequest);

        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("", null)]
    [InlineData(null, "")]
    public async Task NextThreeTrainFunction_InvalidStationNames(string from, string to)
    {
        // Arrange
        _mockLogger.Setup(m => m.Log(LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()));

        _mockLoggerFactory.Setup(m => m.CreateLogger(It.IsAny<string>())).Returns(() => _mockLogger.Object);


        var input = new ApiRequest { From = from, To = to };
        var body = JsonSerializer.Serialize(input);

        var mockRequest = new MockHttpRequestData(body);
        var mockResponse = new MockHttpResponseData(_mockContext.Object);
        var nextThreeTrainFunction = CreateNextThreeTrainFunction();

        // Act
        var result = await nextThreeTrainFunction.Run(mockRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Body.Length);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task NextThreeTrainFunction_InvalidBody()
    {

        // Arrange
        _mockLogger.Setup(m => m.Log(LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()));

        _mockLoggerFactory.Setup(m => m.CreateLogger(It.IsAny<string>())).Returns(() => _mockLogger.Object);

        string? body = null;

        var mockRequest = new MockHttpRequestData(body);
        var mockResponse = new MockHttpResponseData(_mockContext.Object);
        var nextThreeTrainFunction = CreateNextThreeTrainFunction();

        // Act
        var result = await nextThreeTrainFunction.Run(mockRequest);


        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Body.Length);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, result.StatusCode);
    }

    #region Private
    private NextThreeTrainFunction CreateNextThreeTrainFunction()
    {
        _magicHttpClient = new(_handlerMock.Object);
        _mockLoggerFactory.Setup(m => m.CreateLogger(It.IsAny<string>())).Returns(() => _mockLogger.Object);
        _mockHttpClientFactory.Setup(x => x.CreateClient("httpClient")).Returns(() =>
        {
            var client = _magicHttpClient;
            client.BaseAddress = new Uri("http://ThirdPartyUri.info/");
            return client;
        });
        return new(_mockLoggerFactory.Object, _mockHttpClientFactory.Object);
    }

    #endregion
}