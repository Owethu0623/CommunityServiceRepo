namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveLocationDescription : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Requests", "LocationDescription");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Requests", "LocationDescription", c => c.String(nullable: false, maxLength: 200));
        }
    }
}
