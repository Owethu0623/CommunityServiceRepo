namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTechnicianOnboardingAndCitizenTechnicianLink : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TechnicianOnboardings",
                c => new
                    {
                        OnboardingID = c.Int(nullable: false, identity: true),
                        ApplicationID = c.Int(nullable: false),
                        TechnicianID = c.Int(nullable: false),
                        OnboardedByAdministratorID = c.Int(nullable: false),
                        OnboardingDate = c.DateTime(nullable: false),
                        MunicipalEmail = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.OnboardingID)
                .ForeignKey("dbo.TechnicianApplications", t => t.ApplicationID)
                .ForeignKey("dbo.Administrators", t => t.OnboardedByAdministratorID)
                .ForeignKey("dbo.Technicians", t => t.TechnicianID)
                .Index(t => t.ApplicationID, unique: true, name: "IX_TechnicianOnboarding_Application")
                .Index(t => t.TechnicianID)
                .Index(t => t.OnboardedByAdministratorID);
            
            AddColumn("dbo.Technicians", "CitizenID", c => c.Int());
            CreateIndex("dbo.Technicians", "CitizenID");
            AddForeignKey("dbo.Technicians", "CitizenID", "dbo.Citizens", "CitizenID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TechnicianOnboardings", "TechnicianID", "dbo.Technicians");
            DropForeignKey("dbo.TechnicianOnboardings", "OnboardedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.TechnicianOnboardings", "ApplicationID", "dbo.TechnicianApplications");
            DropForeignKey("dbo.Technicians", "CitizenID", "dbo.Citizens");
            DropIndex("dbo.TechnicianOnboardings", new[] { "OnboardedByAdministratorID" });
            DropIndex("dbo.TechnicianOnboardings", new[] { "TechnicianID" });
            DropIndex("dbo.TechnicianOnboardings", "IX_TechnicianOnboarding_Application");
            DropIndex("dbo.Technicians", new[] { "CitizenID" });
            DropColumn("dbo.Technicians", "CitizenID");
            DropTable("dbo.TechnicianOnboardings");
        }
    }
}
