namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTechnicianApplication : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TechnicianApplications",
                c => new
                    {
                        ApplicationID = c.Int(nullable: false, identity: true),
                        ApplicationReference = c.String(nullable: false, maxLength: 50),
                        OpportunityID = c.Int(nullable: false),
                        CitizenID = c.Int(nullable: false),
                        ApplicationDate = c.DateTime(nullable: false),
                        Status = c.Int(nullable: false),
                        CoverLetter = c.String(nullable: false, maxLength: 4000),
                        LastUpdatedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.ApplicationID)
                .ForeignKey("dbo.Citizens", t => t.CitizenID, cascadeDelete: true)
                .ForeignKey("dbo.TechnicianOpportunities", t => t.OpportunityID, cascadeDelete: true)
                .Index(t => new { t.OpportunityID, t.CitizenID }, unique: true, name: "IX_TechnicianApplication_Opportunity_Citizen")
                .Index(t => new { t.OpportunityID, t.CitizenID }, unique: true);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TechnicianApplications", "OpportunityID", "dbo.TechnicianOpportunities");
            DropForeignKey("dbo.TechnicianApplications", "CitizenID", "dbo.Citizens");
            DropIndex("dbo.TechnicianApplications", new[] { "OpportunityID", "CitizenID" });
            DropIndex("dbo.TechnicianApplications", "IX_TechnicianApplication_Opportunity_Citizen");
            DropTable("dbo.TechnicianApplications");
        }
    }
}
