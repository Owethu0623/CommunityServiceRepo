namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddApprovedDateToRequest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Requests", "ApprovedDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Requests", "ApprovedDate");
        }
    }
}
