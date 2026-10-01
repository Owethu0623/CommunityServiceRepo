
namespace CommunityServiceProject.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class RemoveUniqueTechnicianOnboardingApplication : DbMigration
    {
        public override void Up()
        {
            CreateIndex(
                "dbo.TechnicianOnboardings",
                "ApplicationID");
        }

        public override void Down()
        {
            DropIndex(
                "dbo.TechnicianOnboardings",
                new[] { "ApplicationID" });

            CreateIndex(
                "dbo.TechnicianOnboardings",
                "ApplicationID",
                unique: true,
                name: "IX_TechnicianOnboarding_Application");
        }
    }
}
