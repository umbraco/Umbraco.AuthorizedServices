using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.AuthorizedServices.Persistence.Dtos;

[TableName(Constants.Database.TableNames.ApiKey)]
[PrimaryKey("serviceAlias", AutoIncrement = false)]
[ExplicitColumns]
public class KeyDto
{
    [Column("serviceAlias")]
    [PrimaryKeyColumn(Name = "PK_umbracoAuthorizedServiceKey_serviceAlias", AutoIncrement = false)]
    [Length(100)]
    public string ServiceAlias { get; set; } = string.Empty;

    [Column("apiKey")]
    [Length(1000)]
    public string ApiKey { get; set; } = string.Empty;
}
