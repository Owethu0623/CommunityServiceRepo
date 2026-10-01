namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTechnicianMustChangePassword : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Technicians", "MustChangePassword", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Technicians", "MustChangePassword");
        }
    }
}
