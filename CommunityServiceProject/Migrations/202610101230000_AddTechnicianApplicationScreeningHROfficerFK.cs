namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddTechnicianApplicationScreeningHROfficerFK : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.TechnicianApplicationScreenings", "ScreenedByHROfficerID");
            AddForeignKey("dbo.TechnicianApplicationScreenings", "ScreenedByHROfficerID", "dbo.HROfficers", "HROfficerID");
        }

        public override void Down()
        {
            DropForeignKey("dbo.TechnicianApplicationScreenings", "ScreenedByHROfficerID", "dbo.HROfficers");
            DropIndex("dbo.TechnicianApplicationScreenings", new[] { "ScreenedByHROfficerID" });
        }
    }
}
