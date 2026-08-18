namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateRequestGPS : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Requests", "Latitude", c => c.String());
            AlterColumn("dbo.Requests", "Longitude", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Requests", "Longitude", c => c.Double());
            AlterColumn("dbo.Requests", "Latitude", c => c.Double());
        }
    }
}
