namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddTechnicianApplicationSelectionHROfficerFK : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.TechnicianApplicationSelections", "SelectedByHROfficerID");
            AddForeignKey("dbo.TechnicianApplicationSelections", "SelectedByHROfficerID", "dbo.HROfficers", "HROfficerID");
        }

        public override void Down()
        {
            DropForeignKey("dbo.TechnicianApplicationSelections", "SelectedByHROfficerID", "dbo.HROfficers");
            DropIndex("dbo.TechnicianApplicationSelections", new[] { "SelectedByHROfficerID" });
        }
    }
}
