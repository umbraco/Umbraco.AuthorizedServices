using Newtonsoft.Json;

namespace Umbraco.AuthorizedServices.TestSite.Models.ServiceResponses
{
    public class MeetupRequest
    {
        [JsonProperty("query")]
        public string Query { get; set; } = string.Empty;
    }
}
