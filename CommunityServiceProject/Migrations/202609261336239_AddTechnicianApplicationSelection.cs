namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTechnicianApplicationSelection : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TechnicianApplicationSelections",
                c => new
                    {
                        SelectionID = c.Int(nullable: false, identity: true),
                        ApplicationID = c.Int(nullable: false),
                        Comments = c.String(maxLength: 3000),
                        SelectedByAdministratorID = c.Int(nullable: false),
                        SelectionDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.SelectionID)
                .ForeignKey("dbo.TechnicianApplications", t => t.ApplicationID)
                .ForeignKey("dbo.Administrators", t => t.SelectedByAdministratorID)
                .Index(t => t.ApplicationID, unique: true, name: "IX_TechnicianApplicationSelection_Application")
                .Index(t => t.SelectedByAdministratorID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TechnicianApplicationSelections", "SelectedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.TechnicianApplicationSelections", "ApplicationID", "dbo.TechnicianApplications");
            DropIndex("dbo.TechnicianApplicationSelections", new[] { "SelectedByAdministratorID" });
            DropIndex("dbo.TechnicianApplicationSelections", "IX_TechnicianApplicationSelection_Application");
            DropTable("dbo.TechnicianApplicationSelections");
        }
    }
}
