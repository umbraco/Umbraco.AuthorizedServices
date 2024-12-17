using System.Globalization;
using System.Net.Http.Headers;
using Umbraco.AuthorizedServices.Models;

namespace Umbraco.AuthorizedServices.Services.Implement;

internal sealed class ServiceResponseMetadataParser : IServiceResponseMetadataParser
{
    public ServiceResponseMetadata ParseMetadata(HttpResponseMessage httpResponseMessage) =>
        new ServiceResponseMetadata
        {
            CacheControl = GetHeaderValue(httpResponseMessage.Headers, "Cache-Control"),
            Date = httpResponseMessage.Headers.Date,
            ETag = httpResponseMessage.Headers.ETag?.Tag,
            Expires = GetDateTimeHeaderValue(httpResponseMessage.Content.Headers, "Expires"),
            LastModified = GetDateTimeHeaderValue(httpResponseMessage.Content.Headers, "Last-Modified"),
            Location = GetHeaderValue(httpResponseMessage.Headers, "Location"),
            PoweredBy = GetHeaderValue(httpResponseMessage.Headers, "X-Powered-By"),
            Server = GetHeaderValue(httpResponseMessage.Headers, "Server"),
            RateLimits = ParseRateLimits(httpResponseMessage.Headers),
            Vary = GetHeaderValue(httpResponseMessage.Headers, "Vary"),
        };

    private static ServiceResponseMetadata.RateLimitData? ParseRateLimits(HttpResponseHeaders headers)
    {
        if (!headers.Any(x => x.Key.StartsWith("X-RateLimit")))
        {
            return null;
        }

        int.TryParse(GetHeaderValue(headers, "X-RateLimit-Limit"), out var limit);
        int.TryParse(GetHeaderValue(headers, "X-RateLimit-Remaining"), out var remaining);

        var limits = new ServiceResponseMetadata.RateLimitData
        {
            Limit = limit,
            Remaining = remaining,
        };

        var reset = GetHeaderValue(headers, "X-RateLimit-Reset");
        if (!string.IsNullOrEmpty(reset))
        {
            DateTime resetTime = GetRateLimitResetTime(headers, reset);

            limits.Reset = resetTime;
            limits.ResetSeconds = (resetTime - DateTime.Now).Seconds;
        }

        return limits;
    }

    private static DateTime GetRateLimitResetTime(HttpHeaders headers, string reset)
    {
        DateTime resetTime = DateTime.MinValue;
        if (int.TryParse(GetHeaderValue(headers, "X-RateLimit-Reset"), out var resetAsInteger))
        {
            resetTime = DateTimeOffset.FromUnixTimeSeconds(resetAsInteger).DateTime.ToLocalTime();
        }
        else if (!string.IsNullOrEmpty(reset) && DateTime.TryParseExact(
            reset,
            "yyyy-MM-ddTHH:mm:ss.ffffffZ",
            CultureInfo.CurrentCulture,
            DateTimeStyles.RoundtripKind,
            out DateTime resetAsDateTime))
        {
            resetTime = resetAsDateTime.ToLocalTime();
        }

        return resetTime;
    }

    private static string? GetHeaderValue(HttpHeaders headers, string key)
    {
        KeyValuePair<string, IEnumerable<string>> limitUsed = headers
            .FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.CurrentCultureIgnoreCase));
        return limitUsed.Value?.First();
    }

    private static DateTime? GetDateTimeHeaderValue(HttpHeaders headers, string key)
    {
        var headerValue = GetHeaderValue(headers, key);
        if (string.IsNullOrEmpty(headerValue))
        {
            return null;
        }

        if (DateTime.TryParseExact(headerValue, "r", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateValue))
        {
            return dateValue;
        }

        return null;
    }
}
