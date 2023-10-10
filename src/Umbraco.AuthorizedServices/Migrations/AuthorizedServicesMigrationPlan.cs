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
        To<AddDatabaseOAuth2TokenStorageTable>(Constants.Migrations.UmbracoAuthorizedServiceOAuth2TokenTargetState);
        To<AddDatabaseKeyStorageTable>(Constants.Migrations.UmbracoAuthorizedServiceKeyTargetState);
        To<AddDatabaseOAuth1TokenStorageTable>(Constants.Migrations.UmbracoAuthorizedServiceOAuth1TargetState);
    }
}
