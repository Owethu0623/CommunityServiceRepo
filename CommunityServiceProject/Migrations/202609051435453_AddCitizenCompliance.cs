namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCitizenCompliance : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AccountRestrictions",
                c => new
                    {
                        RestrictionID = c.Int(nullable: false, identity: true),
                        CitizenID = c.Int(nullable: false),
                        AdministratorID = c.Int(nullable: false),
                        RestrictionType = c.String(nullable: false, maxLength: 50),
                        Reason = c.String(nullable: false, maxLength: 2000),
                        DateStarted = c.DateTime(nullable: false),
                        DateEnded = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.RestrictionID)
                .ForeignKey("dbo.Administrators", t => t.AdministratorID)
                .ForeignKey("dbo.Citizens", t => t.CitizenID)
                .Index(t => t.CitizenID)
                .Index(t => t.AdministratorID);
            
            CreateTable(
                "dbo.ComplianceRecords",
                c => new
                    {
                        ComplianceID = c.Int(nullable: false, identity: true),
                        CitizenID = c.Int(nullable: false),
                        ConfirmedViolationCount = c.Int(nullable: false),
                        ComplianceStatus = c.Int(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ComplianceID)
                .ForeignKey("dbo.Citizens", t => t.CitizenID)
                .Index(t => t.CitizenID);
            
            CreateTable(
                "dbo.Violations",
                c => new
                    {
                        ViolationID = c.Int(nullable: false, identity: true),
                        ComplianceID = c.Int(nullable: false),
                        RequestID = c.Int(),
                        AdministratorID = c.Int(nullable: false),
                        ViolationType = c.String(nullable: false, maxLength: 100),
                        Description = c.String(nullable: false, maxLength: 2000),
                        DateConfirmed = c.DateTime(nullable: false),
                        Status = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.ViolationID)
                .ForeignKey("dbo.Administrators", t => t.AdministratorID)
                .ForeignKey("dbo.ComplianceRecords", t => t.ComplianceID)
                .ForeignKey("dbo.Requests", t => t.RequestID)
                .Index(t => t.ComplianceID)
                .Index(t => t.RequestID)
                .Index(t => t.AdministratorID);
            
            CreateTable(
                "dbo.Warnings",
                c => new
                    {
                        WarningID = c.Int(nullable: false, identity: true),
                        ViolationID = c.Int(nullable: false),
                        AdministratorID = c.Int(nullable: false),
                        WarningReason = c.String(nullable: false, maxLength: 2000),
                        DateIssued = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.WarningID)
                .ForeignKey("dbo.Administrators", t => t.AdministratorID)
                .ForeignKey("dbo.Violations", t => t.ViolationID)
                .Index(t => t.ViolationID)
                .Index(t => t.AdministratorID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Warnings", "ViolationID", "dbo.Violations");
            DropForeignKey("dbo.Warnings", "AdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.Violations", "RequestID", "dbo.Requests");
            DropForeignKey("dbo.Violations", "ComplianceID", "dbo.ComplianceRecords");
            DropForeignKey("dbo.Violations", "AdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.ComplianceRecords", "CitizenID", "dbo.Citizens");
            DropForeignKey("dbo.AccountRestrictions", "CitizenID", "dbo.Citizens");
            DropForeignKey("dbo.AccountRestrictions", "AdministratorID", "dbo.Administrators");
            DropIndex("dbo.Warnings", new[] { "AdministratorID" });
            DropIndex("dbo.Warnings", new[] { "ViolationID" });
            DropIndex("dbo.Violations", new[] { "AdministratorID" });
            DropIndex("dbo.Violations", new[] { "RequestID" });
            DropIndex("dbo.Violations", new[] { "ComplianceID" });
            DropIndex("dbo.ComplianceRecords", new[] { "CitizenID" });
            DropIndex("dbo.AccountRestrictions", new[] { "AdministratorID" });
            DropIndex("dbo.AccountRestrictions", new[] { "CitizenID" });
            DropTable("dbo.Warnings");
            DropTable("dbo.Violations");
            DropTable("dbo.ComplianceRecords");
            DropTable("dbo.AccountRestrictions");
        }
    }
}
