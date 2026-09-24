namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixFeedbackIdentity : DbMigration
    {
        public override void Up()
        {
            // Remove the index that depends on FeedbackID.
            DropIndex(
                "dbo.Feedbacks",
                new[] { "FeedbackID" });

            // Remove the primary key.
            DropPrimaryKey(
                "dbo.Feedbacks");

            // Remove the old non-identity FeedbackID.
            DropColumn(
                "dbo.Feedbacks",
                "FeedbackID");

            // Recreate FeedbackID as an identity column.
            AddColumn(
                "dbo.Feedbacks",
                "FeedbackID",
                c => c.Int(nullable: false, identity: true));

            // Restore the primary key.
            AddPrimaryKey(
                "dbo.Feedbacks",
                "FeedbackID");
        }

        public override void Down()
        {
            DropPrimaryKey(
                "dbo.Feedbacks");

            DropColumn(
                "dbo.Feedbacks",
                "FeedbackID");

            AddColumn(
                "dbo.Feedbacks",
                "FeedbackID",
                c => c.Int(nullable: false));

            AddPrimaryKey(
                "dbo.Feedbacks",
                "FeedbackID");

            CreateIndex(
                "dbo.Feedbacks",
                "FeedbackID",
                name: "IX_FeedbackID");
        }
    }
}
