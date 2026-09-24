namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNotifications : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        NotificationID = c.Int(nullable: false, identity: true),
                        CitizenID = c.Int(nullable: false),
                        RequestID = c.Int(),
                        Message = c.String(nullable: false, maxLength: 500),
                        DateCreated = c.DateTime(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.NotificationID)
                .ForeignKey("dbo.Citizens", t => t.CitizenID, cascadeDelete: true)
                .ForeignKey("dbo.Requests", t => t.RequestID)
                .Index(t => t.CitizenID)
                .Index(t => t.RequestID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Notifications", "RequestID", "dbo.Requests");
            DropForeignKey("dbo.Notifications", "CitizenID", "dbo.Citizens");
            DropIndex("dbo.Notifications", new[] { "RequestID" });
            DropIndex("dbo.Notifications", new[] { "CitizenID" });
            DropTable("dbo.Notifications");
        }
    }
}
