using NPoco;

using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.AuthorizedServices.Persistence.Dtos;

[TableName(Constants.Migrations.UmbracoAuthorizedServiceTokenTableName)]
[PrimaryKey("serviceAlias", AutoIncrement = false)]
[ExplicitColumns]
public class TokenDto
{
    [Column("serviceAlias")]
    [PrimaryKeyColumn(Name = "PK_serviceAlias", AutoIncrement = false)]
    [Length(100)]
    public string ServiceAlias { get; set; } = string.Empty;

    [Column("accessToken")]
    [Length(1000)]
    public string AccessToken { get; set; } = string.Empty;

    [Column("refreshToken")]
    [Length(1000)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? RefreshToken { get; set; }

    [Column("expiresOn")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? ExpiresOn { get; set; }
}
