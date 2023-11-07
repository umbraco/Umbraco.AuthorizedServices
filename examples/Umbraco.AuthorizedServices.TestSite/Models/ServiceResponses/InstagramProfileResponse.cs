using System.Text.Json.Serialization;

namespace Umbraco.AuthorizedServices.TestSite.Models.ServiceResponses;

public class InstagramProfileResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
}
