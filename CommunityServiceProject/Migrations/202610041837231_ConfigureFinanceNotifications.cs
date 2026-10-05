namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConfigureFinanceNotifications : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FinanceNotifications",
                c => new
                    {
                        FinanceNotificationID = c.Int(nullable: false, identity: true),
                        CitizenID = c.Int(nullable: false),
                        InvoiceID = c.Int(),
                        PaymentID = c.Int(),
                        RefundID = c.Int(),
                        ReceiptID = c.Int(),
                        NotificationType = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 150),
                        Message = c.String(nullable: false, maxLength: 2000),
                        DateCreated = c.DateTime(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                        ReadDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.FinanceNotificationID)
                .ForeignKey("dbo.Citizens", t => t.CitizenID)
                .ForeignKey("dbo.Invoices", t => t.InvoiceID)
                .ForeignKey("dbo.Payments", t => t.PaymentID)
                .ForeignKey("dbo.Receipts", t => t.ReceiptID)
                .ForeignKey("dbo.Refunds", t => t.RefundID)
                .Index(t => t.CitizenID)
                .Index(t => t.InvoiceID)
                .Index(t => t.PaymentID)
                .Index(t => t.RefundID)
                .Index(t => t.ReceiptID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FinanceNotifications", "RefundID", "dbo.Refunds");
            DropForeignKey("dbo.FinanceNotifications", "ReceiptID", "dbo.Receipts");
            DropForeignKey("dbo.FinanceNotifications", "PaymentID", "dbo.Payments");
            DropForeignKey("dbo.FinanceNotifications", "InvoiceID", "dbo.Invoices");
            DropForeignKey("dbo.FinanceNotifications", "CitizenID", "dbo.Citizens");
            DropIndex("dbo.FinanceNotifications", new[] { "ReceiptID" });
            DropIndex("dbo.FinanceNotifications", new[] { "RefundID" });
            DropIndex("dbo.FinanceNotifications", new[] { "PaymentID" });
            DropIndex("dbo.FinanceNotifications", new[] { "InvoiceID" });
            DropIndex("dbo.FinanceNotifications", new[] { "CitizenID" });
            DropTable("dbo.FinanceNotifications");
        }
    }
}
