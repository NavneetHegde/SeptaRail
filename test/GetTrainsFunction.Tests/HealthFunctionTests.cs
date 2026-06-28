using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GetTrainsFunction.Tests;

public class HealthFunctionTests
{
    [Fact]
    public void Returns_Ok_With_Healthy_Status()
    {
        var sut = new HealthFunction();
        var req = new DefaultHttpContext().Request;

        var result = sut.Run(req);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);

        // Anonymous payload: { status = "healthy", timestamp = ... }
        var status = ok.Value!.GetType().GetProperty("status")!.GetValue(ok.Value);
        Assert.Equal("healthy", status);
    }
}
