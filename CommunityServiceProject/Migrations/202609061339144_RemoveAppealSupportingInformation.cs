namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveAppealSupportingInformation : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Appeals", "SupportingInformation");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Appeals", "SupportingInformation", c => c.String(maxLength: 2000));
        }
    }
}
