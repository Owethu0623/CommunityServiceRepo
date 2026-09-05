namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MakeRequestReferenceRequiredUnique : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Requests", "ReferenceNumber", c => c.String(nullable: false, maxLength: 30));
            CreateIndex("dbo.Requests", "ReferenceNumber", unique: true, name: "IX_Request_ReferenceNumber");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Requests", "IX_Request_ReferenceNumber");
            AlterColumn("dbo.Requests", "ReferenceNumber", c => c.String(maxLength: 30));
        }
    }
}
