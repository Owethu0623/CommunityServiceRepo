namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MakeTechnicianOpportunityCodeUnique : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.TechnicianOpportunities", "OpportunityCode", unique: true, name: "IX_TechnicianOpportunity_OpportunityCode");
        }
        
        public override void Down()
        {
            DropIndex("dbo.TechnicianOpportunities", "IX_TechnicianOpportunity_OpportunityCode");
        }
    }
}
