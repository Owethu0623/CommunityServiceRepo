namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRequestReferenceNumber : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Requests", "ReferenceNumber", c => c.String(maxLength: 30));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Requests", "ReferenceNumber");
        }
    }
}
