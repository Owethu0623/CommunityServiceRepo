namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPendingModelChanges : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ApplicationAssessments", "RecordedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.TechnicianOpportunities", "CreatedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.ApplicationInterviews", "RecordedByAdministratorID", "dbo.Administrators");
            DropIndex("dbo.ApplicationAssessments", new[] { "RecordedByAdministratorID" });
            DropIndex("dbo.TechnicianOpportunities", new[] { "CreatedByAdministratorID" });
            DropIndex("dbo.ApplicationInterviews", new[] { "RecordedByAdministratorID" });
            CreateTable(
                "dbo.AdministratorNotifications",
                c => new
                    {
                        AdministratorNotificationID = c.Int(nullable: false, identity: true),
                        AdministratorID = c.Int(nullable: false),
                        ApplicationID = c.Int(),
                        Title = c.String(nullable: false, maxLength: 150),
                        Message = c.String(nullable: false, maxLength: 2000),
                        DateCreated = c.DateTime(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                        ReadDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.AdministratorNotificationID)
                .ForeignKey("dbo.Administrators", t => t.AdministratorID, cascadeDelete: true)
                .ForeignKey("dbo.TechnicianApplications", t => t.ApplicationID)
                .Index(t => t.AdministratorID)
                .Index(t => t.ApplicationID);
            
            CreateTable(
                "dbo.HROfficerNotifications",
                c => new
                    {
                        HROfficerNotificationID = c.Int(nullable: false, identity: true),
                        HROfficerID = c.Int(nullable: false),
                        ApplicationID = c.Int(),
                        Title = c.String(nullable: false, maxLength: 150),
                        Message = c.String(nullable: false, maxLength: 2000),
                        DateCreated = c.DateTime(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                        ReadDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.HROfficerNotificationID)
                .ForeignKey("dbo.TechnicianApplications", t => t.ApplicationID)
                .ForeignKey("dbo.HROfficers", t => t.HROfficerID, cascadeDelete: true)
                .Index(t => t.HROfficerID)
                .Index(t => t.ApplicationID);
            
            AddColumn("dbo.Requests", "ComplianceConfirmed", c => c.Boolean(nullable: false));
            AddColumn("dbo.Requests", "ComplianceConfirmedDate", c => c.DateTime());
            AddColumn("dbo.ApplicationAssessments", "RecordedByHROfficerID", c => c.Int());
            AddColumn("dbo.TechnicianOpportunities", "CreatedByHROfficerID", c => c.Int());
            AddColumn("dbo.TechnicianOpportunities", "LastUpdatedByHROfficerID", c => c.Int());
            AddColumn("dbo.ApplicationInterviews", "RecordedByHROfficerID", c => c.Int());
            AddColumn("dbo.TechnicianOnboardings", "OnboardedByHROfficerID", c => c.Int());
            AlterColumn("dbo.ApplicationAssessments", "RecordedByAdministratorID", c => c.Int());
            AlterColumn("dbo.TechnicianOpportunities", "CreatedByAdministratorID", c => c.Int());
            AlterColumn("dbo.ApplicationInterviews", "RecordedByAdministratorID", c => c.Int());
            CreateIndex("dbo.TechnicianOpportunities", "CreatedByAdministratorID");
            CreateIndex("dbo.TechnicianOpportunities", "CreatedByHROfficerID");
            CreateIndex("dbo.TechnicianOpportunities", "LastUpdatedByHROfficerID");
            CreateIndex("dbo.ApplicationAssessments", "RecordedByAdministratorID");
            CreateIndex("dbo.ApplicationAssessments", "RecordedByHROfficerID");
            CreateIndex("dbo.ApplicationInterviews", "RecordedByAdministratorID");
            CreateIndex("dbo.ApplicationInterviews", "RecordedByHROfficerID");
            CreateIndex("dbo.TechnicianOnboardings", "OnboardedByHROfficerID");
            AddForeignKey("dbo.TechnicianOpportunities", "CreatedByHROfficerID", "dbo.HROfficers", "HROfficerID");
            AddForeignKey("dbo.TechnicianOpportunities", "LastUpdatedByHROfficerID", "dbo.HROfficers", "HROfficerID");
            AddForeignKey("dbo.ApplicationAssessments", "RecordedByHROfficerID", "dbo.HROfficers", "HROfficerID");
            AddForeignKey("dbo.ApplicationInterviews", "RecordedByHROfficerID", "dbo.HROfficers", "HROfficerID");
            AddForeignKey("dbo.TechnicianOnboardings", "OnboardedByHROfficerID", "dbo.HROfficers", "HROfficerID");
            AddForeignKey("dbo.ApplicationAssessments", "RecordedByAdministratorID", "dbo.Administrators", "AdministratorID");
            AddForeignKey("dbo.TechnicianOpportunities", "CreatedByAdministratorID", "dbo.Administrators", "AdministratorID");
            AddForeignKey("dbo.ApplicationInterviews", "RecordedByAdministratorID", "dbo.Administrators", "AdministratorID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ApplicationInterviews", "RecordedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.TechnicianOpportunities", "CreatedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.ApplicationAssessments", "RecordedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.TechnicianOnboardings", "OnboardedByHROfficerID", "dbo.HROfficers");
            DropForeignKey("dbo.HROfficerNotifications", "HROfficerID", "dbo.HROfficers");
            DropForeignKey("dbo.HROfficerNotifications", "ApplicationID", "dbo.TechnicianApplications");
            DropForeignKey("dbo.ApplicationInterviews", "RecordedByHROfficerID", "dbo.HROfficers");
            DropForeignKey("dbo.ApplicationAssessments", "RecordedByHROfficerID", "dbo.HROfficers");
            DropForeignKey("dbo.AdministratorNotifications", "ApplicationID", "dbo.TechnicianApplications");
            DropForeignKey("dbo.TechnicianOpportunities", "LastUpdatedByHROfficerID", "dbo.HROfficers");
            DropForeignKey("dbo.TechnicianOpportunities", "CreatedByHROfficerID", "dbo.HROfficers");
            DropForeignKey("dbo.AdministratorNotifications", "AdministratorID", "dbo.Administrators");
            DropIndex("dbo.TechnicianOnboardings", new[] { "OnboardedByHROfficerID" });
            DropIndex("dbo.HROfficerNotifications", new[] { "ApplicationID" });
            DropIndex("dbo.HROfficerNotifications", new[] { "HROfficerID" });
            DropIndex("dbo.ApplicationInterviews", new[] { "RecordedByHROfficerID" });
            DropIndex("dbo.ApplicationInterviews", new[] { "RecordedByAdministratorID" });
            DropIndex("dbo.ApplicationAssessments", new[] { "RecordedByHROfficerID" });
            DropIndex("dbo.ApplicationAssessments", new[] { "RecordedByAdministratorID" });
            DropIndex("dbo.TechnicianOpportunities", new[] { "LastUpdatedByHROfficerID" });
            DropIndex("dbo.TechnicianOpportunities", new[] { "CreatedByHROfficerID" });
            DropIndex("dbo.TechnicianOpportunities", new[] { "CreatedByAdministratorID" });
            DropIndex("dbo.AdministratorNotifications", new[] { "ApplicationID" });
            DropIndex("dbo.AdministratorNotifications", new[] { "AdministratorID" });
            AlterColumn("dbo.ApplicationInterviews", "RecordedByAdministratorID", c => c.Int(nullable: false));
            AlterColumn("dbo.TechnicianOpportunities", "CreatedByAdministratorID", c => c.Int(nullable: false));
            AlterColumn("dbo.ApplicationAssessments", "RecordedByAdministratorID", c => c.Int(nullable: false));
            DropColumn("dbo.TechnicianOnboardings", "OnboardedByHROfficerID");
            DropColumn("dbo.TechnicianApplicationSelections", "SelectedByHROfficerID");
            DropColumn("dbo.ApplicationInterviews", "RecordedByHROfficerID");
            DropColumn("dbo.TechnicianOpportunities", "LastUpdatedByHROfficerID");
            DropColumn("dbo.TechnicianOpportunities", "CreatedByHROfficerID");
            DropColumn("dbo.ApplicationAssessments", "RecordedByHROfficerID");
            DropColumn("dbo.Requests", "ComplianceConfirmedDate");
            DropColumn("dbo.Requests", "ComplianceConfirmed");
            DropTable("dbo.HROfficerNotifications");
            DropTable("dbo.AdministratorNotifications");
            CreateIndex("dbo.ApplicationInterviews", "RecordedByAdministratorID");
            CreateIndex("dbo.TechnicianOpportunities", "CreatedByAdministratorID");
            CreateIndex("dbo.ApplicationAssessments", "RecordedByAdministratorID");
            AddForeignKey("dbo.ApplicationInterviews", "RecordedByAdministratorID", "dbo.Administrators", "AdministratorID", cascadeDelete: true);
            AddForeignKey("dbo.TechnicianOpportunities", "CreatedByAdministratorID", "dbo.Administrators", "AdministratorID", cascadeDelete: true);
            AddForeignKey("dbo.ApplicationAssessments", "RecordedByAdministratorID", "dbo.Administrators", "AdministratorID", cascadeDelete: true);
        }
    }
}
