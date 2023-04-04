using Newtonsoft.Json;

namespace Umbraco.AuthorizedServices.TestSite.Models;

public class DynamicsFormResponse
{
    [JsonProperty("value")]
    public IEnumerable<Result> Results { get; set; } = Enumerable.Empty<Result>();

    public class Result
    {
        [JsonProperty("msdyncrm_marketingformid")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("msdyncrm_name")]
        public string Name { get; set; } = string.Empty;
    }
}

