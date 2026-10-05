namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddAdministratorNotifications : DbMigration
    {
        public override void Up()
        {
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
        }

        public override void Down()
        {
            DropForeignKey("dbo.AdministratorNotifications", "ApplicationID", "dbo.TechnicianApplications");
            DropForeignKey("dbo.AdministratorNotifications", "AdministratorID", "dbo.Administrators");
            DropIndex("dbo.AdministratorNotifications", new[] { "ApplicationID" });
            DropIndex("dbo.AdministratorNotifications", new[] { "AdministratorID" });
            DropTable("dbo.AdministratorNotifications");
        }
    }
}
