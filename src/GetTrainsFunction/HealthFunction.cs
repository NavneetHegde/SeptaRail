using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace GetTrainsFunction;

/// <summary>
/// Lightweight, unauthenticated liveness probe so the hosting platform and the
/// mobile client can check availability without a function key.
/// </summary>
public class HealthFunction
{
    [Function("health")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        => new OkObjectResult(new { status = "healthy", timestamp = DateTimeOffset.UtcNow });
}
