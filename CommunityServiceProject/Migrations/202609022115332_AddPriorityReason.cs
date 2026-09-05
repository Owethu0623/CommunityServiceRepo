namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPriorityReason : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Requests", "ProblemLocation", c => c.String(nullable: false, maxLength: 500));
            AddColumn("dbo.Requests", "PriorityReason", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Requests", "PriorityReason");
            DropColumn("dbo.Requests", "ProblemLocation");
        }
    }
}
