using Umbraco.Cms.Core.Packaging;

namespace Umbraco.AuthorizedServices.Migrations;

/// <summary>
/// Defines the migration plan for the Authorized Services package.
/// </summary>
public class AuthorizedServicesMigrationPlan : PackageMigrationPlan
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizedServicesMigrationPlan" /> class.
    /// </summary>
    public AuthorizedServicesMigrationPlan()
        : base(Constants.PackageName, Constants.Migrations.MigrationPlan)
    { }

    /// <inheritdoc />
    public override bool IgnoreCurrentState => false;

    /// <inheritdoc />
    protected override void DefinePlan()
    {
        To<AddDatabaseTokenStorageTable>(Constants.Migrations.UmbracoAuthorizedServiceTokenTargetState);
        To<AddDatabaseKeyStorageTable>(Constants.Migrations.UmbracoAuthorizedServiceKeyTargetState);
        To<AddDatabaseOAuth1aTokenStorageTable>(Constants.Migrations.UmbracoAuthorizedServiceOAuth1aTargetState);
    }
}
