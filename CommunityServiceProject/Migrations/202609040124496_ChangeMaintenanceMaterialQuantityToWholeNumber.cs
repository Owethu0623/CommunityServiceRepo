namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeMaintenanceMaterialQuantityToWholeNumber : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.MaintenanceMaterials", "Quantity", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.MaintenanceMaterials", "Quantity", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
