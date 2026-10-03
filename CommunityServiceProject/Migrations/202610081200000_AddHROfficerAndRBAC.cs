namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddHROfficerAndRBAC : DbMigration
    {
        public override void Up()
        {
            // Migration assumes HROfficers table may already exist from automatic migration.
            // If it doesn't, create it here to ensure deployment safety.
            CreateTable(
                "dbo.HROfficers",
                c => new
                    {
                        HROfficerID = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false, maxLength: 50),
                        LastName = c.String(nullable: false, maxLength: 50),
                        EmailAddress = c.String(nullable: false, maxLength: 150),
                        Password = c.String(nullable: false),
                        AccountStatus = c.Int(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.HROfficerID)
                .Index(t => t.EmailAddress, unique: true, name: "IX_HROfficer_Email");
        }

        public override void Down()
        {
            DropIndex("dbo.HROfficers", "IX_HROfficer_Email");
            DropTable("dbo.HROfficers");
        }
    }
}
