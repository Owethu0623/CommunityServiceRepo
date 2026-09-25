namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMunicipalProjectManagement : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MunicipalProjects",
                c => new
                    {
                        ProjectID = c.Int(nullable: false, identity: true),
                        ProjectCode = c.String(nullable: false, maxLength: 50),
                        ProjectName = c.String(nullable: false, maxLength: 200),
                        ProjectType = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 2000),
                        ProjectLocation = c.String(maxLength: 500),
                        Latitude = c.Double(),
                        Longitude = c.Double(),
                        WardID = c.Int(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        ExpectedCompletionDate = c.DateTime(),
                        ActualCompletionDate = c.DateTime(),
                        Status = c.Int(nullable: false),
                        Priority = c.Int(nullable: false),
                        EstimatedBudget = c.Decimal(precision: 18, scale: 2),
                        ResponsibleAdministratorID = c.Int(nullable: false),
                        DateRegistered = c.DateTime(nullable: false),
                        CreatedByAdministratorID = c.Int(nullable: false),
                        LastUpdatedDate = c.DateTime(),
                        LastUpdatedByAdministratorID = c.Int(),
                    })
                .PrimaryKey(t => t.ProjectID)
                .ForeignKey("dbo.Administrators", t => t.CreatedByAdministratorID)
                .ForeignKey("dbo.Administrators", t => t.LastUpdatedByAdministratorID)
                .ForeignKey("dbo.Administrators", t => t.ResponsibleAdministratorID)
                .ForeignKey("dbo.Wards", t => t.WardID)
                .Index(t => t.ProjectCode, unique: true, name: "IX_MunicipalProject_ProjectCode")
                .Index(t => t.WardID)
                .Index(t => t.ResponsibleAdministratorID)
                .Index(t => t.CreatedByAdministratorID)
                .Index(t => t.LastUpdatedByAdministratorID);
            
            CreateIndex("dbo.AssetProjects", "ProjectID");
            AddForeignKey("dbo.AssetProjects", "ProjectID", "dbo.MunicipalProjects", "ProjectID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AssetProjects", "ProjectID", "dbo.MunicipalProjects");
            DropForeignKey("dbo.MunicipalProjects", "WardID", "dbo.Wards");
            DropForeignKey("dbo.MunicipalProjects", "ResponsibleAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.MunicipalProjects", "LastUpdatedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.MunicipalProjects", "CreatedByAdministratorID", "dbo.Administrators");
            DropIndex("dbo.MunicipalProjects", new[] { "LastUpdatedByAdministratorID" });
            DropIndex("dbo.MunicipalProjects", new[] { "CreatedByAdministratorID" });
            DropIndex("dbo.MunicipalProjects", new[] { "ResponsibleAdministratorID" });
            DropIndex("dbo.MunicipalProjects", new[] { "WardID" });
            DropIndex("dbo.MunicipalProjects", "IX_MunicipalProject_ProjectCode");
            DropIndex("dbo.AssetProjects", new[] { "ProjectID" });
            DropTable("dbo.MunicipalProjects");
        }
    }
}
