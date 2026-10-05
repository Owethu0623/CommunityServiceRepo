using System;
using System.Data.Entity.Migrations;

namespace CommunityServiceProject.Migrations
{
    public partial class AddRequestComplianceFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Requests", "ComplianceConfirmed", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("dbo.Requests", "ComplianceConfirmedDate", c => c.DateTime());
        }

        public override void Down()
        {
            DropColumn("dbo.Requests", "ComplianceConfirmedDate");
            DropColumn("dbo.Requests", "ComplianceConfirmed");
        }
    }
}
