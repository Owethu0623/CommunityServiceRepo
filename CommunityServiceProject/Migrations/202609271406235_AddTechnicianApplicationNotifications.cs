namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTechnicianApplicationNotifications : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TechnicianApplicationNotifications",
                c => new
                    {
                        TechnicianApplicationNotificationID = c.Int(nullable: false, identity: true),
                        ApplicationID = c.Int(nullable: false),
                        CitizenID = c.Int(nullable: false),
                        NotificationType = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 150),
                        Message = c.String(nullable: false, maxLength: 2000),
                        DateCreated = c.DateTime(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                        ReadDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.TechnicianApplicationNotificationID)
                .ForeignKey("dbo.TechnicianApplications", t => t.ApplicationID)
                .ForeignKey("dbo.Citizens", t => t.CitizenID)
                .Index(t => t.ApplicationID)
                .Index(t => t.CitizenID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TechnicianApplicationNotifications", "CitizenID", "dbo.Citizens");
            DropForeignKey("dbo.TechnicianApplicationNotifications", "ApplicationID", "dbo.TechnicianApplications");
            DropIndex("dbo.TechnicianApplicationNotifications", new[] { "CitizenID" });
            DropIndex("dbo.TechnicianApplicationNotifications", new[] { "ApplicationID" });
            DropTable("dbo.TechnicianApplicationNotifications");
        }
    }
}
