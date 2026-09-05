namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddWardToRequests : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Wards",
                c => new
                {
                    WardID = c.Int(nullable: false, identity: true),
                    WardName = c.String(nullable: false, maxLength: 100),
                    WardNumber = c.String(nullable: false, maxLength: 20),
                    Description = c.String(maxLength: 250),
                })
                .PrimaryKey(t => t.WardID);

            Sql(@"
        INSERT INTO dbo.Wards
            (WardName, WardNumber, Description)
        VALUES
            ('Ward 1', '1', 'Default municipal ward');
    ");

            AddColumn(
                "dbo.Requests",
                "WardID",
                c => c.Int(nullable: false, defaultValue: 1)
            );

            CreateIndex("dbo.Requests", "WardID");

            AddForeignKey(
                "dbo.Requests",
                "WardID",
                "dbo.Wards",
                "WardID"
            );
        }

        public override void Down()
        {
            DropForeignKey("dbo.Requests", "WardID", "dbo.Wards");
            DropIndex("dbo.Requests", new[] { "WardID" });
            DropColumn("dbo.Requests", "WardID");
            DropTable("dbo.Wards");
        }
    }
}
