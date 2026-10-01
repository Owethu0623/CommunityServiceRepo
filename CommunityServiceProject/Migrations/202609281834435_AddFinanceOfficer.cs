namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFinanceOfficer : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FinanceOfficers",
                c => new
                    {
                        FinanceOfficerID = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false, maxLength: 50),
                        LastName = c.String(nullable: false, maxLength: 50),
                        EmailAddress = c.String(nullable: false, maxLength: 150),
                        Password = c.String(nullable: false),
                        AccountStatus = c.Int(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.FinanceOfficerID)
                .Index(t => t.EmailAddress, unique: true);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.FinanceOfficers", new[] { "EmailAddress" });
            DropTable("dbo.FinanceOfficers");
        }
    }
}
