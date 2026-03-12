using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace OptimusFrame.Core.API.HealthChecks;

[ExcludeFromCodeCoverage]
public static class HealthCheckResponseWriter
{
    public static Task WriteHealthCheckResponse(HttpContext context, HealthReport healthReport)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var result = new
        {
            status = healthReport.Status.ToString(),
            totalDuration = healthReport.TotalDuration.TotalMilliseconds,
            checks = healthReport.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration.TotalMilliseconds,
                exception = entry.Value.Exception?.Message,
                data = entry.Value.Data,
                tags = entry.Value.Tags
            })
        };

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(result, options));
    }
}
