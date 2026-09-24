namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAppealApprovalAcknowledged : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appeals", "ApprovalAcknowledged", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appeals", "ApprovalAcknowledged");
        }
    }
}
