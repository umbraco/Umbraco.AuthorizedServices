using NPoco;

using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.AuthorizedServices.Migrations;

[TableName(Constants.Migrations.TableName)]
[PrimaryKey("Id", AutoIncrement = true)]
[ExplicitColumns]
public class DatabaseTokenStorageSchema
{
    [PrimaryKeyColumn(AutoIncrement = true, IdentitySeed = 1)]
    [Column("id")]
    public int Id { get; set; }

    [Column("serviceAlias")]
    public string ServiceAlias { get; set; } = string.Empty;

    [Column("accessToken")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    public string AccessToken { get; set; } = string.Empty;

    [Column("refreshToken")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    public string RefreshToken { get; set; } = string.Empty;

    [Column("expiresOn")]
    public DateTime? ExpiresOn { get; set; }
}
