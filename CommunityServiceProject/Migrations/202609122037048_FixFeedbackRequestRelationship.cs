namespace CommunityServiceProject.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class FixFeedbackRequestRelationship : DbMigration
    {
        public override void Up()
        {
            // Remove the incorrect FK:
            // Feedbacks.FeedbackID -> Requests.RequestID
            DropForeignKey(
                "dbo.Feedbacks",
                "FeedbackID",
                "dbo.Requests");

            // Add the correct FK:
            // Feedbacks.RequestID -> Requests.RequestID
            AddForeignKey(
                "dbo.Feedbacks",
                "RequestID",
                "dbo.Requests",
                "RequestID",
                cascadeDelete: false);
        }

        public override void Down()
        {
            // Remove the correct FK.
            DropForeignKey(
                "dbo.Feedbacks",
                "RequestID",
                "dbo.Requests");

            // Restore the old FK if rolled back.
            AddForeignKey(
                "dbo.Feedbacks",
                "FeedbackID",
                "dbo.Requests",
                "RequestID",
                cascadeDelete: false);
        }
    }
}