namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExpandAssetHistoryValues : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AssetHistories", "PreviousValue", c => c.String(maxLength: 2000));
            AlterColumn("dbo.AssetHistories", "NewValue", c => c.String(maxLength: 2000));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AssetHistories", "NewValue", c => c.String(maxLength: 100));
            AlterColumn("dbo.AssetHistories", "PreviousValue", c => c.String(maxLength: 100));
        }
    }
}
