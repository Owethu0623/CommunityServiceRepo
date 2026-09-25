namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTechnicianOpportunity : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TechnicianOpportunities",
                c => new
                    {
                        OpportunityID = c.Int(nullable: false, identity: true),
                        OpportunityCode = c.String(nullable: false, maxLength: 50),
                        Title = c.String(nullable: false, maxLength: 200),
                        Description = c.String(nullable: false, maxLength: 4000),
                        Responsibilities = c.String(nullable: false, maxLength: 4000),
                        Requirements = c.String(nullable: false, maxLength: 4000),
                        RequiredQualifications = c.String(nullable: false, maxLength: 2000),
                        RequiredExperience = c.String(maxLength: 2000),
                        ApplicationInstructions = c.String(maxLength: 2000),
                        ApplicationStartDate = c.DateTime(nullable: false),
                        ApplicationDeadline = c.DateTime(nullable: false),
                        Status = c.Int(nullable: false),
                        EmploymentType = c.String(maxLength: 100),
                        NumberOfPositions = c.Int(),
                        DateCreated = c.DateTime(nullable: false),
                        CreatedByAdministratorID = c.Int(nullable: false),
                        PublishedDate = c.DateTime(),
                        ClosedDate = c.DateTime(),
                        LastUpdatedDate = c.DateTime(),
                        LastUpdatedByAdministratorID = c.Int(),
                    })
                .PrimaryKey(t => t.OpportunityID)
                .ForeignKey("dbo.Administrators", t => t.CreatedByAdministratorID, cascadeDelete: true)
                .ForeignKey("dbo.Administrators", t => t.LastUpdatedByAdministratorID)
                .Index(t => t.CreatedByAdministratorID)
                .Index(t => t.LastUpdatedByAdministratorID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TechnicianOpportunities", "LastUpdatedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.TechnicianOpportunities", "CreatedByAdministratorID", "dbo.Administrators");
            DropIndex("dbo.TechnicianOpportunities", new[] { "LastUpdatedByAdministratorID" });
            DropIndex("dbo.TechnicianOpportunities", new[] { "CreatedByAdministratorID" });
            DropTable("dbo.TechnicianOpportunities");
        }
    }
}
