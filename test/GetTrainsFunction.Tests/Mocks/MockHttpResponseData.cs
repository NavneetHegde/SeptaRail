using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace GetTrainsFunction.Tests;

public sealed class MockHttpResponseData : HttpResponseData
{
    public MockHttpResponseData(FunctionContext context) : base(context)
    {
    }
    public override HttpStatusCode StatusCode { get; set; }
    public override HttpHeadersCollection Headers { get; set; }
    public override Stream Body { get; set; } = new MemoryStream();
    public override HttpCookies Cookies { get; }
}
