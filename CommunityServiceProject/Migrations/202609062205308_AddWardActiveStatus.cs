namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddWardActiveStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn(
                "dbo.Wards",
                "IsActive",
                c => c.Boolean(
                    nullable: false,
                    defaultValue: true
                )
            );
        }

        public override void Down()
        {
            DropColumn("dbo.Wards", "IsActive");
        }
    }
}