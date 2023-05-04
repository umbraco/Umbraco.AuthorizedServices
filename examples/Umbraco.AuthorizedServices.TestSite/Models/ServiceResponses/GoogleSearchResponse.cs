using Newtonsoft.Json;

namespace Umbraco.AuthorizedServices.TestSite.Models.ServiceResponses;

public class GoogleSearchResponse
{
    [JsonProperty("inspectionUrl")]
    public string InspectionUrl { get; set; } = string.Empty;

    [JsonProperty("siteUrl")]
    public string SiteUrl { get; set; } = string.Empty;

    [JsonProperty("languageCode")]
    public string LanguageCode { get; set; } = string.Empty;
}
