namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MakeRequestLocationOptional : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Requests", "Latitude", c => c.Double());
            AlterColumn("dbo.Requests", "Longitude", c => c.Double());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Requests", "Longitude", c => c.Double(nullable: false));
            AlterColumn("dbo.Requests", "Latitude", c => c.Double(nullable: false));
        }
    }
}
