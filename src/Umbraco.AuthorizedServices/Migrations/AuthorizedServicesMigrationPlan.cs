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
        : base(Constants.PackageName, Constants.Database.Migrations.MigrationPlan)
    { }

    /// <inheritdoc />
    protected override void DefinePlan()
    {
        // 0.2
        To<AddOAuth2TokenTable>(Constants.Database.Migrations.TargetStates.AddOAuth2TokenTable);

        // 0.3
        To<RenameOAuth2TokenTable>(Constants.Database.Migrations.TargetStates.RenameOAuth2TokenTable);
        To<AddKeyTable>(Constants.Database.Migrations.TargetStates.AddKeyTable);
        To<AddOAuth1TokenTable>(Constants.Database.Migrations.TargetStates.AddOAuth1TokenTable);
    }
}
