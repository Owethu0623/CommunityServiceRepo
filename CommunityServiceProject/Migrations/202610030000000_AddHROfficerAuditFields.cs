namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddHROfficerAuditFields : DbMigration
    {
        public override void Up()
        {
            // ApplicationAssessments: add RecordedByHROfficerID
            AddColumn("dbo.ApplicationAssessments", "RecordedByHROfficerID", c => c.Int());
            CreateIndex("dbo.ApplicationAssessments", "RecordedByHROfficerID");
            AddForeignKey("dbo.ApplicationAssessments", "RecordedByHROfficerID", "dbo.HROfficers", "HROfficerID");

            // ApplicationInterviews: add RecordedByHROfficerID
            AddColumn("dbo.ApplicationInterviews", "RecordedByHROfficerID", c => c.Int());
            CreateIndex("dbo.ApplicationInterviews", "RecordedByHROfficerID");
            AddForeignKey("dbo.ApplicationInterviews", "RecordedByHROfficerID", "dbo.HROfficers", "HROfficerID");

            // TechnicianOnboardings: add OnboardedByHROfficerID
            AddColumn("dbo.TechnicianOnboardings", "OnboardedByHROfficerID", c => c.Int());
            CreateIndex("dbo.TechnicianOnboardings", "OnboardedByHROfficerID");
            AddForeignKey("dbo.TechnicianOnboardings", "OnboardedByHROfficerID", "dbo.HROfficers", "HROfficerID");

            // TechnicianOpportunities: add CreatedByHROfficerID and LastUpdatedByHROfficerID
            AddColumn("dbo.TechnicianOpportunities", "CreatedByHROfficerID", c => c.Int());
            AddColumn("dbo.TechnicianOpportunities", "LastUpdatedByHROfficerID", c => c.Int());
            CreateIndex("dbo.TechnicianOpportunities", "CreatedByHROfficerID");
            CreateIndex("dbo.TechnicianOpportunities", "LastUpdatedByHROfficerID");
            AddForeignKey("dbo.TechnicianOpportunities", "CreatedByHROfficerID", "dbo.HROfficers", "HROfficerID");
            AddForeignKey("dbo.TechnicianOpportunities", "LastUpdatedByHROfficerID", "dbo.HROfficers", "HROfficerID");
        }

        public override void Down()
        {
            // TechnicianOpportunities
            DropForeignKey("dbo.TechnicianOpportunities", "LastUpdatedByHROfficerID", "dbo.HROfficers");
            DropForeignKey("dbo.TechnicianOpportunities", "CreatedByHROfficerID", "dbo.HROfficers");
            DropIndex("dbo.TechnicianOpportunities", new[] { "LastUpdatedByHROfficerID" });
            DropIndex("dbo.TechnicianOpportunities", new[] { "CreatedByHROfficerID" });
            DropColumn("dbo.TechnicianOpportunities", "LastUpdatedByHROfficerID");
            DropColumn("dbo.TechnicianOpportunities", "CreatedByHROfficerID");

            // TechnicianOnboardings
            DropForeignKey("dbo.TechnicianOnboardings", "OnboardedByHROfficerID", "dbo.HROfficers");
            DropIndex("dbo.TechnicianOnboardings", new[] { "OnboardedByHROfficerID" });
            DropColumn("dbo.TechnicianOnboardings", "OnboardedByHROfficerID");

            // ApplicationInterviews
            DropForeignKey("dbo.ApplicationInterviews", "RecordedByHROfficerID", "dbo.HROfficers");
            DropIndex("dbo.ApplicationInterviews", new[] { "RecordedByHROfficerID" });
            DropColumn("dbo.ApplicationInterviews", "RecordedByHROfficerID");

            // ApplicationAssessments
            DropForeignKey("dbo.ApplicationAssessments", "RecordedByHROfficerID", "dbo.HROfficers");
            DropIndex("dbo.ApplicationAssessments", new[] { "RecordedByHROfficerID" });
            DropColumn("dbo.ApplicationAssessments", "RecordedByHROfficerID");
        }
    }
}
