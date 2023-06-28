using System.Text.Json.Serialization;

namespace Umbraco.AuthorizedServices.TestSite.Models.ServiceResponses;

public class AssetBankSearchResponse : List<AssetBankAsset>
{
}

public class AssetBankAsset
{
    public int Id { get; set; }

    [JsonPropertyName("originalFilename")]
    public string OriginalFileName { get; set; } = string.Empty;

    public override string ToString() => $"{OriginalFileName} ({Id})";
}



