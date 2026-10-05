using System;
using System.Data.Entity.Migrations;

namespace CommunityServiceProject.Migrations
{
    public partial class AddHROfficerNotifications : DbMigration
    {
        public override void Up()
        {
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
                .ForeignKey("dbo.HROfficers", t => t.HROfficerID, cascadeDelete: true)
                .ForeignKey("dbo.TechnicianApplications", t => t.ApplicationID)
                .Index(t => t.HROfficerID)
                .Index(t => t.ApplicationID);
        }

        public override void Down()
        {
            DropForeignKey("dbo.HROfficerNotifications", "ApplicationID", "dbo.TechnicianApplications");
            DropForeignKey("dbo.HROfficerNotifications", "HROfficerID", "dbo.HROfficers");
            DropIndex("dbo.HROfficerNotifications", new[] { "ApplicationID" });
            DropIndex("dbo.HROfficerNotifications", new[] { "HROfficerID" });
            DropTable("dbo.HROfficerNotifications");
        }
    }
}
