using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.Security.Claims;
using System.Text;

namespace GetTrainsFunction.Tests;

public sealed class MockHttpRequestData : HttpRequestData
{
    // No behaviour is actually needed from this.
    private static readonly FunctionContext Context = Mock.Of<FunctionContext>();

    public MockHttpRequestData(string body) : base(Context)
    {
        if (!string.IsNullOrWhiteSpace(body))
        {
            var bytes = Encoding.UTF8.GetBytes(body);
            Body = new MemoryStream(bytes);
        }
    }

    public override HttpResponseData CreateResponse()
    {
        return new MockHttpResponseData(Context);
    }

    public override Stream Body { get; }
    public override HttpHeadersCollection Headers { get; }
    public override IReadOnlyCollection<IHttpCookie> Cookies { get; }
    public override Uri Url { get; }
    public override IEnumerable<ClaimsIdentity> Identities { get; }
    public override string Method { get; }
}
