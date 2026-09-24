namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMunicipalAssetManagement : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AssetHistories",
                c => new
                    {
                        AssetHistoryID = c.Int(nullable: false, identity: true),
                        AssetID = c.Int(nullable: false),
                        AdministratorID = c.Int(nullable: false),
                        ActivityType = c.String(nullable: false, maxLength: 100),
                        Description = c.String(nullable: false, maxLength: 2000),
                        ActivityDate = c.DateTime(nullable: false),
                        PreviousValue = c.String(maxLength: 100),
                        NewValue = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.AssetHistoryID)
                .ForeignKey("dbo.Administrators", t => t.AdministratorID)
                .ForeignKey("dbo.MunicipalAssets", t => t.AssetID)
                .Index(t => t.AssetID)
                .Index(t => t.AdministratorID);
            
            CreateTable(
                "dbo.MunicipalAssets",
                c => new
                    {
                        AssetID = c.Int(nullable: false, identity: true),
                        AssetCode = c.String(nullable: false, maxLength: 50),
                        AssetName = c.String(nullable: false, maxLength: 150),
                        AssetType = c.String(nullable: false, maxLength: 100),
                        AssetCategory = c.String(maxLength: 100),
                        Description = c.String(maxLength: 1000),
                        Latitude = c.Double(),
                        Longitude = c.Double(),
                        LocationDescription = c.String(maxLength: 500),
                        Condition = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        DateRegistered = c.DateTime(nullable: false),
                        LastInspectionDate = c.DateTime(),
                        LastMaintenanceDate = c.DateTime(),
                        DateRetired = c.DateTime(),
                        RetirementReason = c.String(maxLength: 1000),
                        CreatedByAdministratorID = c.Int(nullable: false),
                        LastUpdatedDate = c.DateTime(),
                        LastUpdatedByAdministratorID = c.Int(),
                    })
                .PrimaryKey(t => t.AssetID)
                .ForeignKey("dbo.Administrators", t => t.CreatedByAdministratorID)
                .ForeignKey("dbo.Administrators", t => t.LastUpdatedByAdministratorID)
                .Index(t => t.AssetCode, unique: true, name: "IX_MunicipalAsset_AssetCode")
                .Index(t => t.CreatedByAdministratorID)
                .Index(t => t.LastUpdatedByAdministratorID);
            
            CreateTable(
                "dbo.AssetInspections",
                c => new
                    {
                        InspectionID = c.Int(nullable: false, identity: true),
                        AssetID = c.Int(nullable: false),
                        AdministratorID = c.Int(nullable: false),
                        InspectionDate = c.DateTime(nullable: false),
                        Condition = c.String(nullable: false, maxLength: 50),
                        Findings = c.String(nullable: false, maxLength: 2000),
                        InspectorNotes = c.String(maxLength: 2000),
                        RecommendedAction = c.String(maxLength: 100),
                        NextInspectionDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.InspectionID)
                .ForeignKey("dbo.Administrators", t => t.AdministratorID)
                .ForeignKey("dbo.MunicipalAssets", t => t.AssetID)
                .Index(t => t.AssetID)
                .Index(t => t.AdministratorID);
            
            CreateTable(
                "dbo.AssetMaintenances",
                c => new
                    {
                        AssetMaintenanceID = c.Int(nullable: false, identity: true),
                        AssetID = c.Int(nullable: false),
                        MaintenanceWorkID = c.Int(nullable: false),
                        LinkedByAdministratorID = c.Int(nullable: false),
                        LinkDate = c.DateTime(nullable: false),
                        Notes = c.String(maxLength: 1000),
                    })
                .PrimaryKey(t => t.AssetMaintenanceID)
                .ForeignKey("dbo.MunicipalAssets", t => t.AssetID)
                .ForeignKey("dbo.Administrators", t => t.LinkedByAdministratorID)
                .ForeignKey("dbo.MaintenanceWorks", t => t.MaintenanceWorkID)
                .Index(t => t.AssetID)
                .Index(t => t.MaintenanceWorkID)
                .Index(t => t.LinkedByAdministratorID);
            
            CreateTable(
                "dbo.AssetProjects",
                c => new
                    {
                        AssetProjectID = c.Int(nullable: false, identity: true),
                        AssetID = c.Int(nullable: false),
                        ProjectID = c.Int(nullable: false),
                        LinkedByAdministratorID = c.Int(nullable: false),
                        LinkDate = c.DateTime(nullable: false),
                        Notes = c.String(maxLength: 1000),
                    })
                .PrimaryKey(t => t.AssetProjectID)
                .ForeignKey("dbo.MunicipalAssets", t => t.AssetID)
                .ForeignKey("dbo.Administrators", t => t.LinkedByAdministratorID)
                .Index(t => t.AssetID)
                .Index(t => t.LinkedByAdministratorID);
            
            CreateTable(
                "dbo.AssetRequests",
                c => new
                    {
                        AssetRequestID = c.Int(nullable: false, identity: true),
                        AssetID = c.Int(nullable: false),
                        RequestID = c.Int(nullable: false),
                        LinkedByAdministratorID = c.Int(nullable: false),
                        LinkDate = c.DateTime(nullable: false),
                        Notes = c.String(maxLength: 1000),
                    })
                .PrimaryKey(t => t.AssetRequestID)
                .ForeignKey("dbo.MunicipalAssets", t => t.AssetID)
                .ForeignKey("dbo.Administrators", t => t.LinkedByAdministratorID)
                .ForeignKey("dbo.Requests", t => t.RequestID)
                .Index(t => t.AssetID)
                .Index(t => t.RequestID)
                .Index(t => t.LinkedByAdministratorID);
            
            CreateTable(
                "dbo.AssetMaintenanceNeeds",
                c => new
                    {
                        MaintenanceNeedID = c.Int(nullable: false, identity: true),
                        AssetID = c.Int(nullable: false),
                        IdentifiedByAdministratorID = c.Int(nullable: false),
                        MaintenanceType = c.String(nullable: false, maxLength: 200),
                        Description = c.String(nullable: false, maxLength: 2000),
                        Status = c.Int(nullable: false),
                        Priority = c.Int(nullable: false),
                        DateIdentified = c.DateTime(nullable: false),
                        TargetDate = c.DateTime(),
                        ResolvedDate = c.DateTime(),
                        ResolutionNotes = c.String(maxLength: 2000),
                    })
                .PrimaryKey(t => t.MaintenanceNeedID)
                .ForeignKey("dbo.MunicipalAssets", t => t.AssetID)
                .ForeignKey("dbo.Administrators", t => t.IdentifiedByAdministratorID)
                .Index(t => t.AssetID)
                .Index(t => t.IdentifiedByAdministratorID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AssetHistories", "AssetID", "dbo.MunicipalAssets");
            DropForeignKey("dbo.AssetMaintenanceNeeds", "IdentifiedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.AssetMaintenanceNeeds", "AssetID", "dbo.MunicipalAssets");
            DropForeignKey("dbo.MunicipalAssets", "LastUpdatedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.MunicipalAssets", "CreatedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.AssetRequests", "RequestID", "dbo.Requests");
            DropForeignKey("dbo.AssetRequests", "LinkedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.AssetRequests", "AssetID", "dbo.MunicipalAssets");
            DropForeignKey("dbo.AssetProjects", "LinkedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.AssetProjects", "AssetID", "dbo.MunicipalAssets");
            DropForeignKey("dbo.AssetMaintenances", "MaintenanceWorkID", "dbo.MaintenanceWorks");
            DropForeignKey("dbo.AssetMaintenances", "LinkedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.AssetMaintenances", "AssetID", "dbo.MunicipalAssets");
            DropForeignKey("dbo.AssetInspections", "AssetID", "dbo.MunicipalAssets");
            DropForeignKey("dbo.AssetInspections", "AdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.AssetHistories", "AdministratorID", "dbo.Administrators");
            DropIndex("dbo.AssetMaintenanceNeeds", new[] { "IdentifiedByAdministratorID" });
            DropIndex("dbo.AssetMaintenanceNeeds", new[] { "AssetID" });
            DropIndex("dbo.AssetRequests", new[] { "LinkedByAdministratorID" });
            DropIndex("dbo.AssetRequests", new[] { "RequestID" });
            DropIndex("dbo.AssetRequests", new[] { "AssetID" });
            DropIndex("dbo.AssetProjects", new[] { "LinkedByAdministratorID" });
            DropIndex("dbo.AssetProjects", new[] { "AssetID" });
            DropIndex("dbo.AssetMaintenances", new[] { "LinkedByAdministratorID" });
            DropIndex("dbo.AssetMaintenances", new[] { "MaintenanceWorkID" });
            DropIndex("dbo.AssetMaintenances", new[] { "AssetID" });
            DropIndex("dbo.AssetInspections", new[] { "AdministratorID" });
            DropIndex("dbo.AssetInspections", new[] { "AssetID" });
            DropIndex("dbo.MunicipalAssets", new[] { "LastUpdatedByAdministratorID" });
            DropIndex("dbo.MunicipalAssets", new[] { "CreatedByAdministratorID" });
            DropIndex("dbo.MunicipalAssets", "IX_MunicipalAsset_AssetCode");
            DropIndex("dbo.AssetHistories", new[] { "AdministratorID" });
            DropIndex("dbo.AssetHistories", new[] { "AssetID" });
            DropTable("dbo.AssetMaintenanceNeeds");
            DropTable("dbo.AssetRequests");
            DropTable("dbo.AssetProjects");
            DropTable("dbo.AssetMaintenances");
            DropTable("dbo.AssetInspections");
            DropTable("dbo.MunicipalAssets");
            DropTable("dbo.AssetHistories");
        }
    }
}
