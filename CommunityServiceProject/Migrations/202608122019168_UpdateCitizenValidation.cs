namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateCitizenValidation : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Citizens", "Password", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Citizens", "Password", c => c.String(nullable: false));
        }
    }
}
