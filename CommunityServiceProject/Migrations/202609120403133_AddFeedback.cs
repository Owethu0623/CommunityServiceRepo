namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFeedback : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Feedbacks",
                c => new
                    {
                        FeedbackID = c.Int(nullable: false),
                        RequestID = c.Int(nullable: false),
                        CitizenID = c.Int(nullable: false),
                        Rating = c.Int(nullable: false),
                        Comment = c.String(maxLength: 2000),
                        ResolutionStatus = c.Int(nullable: false),
                        DateSubmitted = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.FeedbackID)
                .ForeignKey("dbo.Citizens", t => t.CitizenID)
                .ForeignKey("dbo.Requests", t => t.FeedbackID)
                .Index(t => t.FeedbackID)
                .Index(t => t.RequestID, unique: true, name: "IX_Feedback_RequestID")
                .Index(t => t.CitizenID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Feedbacks", "FeedbackID", "dbo.Requests");
            DropForeignKey("dbo.Feedbacks", "CitizenID", "dbo.Citizens");
            DropIndex("dbo.Feedbacks", new[] { "CitizenID" });
            DropIndex("dbo.Feedbacks", "IX_Feedback_RequestID");
            DropIndex("dbo.Feedbacks", new[] { "FeedbackID" });
            DropTable("dbo.Feedbacks");
        }
    }
}
