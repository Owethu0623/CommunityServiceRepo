namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateRequestModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Requests", "LocationDescription", c => c.String(nullable: false, maxLength: 200));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Requests", "LocationDescription");
        }
    }
}
