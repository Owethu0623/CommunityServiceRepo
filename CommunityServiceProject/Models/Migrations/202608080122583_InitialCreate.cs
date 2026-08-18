namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Administrators",
                c => new
                    {
                        user = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.user);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        CategoryID = c.Int(nullable: false, identity: true),
                        CategoryName = c.String(nullable: false, maxLength: 50),
                        Description = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.CategoryID);
            
            CreateTable(
                "dbo.Requests",
                c => new
                    {
                        RequestID = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 100),
                        Description = c.String(nullable: false),
                        DateSubmitted = c.DateTime(nullable: false),
                        Status = c.Int(nullable: false),
                        Latitude = c.Double(nullable: false),
                        Longitude = c.Double(nullable: false),
                        ImagePath = c.String(),
                        CitizenID = c.Int(nullable: false),
                        AdministratorID = c.Int(),
                        TechnicianID = c.Int(),
                        CategoryID = c.Int(nullable: false),
                        Administrator_user = c.Int(),
                        Technician_user = c.Int(),
                    })
                .PrimaryKey(t => t.RequestID)
                .ForeignKey("dbo.Administrators", t => t.Administrator_user)
                .ForeignKey("dbo.Categories", t => t.CategoryID, cascadeDelete: true)
                .ForeignKey("dbo.Citizens", t => t.CitizenID, cascadeDelete: true)
                .ForeignKey("dbo.Technicians", t => t.Technician_user)
                .Index(t => t.CitizenID)
                .Index(t => t.CategoryID)
                .Index(t => t.Administrator_user)
                .Index(t => t.Technician_user);
            
            CreateTable(
                "dbo.Citizens",
                c => new
                    {
                        CitizenID = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false, maxLength: 50),
                        LastName = c.String(nullable: false, maxLength: 50),
                        EmailAddress = c.String(nullable: false),
                        PhoneNumber = c.String(nullable: false),
                        Password = c.String(nullable: false),
                        ResidentialAddress = c.String(nullable: false, maxLength: 200),
                        DateRegistered = c.DateTime(nullable: false),
                        AccountStatus = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CitizenID);
            
            CreateTable(
                "dbo.Technicians",
                c => new
                    {
                        user = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.user);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Requests", "Technician_user", "dbo.Technicians");
            DropForeignKey("dbo.Requests", "CitizenID", "dbo.Citizens");
            DropForeignKey("dbo.Requests", "CategoryID", "dbo.Categories");
            DropForeignKey("dbo.Requests", "Administrator_user", "dbo.Administrators");
            DropIndex("dbo.Requests", new[] { "Technician_user" });
            DropIndex("dbo.Requests", new[] { "Administrator_user" });
            DropIndex("dbo.Requests", new[] { "CategoryID" });
            DropIndex("dbo.Requests", new[] { "CitizenID" });
            DropTable("dbo.Technicians");
            DropTable("dbo.Citizens");
            DropTable("dbo.Requests");
            DropTable("dbo.Categories");
            DropTable("dbo.Administrators");
        }
    }
}
