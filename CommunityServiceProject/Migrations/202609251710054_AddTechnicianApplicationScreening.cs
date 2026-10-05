namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTechnicianApplicationScreening : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TechnicianApplicationScreenings",
                c => new
                {
                    ScreeningID = c.Int(nullable: false, identity: true),
                    ApplicationID = c.Int(nullable: false),
                    QualificationAssessment = c.Int(nullable: false),
                    ExperienceAssessment = c.Int(nullable: false),
                    RequirementsAssessment = c.Int(nullable: false),
                    OverallResult = c.Int(nullable: false),
                    ScreeningComments = c.String(nullable: false, maxLength: 3000),
                    ScreenedByAdministratorID = c.Int(),
                    ScreenedByHROfficerID = c.Int(),
                    ScreeningDate = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.ScreeningID)
                .ForeignKey(
                    "dbo.TechnicianApplications",
                    t => t.ApplicationID,
                    cascadeDelete: false)
                .ForeignKey(
                    "dbo.Administrators",
                    t => t.ScreenedByAdministratorID)
                .Index(t => t.ApplicationID)
                .Index(t => t.ScreenedByAdministratorID);
        }
        public override void Down()
        {
            DropForeignKey(
                "dbo.TechnicianApplicationScreenings",
                "ScreenedByAdministratorID",
                "dbo.Administrators");

            DropForeignKey(
                "dbo.TechnicianApplicationScreenings",
                "ApplicationID",
                "dbo.TechnicianApplications");

            DropIndex(
                "dbo.TechnicianApplicationScreenings",
                new[] { "ScreenedByAdministratorID" });

            DropIndex(
                "dbo.TechnicianApplicationScreenings",
                new[] { "ApplicationID" });

            DropTable(
                "dbo.TechnicianApplicationScreenings");
        }
    }
}
