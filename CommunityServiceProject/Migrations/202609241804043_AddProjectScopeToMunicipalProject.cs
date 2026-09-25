namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProjectScopeToMunicipalProject : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MunicipalProjects", "ProjectScope", c => c.String(maxLength: 4000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MunicipalProjects", "ProjectScope");
        }
    }
}
