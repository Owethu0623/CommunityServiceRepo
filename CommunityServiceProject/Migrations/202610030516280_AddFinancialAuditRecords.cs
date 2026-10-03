namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFinancialAuditRecords : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FinancialAuditRecords",
                c => new
                    {
                        FinancialAuditRecordID = c.Int(nullable: false, identity: true),
                        AuditDate = c.DateTime(nullable: false),
                        FinanceOfficerID = c.Int(nullable: false),
                        PerformedBy = c.String(nullable: false, maxLength: 100),
                        Action = c.String(nullable: false, maxLength: 100),
                        EntityType = c.String(nullable: false, maxLength: 50),
                        EntityID = c.Int(nullable: false),
                        Reference = c.String(maxLength: 100),
                        PreviousStatus = c.String(maxLength: 50),
                        NewStatus = c.String(maxLength: 50),
                        Amount = c.Decimal(precision: 18, scale: 2),
                        Details = c.String(maxLength: 1000),
                        IPAddress = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.FinancialAuditRecordID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.FinancialAuditRecords");
        }
    }
}
