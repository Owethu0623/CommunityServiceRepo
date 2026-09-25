namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMunicipalProjectLocationCoordinates : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MunicipalProjects", "LocationDescription", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MunicipalProjects", "LocationDescription");
        }
    }
}
