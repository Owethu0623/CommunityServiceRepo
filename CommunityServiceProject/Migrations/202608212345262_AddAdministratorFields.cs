using System;
using System.Data.Entity.Migrations;

namespace CommunityServiceProject.Migrations
{
    public partial class AddAdministratorFields : DbMigration
    {
        public override void Up()
        {
            // Remove the old relationship between Requests and Administrators
            DropForeignKey(
                "dbo.Requests",
                "Administrator_user",
                "dbo.Administrators");

            // Remove the old AdministratorID column if it exists
            DropColumn(
                "dbo.Requests",
                "AdministratorID");

            // Remove the old Administrators table
            DropTable("dbo.Administrators");

            // Recreate the Administrators table correctly
            CreateTable(
                "dbo.Administrators",
                c => new
                {
                    AdministratorID = c.Int(nullable: false, identity: true),
                    FirstName = c.String(nullable: false, maxLength: 50),
                    LastName = c.String(nullable: false, maxLength: 50),
                    EmailAddress = c.String(nullable: false, maxLength: 100),
                    PhoneNumber = c.String(nullable: false, maxLength: 20),
                    Password = c.String(nullable: false, maxLength: 100),
                    AccountStatus = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.AdministratorID);

            // Rename the existing Request relationship column
            RenameColumn(
                table: "dbo.Requests",
                name: "Administrator_user",
                newName: "AdministratorID");

            RenameIndex(
                table: "dbo.Requests",
                name: "IX_Administrator_user",
                newName: "IX_AdministratorID");

            // Reconnect Requests to the new Administrators table
            AddForeignKey(
                "dbo.Requests",
                "AdministratorID",
                "dbo.Administrators",
                "AdministratorID");
        }

        public override void Down()
        {
            DropForeignKey(
                "dbo.Requests",
                "AdministratorID",
                "dbo.Administrators");

            DropIndex(
                "dbo.Requests",
                "IX_AdministratorID");

            RenameColumn(
                table: "dbo.Requests",
                name: "AdministratorID",
                newName: "Administrator_user");

            DropTable("dbo.Administrators");

            AddColumn(
                "dbo.Administrators",
                "user",
                c => c.Int(nullable: false, identity: true));

            AddPrimaryKey(
                "dbo.Administrators",
                "user");

            AddForeignKey(
                "dbo.Requests",
                "Administrator_user",
                "dbo.Administrators",
                "user");
        }
    }
}