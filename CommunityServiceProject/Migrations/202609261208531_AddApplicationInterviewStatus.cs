namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddApplicationInterviewStatus : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ApplicationInterviews",
                c => new
                    {
                        InterviewID = c.Int(nullable: false, identity: true),
                        ApplicationID = c.Int(nullable: false),
                        InterviewDate = c.DateTime(nullable: false),
                        Location = c.String(nullable: false, maxLength: 300),
                        InterviewMethod = c.String(nullable: false, maxLength: 100),
                        Status = c.Int(nullable: false),
                        Outcome = c.String(maxLength: 2000),
                        Comments = c.String(maxLength: 3000),
                        RecordedByAdministratorID = c.Int(nullable: false),
                        DateRecorded = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.InterviewID)
                .ForeignKey("dbo.TechnicianApplications", t => t.ApplicationID, cascadeDelete: false)
                .ForeignKey("dbo.Administrators", t => t.RecordedByAdministratorID, cascadeDelete: false)
                .Index(t => t.ApplicationID)
                .Index(t => t.RecordedByAdministratorID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ApplicationInterviews", "RecordedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.ApplicationInterviews", "ApplicationID", "dbo.TechnicianApplications");
            DropIndex("dbo.ApplicationInterviews", new[] { "RecordedByAdministratorID" });
            DropIndex("dbo.ApplicationInterviews", new[] { "ApplicationID" });
            DropTable("dbo.ApplicationInterviews");
        }
    }
}
