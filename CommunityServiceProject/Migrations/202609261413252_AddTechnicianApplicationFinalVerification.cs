namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTechnicianApplicationFinalVerification : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TechnicianApplicationFinalVerifications",
                c => new
                    {
                        FinalVerificationID = c.Int(nullable: false, identity: true),
                        ApplicationID = c.Int(nullable: false),
                        Result = c.Int(nullable: false),
                        Comments = c.String(nullable: false, maxLength: 3000),
                        VerifiedByAdministratorID = c.Int(nullable: false),
                        VerificationDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.FinalVerificationID)
                .ForeignKey("dbo.TechnicianApplications", t => t.ApplicationID)
                .ForeignKey("dbo.Administrators", t => t.VerifiedByAdministratorID)
                .Index(t => t.ApplicationID, unique: true, name: "IX_TechnicianApplicationFinalVerification_Application")
                .Index(t => t.VerifiedByAdministratorID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TechnicianApplicationFinalVerifications", "VerifiedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.TechnicianApplicationFinalVerifications", "ApplicationID", "dbo.TechnicianApplications");
            DropIndex("dbo.TechnicianApplicationFinalVerifications", new[] { "VerifiedByAdministratorID" });
            DropIndex("dbo.TechnicianApplicationFinalVerifications", "IX_TechnicianApplicationFinalVerification_Application");
            DropTable("dbo.TechnicianApplicationFinalVerifications");
        }
    }
}
