namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddWardToMunicipalAsset : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MunicipalAssets", "WardID", c => c.Int(nullable: false));

            Sql(@"
        UPDATE MunicipalAssets
        SET WardID =
            CASE AssetCode
                WHEN 'AST-000001' THEN 28
                WHEN 'AST-000002' THEN 32
            END
        WHERE AssetCode IN ('AST-000001', 'AST-000002');
    ");

            AddForeignKey(
                "dbo.MunicipalAssets",
                "WardID",
                "dbo.Wards",
                "WardID",
                cascadeDelete: false);

            CreateIndex(
                "dbo.MunicipalAssets",
                "WardID");
        }

        public override void Down()
        {
            DropForeignKey(
                "dbo.MunicipalAssets",
                "WardID",
                "dbo.Wards");

            DropIndex(
                "dbo.MunicipalAssets",
                new[] { "WardID" });

            DropColumn(
                "dbo.MunicipalAssets",
                "WardID");
        }
    }
}
