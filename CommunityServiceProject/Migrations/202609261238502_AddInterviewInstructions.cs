namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddInterviewInstructions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ApplicationInterviews", "Instructions", c => c.String(maxLength: 3000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ApplicationInterviews", "Instructions");
        }
    }
}
