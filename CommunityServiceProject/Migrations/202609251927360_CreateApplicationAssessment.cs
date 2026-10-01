namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateApplicationAssessment : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ApplicationAssessments",
                c => new
                    {
                        AssessmentID = c.Int(nullable: false, identity: true),
                        ApplicationID = c.Int(nullable: false),
                        AssessmentType = c.String(nullable: false, maxLength: 100),
                        AssessmentDate = c.DateTime(nullable: false),
                        Location = c.String(nullable: false, maxLength: 300),
                        Instructions = c.String(maxLength: 3000),
                        Status = c.Int(nullable: false),
                        Result = c.String(maxLength: 2000),
                        Score = c.Decimal(precision: 18, scale: 2),
                        Comments = c.String(maxLength: 3000),
                        RecordedByAdministratorID = c.Int(nullable: false),
                        DateRecorded = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.AssessmentID)
.ForeignKey(
    "dbo.TechnicianApplications",
    t => t.ApplicationID,
    cascadeDelete: false)
.ForeignKey(
    "dbo.Administrators",
    t => t.RecordedByAdministratorID,
    cascadeDelete: false)
                .Index(t => t.ApplicationID)
                .Index(t => t.RecordedByAdministratorID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ApplicationAssessments", "RecordedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.ApplicationAssessments", "ApplicationID", "dbo.TechnicianApplications");
            DropIndex("dbo.ApplicationAssessments", new[] { "RecordedByAdministratorID" });
            DropIndex("dbo.ApplicationAssessments", new[] { "ApplicationID" });
            DropTable("dbo.ApplicationAssessments");
        }
    }
}
