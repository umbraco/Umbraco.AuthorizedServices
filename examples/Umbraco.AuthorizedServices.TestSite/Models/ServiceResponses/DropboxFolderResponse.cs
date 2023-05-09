using Newtonsoft.Json;

namespace Umbraco.AuthorizedServices.TestSite.Models.ServiceResponses;

public class DropboxFolderResponse
{
    [JsonProperty("include_deleted")]
    public bool IncludeDeleted { get; set; }

    [JsonProperty("include_media_info")]
    public bool IncludeMediaInfo { get; set; }

    [JsonProperty("path")]
    public string? Path { get; set; }
}
