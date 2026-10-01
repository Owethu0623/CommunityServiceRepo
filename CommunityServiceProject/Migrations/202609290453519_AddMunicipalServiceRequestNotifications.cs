namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMunicipalServiceRequestNotifications : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MunicipalServiceRequestNotifications",
                c => new
                    {
                        MunicipalServiceRequestNotificationID = c.Int(nullable: false, identity: true),
                        CitizenID = c.Int(nullable: false),
                        MunicipalServiceRequestID = c.Int(nullable: false),
                        NotificationType = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 150),
                        Message = c.String(nullable: false, maxLength: 2000),
                        DateCreated = c.DateTime(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                        ReadDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.MunicipalServiceRequestNotificationID)
                .ForeignKey("dbo.Citizens", t => t.CitizenID, cascadeDelete: true)
                .ForeignKey("dbo.MunicipalServiceRequests", t => t.MunicipalServiceRequestID, cascadeDelete: true)
                .Index(t => t.CitizenID)
                .Index(t => t.MunicipalServiceRequestID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MunicipalServiceRequestNotifications", "MunicipalServiceRequestID", "dbo.MunicipalServiceRequests");
            DropForeignKey("dbo.MunicipalServiceRequestNotifications", "CitizenID", "dbo.Citizens");
            DropIndex("dbo.MunicipalServiceRequestNotifications", new[] { "MunicipalServiceRequestID" });
            DropIndex("dbo.MunicipalServiceRequestNotifications", new[] { "CitizenID" });
            DropTable("dbo.MunicipalServiceRequestNotifications");
        }
    }
}
