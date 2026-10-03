namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRefundReviewAndSupportingDocuments : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Refunds", "ReviewDate", c => c.DateTime());
            AddColumn("dbo.Refunds", "ReviewedByFinanceOfficerID", c => c.Int());
            AddColumn("dbo.Refunds", "ReviewComments", c => c.String(maxLength: 500));
            AddColumn("dbo.Refunds", "SupportingDocumentPath", c => c.String(maxLength: 500));
            AddColumn("dbo.Refunds", "SupportingDocumentName", c => c.String(maxLength: 255));
            CreateIndex("dbo.Refunds", "RefundReference", unique: true, name: "IX_Refund_RefundReference");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Refunds", "IX_Refund_RefundReference");
            DropColumn("dbo.Refunds", "SupportingDocumentName");
            DropColumn("dbo.Refunds", "SupportingDocumentPath");
            DropColumn("dbo.Refunds", "ReviewComments");
            DropColumn("dbo.Refunds", "ReviewedByFinanceOfficerID");
            DropColumn("dbo.Refunds", "ReviewDate");
        }
    }
}
