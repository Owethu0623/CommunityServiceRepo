namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAppeal : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Appeals",
                c => new
                    {
                        AppealID = c.Int(nullable: false, identity: true),
                        ReferenceNumber = c.String(nullable: false, maxLength: 30),
                        CitizenID = c.Int(nullable: false),
                        RestrictionID = c.Int(nullable: false),
                        AdministratorID = c.Int(),
                        Reason = c.String(nullable: false, maxLength: 2000),
                        SupportingInformation = c.String(maxLength: 2000),
                        SupportingDocumentPath = c.String(maxLength: 500),
                        DateSubmitted = c.DateTime(nullable: false),
                        DateReviewed = c.DateTime(),
                        DateDecision = c.DateTime(),
                        Status = c.Int(nullable: false),
                        DecisionReason = c.String(maxLength: 2000),
                    })
                .PrimaryKey(t => t.AppealID)
                .ForeignKey("dbo.Administrators", t => t.AdministratorID)
                .ForeignKey("dbo.Citizens", t => t.CitizenID)
                .ForeignKey("dbo.AccountRestrictions", t => t.RestrictionID)
                .Index(t => t.CitizenID)
                .Index(t => t.RestrictionID)
                .Index(t => t.AdministratorID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Appeals", "RestrictionID", "dbo.AccountRestrictions");
            DropForeignKey("dbo.Appeals", "CitizenID", "dbo.Citizens");
            DropForeignKey("dbo.Appeals", "AdministratorID", "dbo.Administrators");
            DropIndex("dbo.Appeals", new[] { "AdministratorID" });
            DropIndex("dbo.Appeals", new[] { "RestrictionID" });
            DropIndex("dbo.Appeals", new[] { "CitizenID" });
            DropTable("dbo.Appeals");
        }
    }
}
