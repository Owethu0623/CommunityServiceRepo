namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTechnicianSprint2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Requests", "CategoryID", "dbo.Categories");
            DropForeignKey("dbo.Requests", "CitizenID", "dbo.Citizens");
            DropForeignKey("dbo.Requests", "WardID", "dbo.Wards");
            CreateTable(
                "dbo.MaintenanceCompletions",
                c => new
                    {
                        MaintenanceCompletionID = c.Int(nullable: false, identity: true),
                        MaintenanceWorkID = c.Int(nullable: false),
                        MaintenanceSummary = c.String(nullable: false, maxLength: 2000),
                        ResolutionAction = c.String(nullable: false, maxLength: 2000),
                        SubmittedDate = c.DateTime(nullable: false),
                        VerificationStatus = c.Int(nullable: false),
                        VerifiedByAdministratorID = c.Int(),
                        VerifiedDate = c.DateTime(),
                        AdministratorComments = c.String(maxLength: 1000),
                    })
                .PrimaryKey(t => t.MaintenanceCompletionID)
                .ForeignKey("dbo.MaintenanceWorks", t => t.MaintenanceWorkID)
                .ForeignKey("dbo.Administrators", t => t.VerifiedByAdministratorID)
                .Index(t => t.MaintenanceWorkID)
                .Index(t => t.VerifiedByAdministratorID);
            
            CreateTable(
                "dbo.MaintenanceKnowledgeBases",
                c => new
                    {
                        KnowledgeBaseID = c.Int(nullable: false, identity: true),
                        MaintenanceCompletionID = c.Int(nullable: false),
                        CategoryID = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 200),
                        Keywords = c.String(maxLength: 500),
                        CreatedDate = c.DateTime(nullable: false),
                        IsApproved = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.KnowledgeBaseID)
                .ForeignKey("dbo.Categories", t => t.CategoryID)
                .ForeignKey("dbo.MaintenanceCompletions", t => t.MaintenanceCompletionID)
                .Index(t => t.MaintenanceCompletionID)
                .Index(t => t.CategoryID);
            
            CreateTable(
                "dbo.MaintenanceWorks",
                c => new
                    {
                        MaintenanceWorkID = c.Int(nullable: false, identity: true),
                        RequestID = c.Int(nullable: false),
                        TechnicianID = c.Int(nullable: false),
                        StartedDate = c.DateTime(),
                        CompletedDate = c.DateTime(),
                        Status = c.Int(nullable: false),
                        ProgressPercentage = c.Int(nullable: false),
                        CurrentActivity = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.MaintenanceWorkID)
                .ForeignKey("dbo.Requests", t => t.RequestID)
                .ForeignKey("dbo.Technicians", t => t.TechnicianID)
                .Index(t => t.RequestID)
                .Index(t => t.TechnicianID);
            
            CreateTable(
                "dbo.MaintenanceEvidences",
                c => new
                    {
                        EvidenceID = c.Int(nullable: false, identity: true),
                        MaintenanceWorkID = c.Int(nullable: false),
                        EvidenceType = c.Int(nullable: false),
                        FilePath = c.String(nullable: false, maxLength: 500),
                        Description = c.String(maxLength: 500),
                        UploadedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.EvidenceID)
                .ForeignKey("dbo.MaintenanceWorks", t => t.MaintenanceWorkID)
                .Index(t => t.MaintenanceWorkID);
            
            CreateTable(
                "dbo.MaintenanceMaterials",
                c => new
                    {
                        MaintenanceMaterialID = c.Int(nullable: false, identity: true),
                        MaintenanceWorkID = c.Int(nullable: false),
                        MaterialName = c.String(nullable: false, maxLength: 100),
                        Quantity = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Unit = c.String(nullable: false, maxLength: 30),
                        RecordedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.MaintenanceMaterialID)
                .ForeignKey("dbo.MaintenanceWorks", t => t.MaintenanceWorkID)
                .Index(t => t.MaintenanceWorkID);
            
            CreateTable(
                "dbo.MaintenanceProgresses",
                c => new
                    {
                        ProgressID = c.Int(nullable: false, identity: true),
                        MaintenanceWorkID = c.Int(nullable: false),
                        ProgressPercentage = c.Int(nullable: false),
                        CurrentActivity = c.String(nullable: false, maxLength: 500),
                        RecordedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ProgressID)
                .ForeignKey("dbo.MaintenanceWorks", t => t.MaintenanceWorkID)
                .Index(t => t.MaintenanceWorkID);
            
            CreateTable(
                "dbo.TechnicianAssignments",
                c => new
                    {
                        AssignmentID = c.Int(nullable: false, identity: true),
                        RequestID = c.Int(nullable: false),
                        TechnicianID = c.Int(nullable: false),
                        AdministratorID = c.Int(nullable: false),
                        AssignedDate = c.DateTime(nullable: false),
                        AcknowledgedDate = c.DateTime(),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AssignmentID)
                .ForeignKey("dbo.Administrators", t => t.AdministratorID)
                .ForeignKey("dbo.Requests", t => t.RequestID)
                .ForeignKey("dbo.Technicians", t => t.TechnicianID)
                .Index(t => t.RequestID)
                .Index(t => t.TechnicianID)
                .Index(t => t.AdministratorID);
            
            CreateTable(
                "dbo.AssignmentIssues",
                c => new
                    {
                        AssignmentIssueID = c.Int(nullable: false, identity: true),
                        AssignmentID = c.Int(nullable: false),
                        IssueType = c.Int(nullable: false),
                        Reason = c.String(nullable: false, maxLength: 1000),
                        ReportedDate = c.DateTime(nullable: false),
                        Status = c.Int(nullable: false),
                        AdministratorResponse = c.String(maxLength: 1000),
                        ResolvedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.AssignmentIssueID)
                .ForeignKey("dbo.TechnicianAssignments", t => t.AssignmentID)
                .Index(t => t.AssignmentID);
            
            CreateTable(
                "dbo.ReassignmentRequests",
                c => new
                    {
                        ReassignmentRequestID = c.Int(nullable: false, identity: true),
                        AssignmentID = c.Int(nullable: false),
                        TechnicianID = c.Int(nullable: false),
                        Reason = c.String(nullable: false, maxLength: 1000),
                        RequestedDate = c.DateTime(nullable: false),
                        Status = c.Int(nullable: false),
                        ReviewedByAdministratorID = c.Int(),
                        ReviewedDate = c.DateTime(),
                        AdministratorResponse = c.String(maxLength: 1000),
                    })
                .PrimaryKey(t => t.ReassignmentRequestID)
                .ForeignKey("dbo.TechnicianAssignments", t => t.AssignmentID)
                .ForeignKey("dbo.Administrators", t => t.ReviewedByAdministratorID)
                .ForeignKey("dbo.Technicians", t => t.TechnicianID)
                .Index(t => t.AssignmentID)
                .Index(t => t.TechnicianID)
                .Index(t => t.ReviewedByAdministratorID);
            
            CreateTable(
                "dbo.TechnicianSkills",
                c => new
                    {
                        TechnicianSkillID = c.Int(nullable: false, identity: true),
                        TechnicianID = c.Int(nullable: false),
                        SkillID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.TechnicianSkillID)
                .ForeignKey("dbo.Skills", t => t.SkillID)
                .ForeignKey("dbo.Technicians", t => t.TechnicianID)
                .Index(t => new { t.TechnicianID, t.SkillID }, unique: true, name: "IX_TechnicianSkill_Technician_Skill");
            
            CreateTable(
                "dbo.Skills",
                c => new
                    {
                        SkillID = c.Int(nullable: false, identity: true),
                        SkillName = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 250),
                    })
                .PrimaryKey(t => t.SkillID);
            
            CreateTable(
                "dbo.WorkNotes",
                c => new
                    {
                        WorkNoteID = c.Int(nullable: false, identity: true),
                        MaintenanceWorkID = c.Int(nullable: false),
                        NoteText = c.String(nullable: false, maxLength: 1000),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.WorkNoteID)
                .ForeignKey("dbo.MaintenanceWorks", t => t.MaintenanceWorkID)
                .Index(t => t.MaintenanceWorkID);
            
            CreateTable(
                "dbo.RequestSkills",
                c => new
                    {
                        RequestSkillID = c.Int(nullable: false, identity: true),
                        RequestID = c.Int(nullable: false),
                        SkillID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.RequestSkillID)
                .ForeignKey("dbo.Requests", t => t.RequestID)
                .ForeignKey("dbo.Skills", t => t.SkillID)
                .Index(t => new { t.RequestID, t.SkillID }, unique: true, name: "IX_RequestSkill_Request_Skill");
            
            AddColumn("dbo.Requests", "Priority", c => c.Int(nullable: false));
            AlterColumn("dbo.Requests", "Latitude", c => c.Double(nullable: false));
            AlterColumn("dbo.Requests", "Longitude", c => c.Double(nullable: false));
            CreateIndex("dbo.Technicians", "EmailAddress", unique: true, name: "IX_Technician_EmailAddress");
            AddForeignKey("dbo.Requests", "CategoryID", "dbo.Categories", "CategoryID");
            AddForeignKey("dbo.Requests", "CitizenID", "dbo.Citizens", "CitizenID");
            AddForeignKey("dbo.Requests", "WardID", "dbo.Wards", "WardID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Requests", "WardID", "dbo.Wards");
            DropForeignKey("dbo.Requests", "CitizenID", "dbo.Citizens");
            DropForeignKey("dbo.Requests", "CategoryID", "dbo.Categories");
            DropForeignKey("dbo.MaintenanceCompletions", "VerifiedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.MaintenanceCompletions", "MaintenanceWorkID", "dbo.MaintenanceWorks");
            DropForeignKey("dbo.MaintenanceKnowledgeBases", "MaintenanceCompletionID", "dbo.MaintenanceCompletions");
            DropForeignKey("dbo.MaintenanceKnowledgeBases", "CategoryID", "dbo.Categories");
            DropForeignKey("dbo.RequestSkills", "SkillID", "dbo.Skills");
            DropForeignKey("dbo.RequestSkills", "RequestID", "dbo.Requests");
            DropForeignKey("dbo.WorkNotes", "MaintenanceWorkID", "dbo.MaintenanceWorks");
            DropForeignKey("dbo.MaintenanceWorks", "TechnicianID", "dbo.Technicians");
            DropForeignKey("dbo.TechnicianSkills", "TechnicianID", "dbo.Technicians");
            DropForeignKey("dbo.TechnicianSkills", "SkillID", "dbo.Skills");
            DropForeignKey("dbo.TechnicianAssignments", "TechnicianID", "dbo.Technicians");
            DropForeignKey("dbo.TechnicianAssignments", "RequestID", "dbo.Requests");
            DropForeignKey("dbo.ReassignmentRequests", "TechnicianID", "dbo.Technicians");
            DropForeignKey("dbo.ReassignmentRequests", "ReviewedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.ReassignmentRequests", "AssignmentID", "dbo.TechnicianAssignments");
            DropForeignKey("dbo.AssignmentIssues", "AssignmentID", "dbo.TechnicianAssignments");
            DropForeignKey("dbo.TechnicianAssignments", "AdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.MaintenanceWorks", "RequestID", "dbo.Requests");
            DropForeignKey("dbo.MaintenanceProgresses", "MaintenanceWorkID", "dbo.MaintenanceWorks");
            DropForeignKey("dbo.MaintenanceMaterials", "MaintenanceWorkID", "dbo.MaintenanceWorks");
            DropForeignKey("dbo.MaintenanceEvidences", "MaintenanceWorkID", "dbo.MaintenanceWorks");
            DropIndex("dbo.RequestSkills", "IX_RequestSkill_Request_Skill");
            DropIndex("dbo.WorkNotes", new[] { "MaintenanceWorkID" });
            DropIndex("dbo.TechnicianSkills", "IX_TechnicianSkill_Technician_Skill");
            DropIndex("dbo.ReassignmentRequests", new[] { "ReviewedByAdministratorID" });
            DropIndex("dbo.ReassignmentRequests", new[] { "TechnicianID" });
            DropIndex("dbo.ReassignmentRequests", new[] { "AssignmentID" });
            DropIndex("dbo.AssignmentIssues", new[] { "AssignmentID" });
            DropIndex("dbo.TechnicianAssignments", new[] { "AdministratorID" });
            DropIndex("dbo.TechnicianAssignments", new[] { "TechnicianID" });
            DropIndex("dbo.TechnicianAssignments", new[] { "RequestID" });
            DropIndex("dbo.Technicians", "IX_Technician_EmailAddress");
            DropIndex("dbo.MaintenanceProgresses", new[] { "MaintenanceWorkID" });
            DropIndex("dbo.MaintenanceMaterials", new[] { "MaintenanceWorkID" });
            DropIndex("dbo.MaintenanceEvidences", new[] { "MaintenanceWorkID" });
            DropIndex("dbo.MaintenanceWorks", new[] { "TechnicianID" });
            DropIndex("dbo.MaintenanceWorks", new[] { "RequestID" });
            DropIndex("dbo.MaintenanceKnowledgeBases", new[] { "CategoryID" });
            DropIndex("dbo.MaintenanceKnowledgeBases", new[] { "MaintenanceCompletionID" });
            DropIndex("dbo.MaintenanceCompletions", new[] { "VerifiedByAdministratorID" });
            DropIndex("dbo.MaintenanceCompletions", new[] { "MaintenanceWorkID" });
            AlterColumn("dbo.Requests", "Longitude", c => c.Double());
            AlterColumn("dbo.Requests", "Latitude", c => c.Double());
            DropColumn("dbo.Requests", "Priority");
            DropTable("dbo.RequestSkills");
            DropTable("dbo.WorkNotes");
            DropTable("dbo.Skills");
            DropTable("dbo.TechnicianSkills");
            DropTable("dbo.ReassignmentRequests");
            DropTable("dbo.AssignmentIssues");
            DropTable("dbo.TechnicianAssignments");
            DropTable("dbo.MaintenanceProgresses");
            DropTable("dbo.MaintenanceMaterials");
            DropTable("dbo.MaintenanceEvidences");
            DropTable("dbo.MaintenanceWorks");
            DropTable("dbo.MaintenanceKnowledgeBases");
            DropTable("dbo.MaintenanceCompletions");
            AddForeignKey("dbo.Requests", "WardID", "dbo.Wards", "WardID", cascadeDelete: true);
            AddForeignKey("dbo.Requests", "CitizenID", "dbo.Citizens", "CitizenID", cascadeDelete: true);
            AddForeignKey("dbo.Requests", "CategoryID", "dbo.Categories", "CategoryID", cascadeDelete: true);
        }
    }
}
