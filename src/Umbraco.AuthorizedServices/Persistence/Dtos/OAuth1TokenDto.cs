using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.AuthorizedServices.Persistence.Dtos;

[TableName(Constants.Migrations.UmbracoAuthorizedServiceOAuth1TokenTableName)]
[PrimaryKey("serviceAlias", AutoIncrement = false)]
[ExplicitColumns]
public class OAuth1TokenDto
{
    [Column("serviceAlias")]
    [PrimaryKeyColumn(Name = "PK_serviceAlias", AutoIncrement = false)]
    [Length(100)]
    public string ServiceAlias { get; set; } = string.Empty;

    [Column("oauthToken")]
    [Length(1000)]
    public string OAuthToken { get; set; } = string.Empty;

    [Column("oauthTokenSecret")]
    [Length(1000)]
    public string OAuthTokenSecret { get; set; } = string.Empty;
}
