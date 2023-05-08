using System.Text.Json.Serialization;

namespace Umbraco.AuthorizedServices.TestSite.Models.ServiceResponses;

public class HubspotContactResponse
{
    public IEnumerable<Result> Results { get; set; } = Enumerable.Empty<Result>();

    public class Result
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("properties")]
        public ResultProperties Properties { get; set; } = new ResultProperties();
    }

    public class ResultProperties
    {
        [JsonPropertyName("firstname")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("lastname")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
    }
}
