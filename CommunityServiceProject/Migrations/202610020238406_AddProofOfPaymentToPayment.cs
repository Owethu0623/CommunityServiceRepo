namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProofOfPaymentToPayment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Payments", "ProofOfPaymentPath", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Payments", "ProofOfPaymentPath");
        }
    }
}
