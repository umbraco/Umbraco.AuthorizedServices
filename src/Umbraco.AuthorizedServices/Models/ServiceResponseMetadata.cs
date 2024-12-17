namespace Umbraco.AuthorizedServices.Models;

public class ServiceResponseMetadata
{
    public string? CacheControl { get; set; }

    public DateTimeOffset? Date { get; set; }

    public string? ETag { get; set; }

    public DateTimeOffset? Expires { get; set; }

    public DateTimeOffset? LastModified { get; set; }

    public string? Location { get; set; }

    public string? PoweredBy { get; set; }

    public RateLimitData? RateLimits { get; set; }

    public string? Server { get; set; }

    public string? Vary { get; set; }

    public class RateLimitData
    {
        public int Limit { get; set; }

        public int Remaining { get; set; }

        public DateTimeOffset? Reset { get; set; }

        public int? ResetSeconds { get; set; }
    }
}
