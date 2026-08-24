namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTechnicianFields : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Requests", "Technician_user", "dbo.Technicians");

            DropIndex("dbo.Requests", new[] { "Technician_user" });

            DropTable("dbo.Technicians");

            CreateTable(
                "dbo.Technicians",
                c => new
                {
                    TechnicianID = c.Int(nullable: false, identity: true),
                    FirstName = c.String(nullable: false, maxLength: 50),
                    LastName = c.String(nullable: false, maxLength: 50),
                    EmailAddress = c.String(nullable: false, maxLength: 100),
                    PhoneNumber = c.String(nullable: false, maxLength: 20),
                    Password = c.String(nullable: false, maxLength: 100),
                    AccountStatus = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.TechnicianID);

            DropColumn("dbo.Requests", "TechnicianID");

            RenameColumn(
                table: "dbo.Requests",
                name: "Technician_user",
                newName: "TechnicianID"
            );

            CreateIndex(
                "dbo.Requests",
                "TechnicianID"
            );

            AddForeignKey(
                "dbo.Requests",
                "TechnicianID",
                "dbo.Technicians",
                "TechnicianID"
            );
        }

        public override void Down()
        {
            AddColumn("dbo.Technicians", "user", c => c.Int(nullable: false, identity: true));
            DropForeignKey("dbo.Requests", "TechnicianID", "dbo.Technicians");
            DropPrimaryKey("dbo.Technicians");
            DropColumn("dbo.Technicians", "AccountStatus");
            DropColumn("dbo.Technicians", "Password");
            DropColumn("dbo.Technicians", "PhoneNumber");
            DropColumn("dbo.Technicians", "EmailAddress");
            DropColumn("dbo.Technicians", "LastName");
            DropColumn("dbo.Technicians", "FirstName");
            DropColumn("dbo.Technicians", "TechnicianID");
            AddPrimaryKey("dbo.Technicians", "user");
            RenameIndex(table: "dbo.Requests", name: "IX_TechnicianID", newName: "IX_Technician_user");
            RenameColumn(table: "dbo.Requests", name: "TechnicianID", newName: "Technician_user");
            AddColumn("dbo.Requests", "TechnicianID", c => c.Int());
            AddForeignKey("dbo.Requests", "Technician_user", "dbo.Technicians", "user");
        }
    }
}
