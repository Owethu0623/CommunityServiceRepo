using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using static System.Data.Entity.Migrations.Model.UpdateDatabaseOperation;
using static System.Web.Razor.Parser.SyntaxConstants;

namespace CommunityServiceProject.Models
{
    public class Community : DbContext
    {
        public Community() : base("Community")
        {
        }

        // Existing entities
        public DbSet<Citizen> Citizens { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<Technician> Technicians { get; set; }
        public DbSet<FinanceOfficer> FinanceOfficers { get; set; }
        public DbSet<HROfficer> HROfficers { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Ward> Wards { get; set; }
        public DbSet<Appeal> Appeals { get; set; }
        // Technician skills
        public DbSet<Skill> Skills { get; set; }
        public DbSet<TechnicianSkill> TechnicianSkills { get; set; }
        public DbSet<RequestSkill> RequestSkills { get; set; }

        // Technician assignments
        public DbSet<TechnicianAssignment> TechnicianAssignments { get; set; }
        public DbSet<AssignmentIssue> AssignmentIssues { get; set; }
        public DbSet<ReassignmentRequest> ReassignmentRequests { get; set; }

        // Maintenance
        public DbSet<MaintenanceWork> MaintenanceWorks { get; set; }
        public DbSet<MaintenanceProgress> MaintenanceProgress { get; set; }
        public DbSet<WorkNote> WorkNotes { get; set; }
        public DbSet<MaintenanceMaterial> MaintenanceMaterials { get; set; }
        public DbSet<MaintenanceEvidence> MaintenanceEvidence { get; set; }
        public DbSet<MaintenanceCompletion> MaintenanceCompletions { get; set; }

        // Maintenance knowledge base
        public DbSet<MaintenanceKnowledgeBase> MaintenanceKnowledgeBases { get; set; }

         // Compliance
        public DbSet<ComplianceRecord> ComplianceRecords { get; set; }
        public DbSet<Violation> Violations { get; set; }
        public DbSet<Warning> Warnings { get; set; }
        public DbSet<AccountRestriction> AccountRestrictions { get; set; }

        // Feedback
        public DbSet<Feedback> Feedbacks { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        // =========================================================
        // MUNICIPAL ASSET MANAGEMENT
        // =========================================================

        public DbSet<MunicipalAsset> MunicipalAssets { get; set; }

        public DbSet<AssetInspection> AssetInspections { get; set; }

        public DbSet<AssetMaintenanceNeed> AssetMaintenanceNeeds { get; set; }

        public DbSet<AssetRequest> AssetRequests { get; set; }

        public DbSet<AssetMaintenance> AssetMaintenances { get; set; }

        public DbSet<AssetProject> AssetProjects { get; set; }

        public DbSet<AssetHistory> AssetHistories { get; set; }
        public DbSet<MunicipalProject> MunicipalProjects { get; set; }

        public DbSet<ProjectObjective> ProjectObjectives { get; set; }

        public DbSet<ProjectMilestone> ProjectMilestones { get; set; }

        public DbSet<ProjectProgress> ProjectProgressRecords { get; set; }

        public DbSet<ProjectEvidence> ProjectEvidenceRecords { get; set; }

        public DbSet<ProjectHistory> ProjectHistoryRecords { get; set; }

        public DbSet<ProjectRequest> ProjectRequests { get; set; }

        public DbSet<TechnicianOpportunity> TechnicianOpportunities { get; set; }
        public DbSet<TechnicianApplication> TechnicianApplications
        {
            get;
            set;
        }

        public DbSet<ApplicationDocument> ApplicationDocuments { get; set; }
        public DbSet<TechnicianApplicationScreening> TechnicianApplicationScreenings { get; set; }
        public DbSet<ApplicationAssessment> ApplicationAssessments { get; set; }
        public DbSet<ApplicationInterview> ApplicationInterviews { get; set; }
        public DbSet<TechnicianApplicationSelection> TechnicianApplicationSelections { get; set; }
        public DbSet<TechnicianApplicationFinalVerification> TechnicianApplicationFinalVerifications { get; set; }
        public DbSet<TechnicianOnboarding> TechnicianOnboardings { get; set; }
        public DbSet<TechnicianApplicationNotification> TechnicianApplicationNotifications { get; set; }
        public DbSet<AdministratorNotification> AdministratorNotifications { get; set; }
        public DbSet<HROfficerNotification> HROfficerNotifications { get; set; }


        public DbSet<ServiceType> ServiceTypes { get; set; }

        public DbSet<FeeSchedule> FeeSchedules { get; set; }

        public DbSet<MunicipalServiceRequest> MunicipalServiceRequests { get; set; }

        public DbSet<Invoice> Invoices { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<Receipt> Receipts { get; set; }

        public DbSet<Refund> Refunds { get; set; }

        public DbSet<TechnicianPayrollProfile> TechnicianPayrollProfiles { get; set; }

        public DbSet<OvertimeClaim> OvertimeClaims { get; set; }

        public DbSet<PayrollPeriod> PayrollPeriods { get; set; }

        public DbSet<Payroll> Payrolls { get; set; }

        public DbSet<PayrollAllowance> PayrollAllowances { get; set; }

        public DbSet<PayrollDeduction> PayrollDeductions { get; set; }

        public DbSet<PayrollPayment> PayrollPayments { get; set; }

        public DbSet<Payslip> Payslips { get; set; }

        public DbSet<FinancialAudit> FinancialAudits { get; set; }
      
      public DbSet<MunicipalServiceRequestNotification> MunicipalServiceRequestNotifications { get; set; }
        public DbSet<FinancialAuditRecord> FinancialAuditRecords { get; set; }
        public DbSet<FinanceNotification> FinanceNotifications { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================================================
            // REQUEST RELATIONSHIPS
            // =========================================================

            // Request -> Citizen
            modelBuilder.Entity<Request>()
                .HasRequired(r => r.Citizen)
                .WithMany()
                .HasForeignKey(r => r.CitizenID)
                .WillCascadeOnDelete(false);

            // Request -> Category
            modelBuilder.Entity<Request>()
                .HasRequired(r => r.Category)
                .WithMany(c => c.Requests)
                .HasForeignKey(r => r.CategoryID)
                .WillCascadeOnDelete(false);

            // Request -> Ward
            modelBuilder.Entity<Request>()
                .HasRequired(r => r.Ward)
                .WithMany(w => w.Requests)
                .HasForeignKey(r => r.WardID)
                .WillCascadeOnDelete(false);

            // Request -> Administrator
            modelBuilder.Entity<Request>()
                .HasOptional(r => r.Administrator)
                .WithMany(a => a.Requests)
                .WillCascadeOnDelete(false);

            // Request -> current Technician
            modelBuilder.Entity<Request>()
                .HasOptional(r => r.Technician)
                .WithMany(t => t.Requests)
                .HasForeignKey(r => r.TechnicianID)
                .WillCascadeOnDelete(false);


            // =========================================================
            // SKILL RELATIONSHIPS
            // =========================================================

            // TechnicianSkill -> Technician
            modelBuilder.Entity<TechnicianSkill>()
                .HasRequired(ts => ts.Technician)
                .WithMany(t => t.TechnicianSkills)
                .HasForeignKey(ts => ts.TechnicianID)
                .WillCascadeOnDelete(false);

            // TechnicianSkill -> Skill
            modelBuilder.Entity<TechnicianSkill>()
                .HasRequired(ts => ts.Skill)
                .WithMany(s => s.TechnicianSkills)
                .HasForeignKey(ts => ts.SkillID)
                .WillCascadeOnDelete(false);

            // RequestSkill -> Request
            modelBuilder.Entity<RequestSkill>()
                .HasRequired(rs => rs.Request)
                .WithMany(r => r.RequiredSkills)
                .HasForeignKey(rs => rs.RequestID)
                .WillCascadeOnDelete(false);

            // RequestSkill -> Skill
            modelBuilder.Entity<RequestSkill>()
                .HasRequired(rs => rs.Skill)
                .WithMany()
                .HasForeignKey(rs => rs.SkillID)
                .WillCascadeOnDelete(false);


            // =========================================================
            // TECHNICIAN ASSIGNMENTS
            // =========================================================

            // TechnicianAssignment -> Request
            modelBuilder.Entity<TechnicianAssignment>()
                .HasRequired(a => a.Request)
                .WithMany(r => r.TechnicianAssignments)
                .HasForeignKey(a => a.RequestID)
                .WillCascadeOnDelete(false);

            // TechnicianAssignment -> Technician
            modelBuilder.Entity<TechnicianAssignment>()
                .HasRequired(a => a.Technician)
                .WithMany(t => t.TechnicianAssignments)
                .HasForeignKey(a => a.TechnicianID)
                .WillCascadeOnDelete(false);

            // TechnicianAssignment -> Administrator
            modelBuilder.Entity<TechnicianAssignment>()
                .HasRequired(a => a.Administrator)
                .WithMany(ad => ad.TechnicianAssignments)
                .HasForeignKey(a => a.AdministratorID)
                .WillCascadeOnDelete(false);


            // =========================================================
            // ASSIGNMENT ISSUES
            // =========================================================

            // AssignmentIssue -> TechnicianAssignment
            modelBuilder.Entity<AssignmentIssue>()
                .HasRequired(i => i.Assignment)
                .WithMany(a => a.AssignmentIssues)
                .HasForeignKey(i => i.AssignmentID)
                .WillCascadeOnDelete(false);


            // =========================================================
            // REASSIGNMENT REQUESTS
            // =========================================================

            // ReassignmentRequest -> Assignment
            modelBuilder.Entity<ReassignmentRequest>()
                .HasRequired(r => r.Assignment)
                .WithMany(a => a.ReassignmentRequests)
                .HasForeignKey(r => r.AssignmentID)
                .WillCascadeOnDelete(false);

            // ReassignmentRequest -> Technician
            modelBuilder.Entity<ReassignmentRequest>()
                .HasRequired(r => r.Technician)
                .WithMany()
                .HasForeignKey(r => r.TechnicianID)
                .WillCascadeOnDelete(false);

            // ReassignmentRequest -> reviewing Administrator
            modelBuilder.Entity<ReassignmentRequest>()
                .HasOptional(r => r.ReviewedByAdministrator)
                .WithMany(a => a.ReassignmentRequests)
                .HasForeignKey(r => r.ReviewedByAdministratorID)
                .WillCascadeOnDelete(false);


            // =========================================================
            // MAINTENANCE
            // =========================================================

            // MaintenanceWork -> Request
            modelBuilder.Entity<MaintenanceWork>()
                .HasRequired(m => m.Request)
                .WithMany(r => r.MaintenanceWorks)
                .HasForeignKey(m => m.RequestID)
                .WillCascadeOnDelete(false);

            // MaintenanceWork -> Technician
            modelBuilder.Entity<MaintenanceWork>()
                .HasRequired(m => m.Technician)
                .WithMany()
                .HasForeignKey(m => m.TechnicianID)
                .WillCascadeOnDelete(false);

            // MaintenanceProgress -> MaintenanceWork
            modelBuilder.Entity<MaintenanceProgress>()
                .HasRequired(p => p.MaintenanceWork)
                .WithMany(m => m.ProgressRecords)
                .HasForeignKey(p => p.MaintenanceWorkID)
                .WillCascadeOnDelete(false);

            // WorkNote -> MaintenanceWork
            modelBuilder.Entity<WorkNote>()
                .HasRequired(n => n.MaintenanceWork)
                .WithMany(m => m.WorkNotes)
                .HasForeignKey(n => n.MaintenanceWorkID)
                .WillCascadeOnDelete(false);

            // MaintenanceMaterial -> MaintenanceWork
            modelBuilder.Entity<MaintenanceMaterial>()
                .HasRequired(m => m.MaintenanceWork)
                .WithMany(w => w.Materials)
                .HasForeignKey(m => m.MaintenanceWorkID)
                .WillCascadeOnDelete(false);

            // MaintenanceEvidence -> MaintenanceWork
            modelBuilder.Entity<MaintenanceEvidence>()
                .HasRequired(e => e.MaintenanceWork)
                .WithMany(m => m.Evidence)
                .HasForeignKey(e => e.MaintenanceWorkID)
                .WillCascadeOnDelete(false);


            // =========================================================
            // MAINTENANCE COMPLETION
            // =========================================================

            // MaintenanceCompletion -> MaintenanceWork
            modelBuilder.Entity<MaintenanceCompletion>()
                .HasRequired(c => c.MaintenanceWork)
                .WithMany(m => m.Completions)
                .HasForeignKey(c => c.MaintenanceWorkID)
                .WillCascadeOnDelete(false);

            // MaintenanceCompletion -> verifying Administrator
            modelBuilder.Entity<MaintenanceCompletion>()
                .HasOptional(c => c.VerifiedByAdministrator)
                .WithMany(a => a.MaintenanceCompletions)
                .HasForeignKey(c => c.VerifiedByAdministratorID)
                .WillCascadeOnDelete(false);


            // =========================================================
            // KNOWLEDGE BASE
            // =========================================================

            // Knowledge base -> MaintenanceCompletion
            modelBuilder.Entity<MaintenanceKnowledgeBase>()
                .HasRequired(k => k.MaintenanceCompletion)
                .WithMany(c => c.KnowledgeBaseEntries)
                .HasForeignKey(k => k.MaintenanceCompletionID)
                .WillCascadeOnDelete(false);

            // Knowledge base -> Category
            modelBuilder.Entity<MaintenanceKnowledgeBase>()
                .HasRequired(k => k.Category)
                .WithMany()
                .HasForeignKey(k => k.CategoryID)
                .WillCascadeOnDelete(false);

            // Knowledge base -> creating Technician
            modelBuilder.Entity<MaintenanceKnowledgeBase>()
                .HasRequired(k => k.CreatedByTechnician)
                .WithMany()
                .HasForeignKey(k => k.CreatedByTechnicianID)
                .WillCascadeOnDelete(false);


            // =========================================================
            // COMPLIANCE
            // =========================================================

            
            // ComplianceRecord -> Citizen
            modelBuilder.Entity<ComplianceRecord>()
                .HasRequired(c => c.Citizen)
                .WithMany()
                .HasForeignKey(c => c.CitizenID)
                .WillCascadeOnDelete(false);

            // Violation -> ComplianceRecord
            modelBuilder.Entity<Violation>()
                .HasRequired(v => v.ComplianceRecord)
                .WithMany(c => c.Violations)
                .HasForeignKey(v => v.ComplianceID)
                .WillCascadeOnDelete(false);

            // Violation -> Request
            modelBuilder.Entity<Violation>()
                .HasOptional(v => v.Request)
                .WithMany()
                .HasForeignKey(v => v.RequestID)
                .WillCascadeOnDelete(false);

            // Violation -> Administrator
            modelBuilder.Entity<Violation>()
                .HasRequired(v => v.Administrator)
                .WithMany()
                .HasForeignKey(v => v.AdministratorID)
                .WillCascadeOnDelete(false);

            // Warning -> Violation
            modelBuilder.Entity<Warning>()
                .HasRequired(w => w.Violation)
                .WithMany(v => v.Warnings)
                .HasForeignKey(w => w.ViolationID)
                .WillCascadeOnDelete(false);

            // Warning -> Administrator
            modelBuilder.Entity<Warning>()
                .HasRequired(w => w.Administrator)
                .WithMany()
                .HasForeignKey(w => w.AdministratorID)
                .WillCascadeOnDelete(false);

            // AccountRestriction -> Citizen
            modelBuilder.Entity<AccountRestriction>()
                .HasRequired(r => r.Citizen)
                .WithMany(c => c.AccountRestrictions)
                .HasForeignKey(r => r.CitizenID)
                .WillCascadeOnDelete(false);

            // AccountRestriction -> Administrator
            modelBuilder.Entity<AccountRestriction>()
                .HasRequired(r => r.Administrator)
                .WithMany()
                .HasForeignKey(r => r.AdministratorID)
                .WillCascadeOnDelete(false);

            // =========================================================
            // FEEDBACK
            // =========================================================

            // Feedback -> Request
            modelBuilder.Entity<Feedback>()
                .HasRequired(f => f.Request)
                .WithMany(r => r.Feedbacks)
                .HasForeignKey(f => f.RequestID)
                .WillCascadeOnDelete(false);

            // Feedback -> Citizen
            modelBuilder.Entity<Feedback>()
                .HasRequired(f => f.Citizen)
                .WithMany()
                .HasForeignKey(f => f.CitizenID)
                .WillCascadeOnDelete(false);

            // =========================================================
            // MUNICIPAL ASSET MANAGEMENT
            // =========================================================

            // MunicipalAsset -> Created By Administrator
            modelBuilder.Entity<MunicipalAsset>()
                .HasRequired(a => a.CreatedByAdministrator)
                .WithMany()
                .HasForeignKey(a => a.CreatedByAdministratorID)
                .WillCascadeOnDelete(false);

            // MunicipalAsset -> Last Updated By Administrator
            modelBuilder.Entity<MunicipalAsset>()
                .HasOptional(a => a.LastUpdatedByAdministrator)
                .WithMany()
                .HasForeignKey(a => a.LastUpdatedByAdministratorID)
                .WillCascadeOnDelete(false);

            // TechnicianOpportunity -> CreatedByAdministrator (optional)
            modelBuilder.Entity<TechnicianOpportunity>()
                .HasOptional(o => o.CreatedByAdministrator)
                .WithMany()
                .HasForeignKey(o => o.CreatedByAdministratorID)
                .WillCascadeOnDelete(false);

            // TechnicianOpportunity -> CreatedByHROfficer (optional)
            modelBuilder.Entity<TechnicianOpportunity>()
                .HasOptional(o => o.CreatedByHROfficer)
                .WithMany()
                .HasForeignKey(o => o.CreatedByHROfficerID)
                .WillCascadeOnDelete(false);

            // TechnicianOpportunity -> LastUpdatedByHROfficer (optional)
            modelBuilder.Entity<TechnicianOpportunity>()
                .HasOptional(o => o.LastUpdatedByHROfficer)
                .WithMany()
                .HasForeignKey(o => o.LastUpdatedByHROfficerID)
                .WillCascadeOnDelete(false);

            // AssetInspection -> MunicipalAsset
            modelBuilder.Entity<AssetInspection>()
                .HasRequired(i => i.Asset)
                .WithMany(a => a.AssetInspections)
                .HasForeignKey(i => i.AssetID)
                .WillCascadeOnDelete(false);

            // AssetInspection -> Administrator
            modelBuilder.Entity<AssetInspection>()
                .HasRequired(i => i.Administrator)
                .WithMany()
                .HasForeignKey(i => i.AdministratorID)
                .WillCascadeOnDelete(false);

            // AssetMaintenanceNeed -> MunicipalAsset
            modelBuilder.Entity<AssetMaintenanceNeed>()
                .HasRequired(n => n.Asset)
                .WithMany(a => a.MaintenanceNeeds)
                .HasForeignKey(n => n.AssetID)
                .WillCascadeOnDelete(false);

            // AssetMaintenanceNeed -> Administrator
            modelBuilder.Entity<AssetMaintenanceNeed>()
                .HasRequired(n => n.IdentifiedByAdministrator)
                .WithMany()
                .HasForeignKey(n => n.IdentifiedByAdministratorID)
                .WillCascadeOnDelete(false);

            // AssetRequest -> MunicipalAsset
            modelBuilder.Entity<AssetRequest>()
                .HasRequired(ar => ar.Asset)
                .WithMany(a => a.AssetRequests)
                .HasForeignKey(ar => ar.AssetID)
                .WillCascadeOnDelete(false);

            // AssetRequest -> Request
            modelBuilder.Entity<AssetRequest>()
                .HasRequired(ar => ar.Request)
                .WithMany()
                .HasForeignKey(ar => ar.RequestID)
                .WillCascadeOnDelete(false);

            // AssetRequest -> Administrator
            modelBuilder.Entity<AssetRequest>()
                .HasRequired(ar => ar.LinkedByAdministrator)
                .WithMany()
                .HasForeignKey(ar => ar.LinkedByAdministratorID)
                .WillCascadeOnDelete(false);

            // AssetMaintenance -> MunicipalAsset
            modelBuilder.Entity<AssetMaintenance>()
                .HasRequired(am => am.Asset)
                .WithMany(a => a.AssetMaintenanceRecords)
                .HasForeignKey(am => am.AssetID)
                .WillCascadeOnDelete(false);

            // AssetMaintenance -> MaintenanceWork
            modelBuilder.Entity<AssetMaintenance>()
                .HasRequired(am => am.MaintenanceWork)
                .WithMany()
                .HasForeignKey(am => am.MaintenanceWorkID)
                .WillCascadeOnDelete(false);

            // AssetMaintenance -> Administrator
            modelBuilder.Entity<AssetMaintenance>()
                .HasRequired(am => am.LinkedByAdministrator)
                .WithMany()
                .HasForeignKey(am => am.LinkedByAdministratorID)
                .WillCascadeOnDelete(false);

            // AssetProject -> MunicipalAsset
            modelBuilder.Entity<AssetProject>()
                .HasRequired(ap => ap.Asset)
                .WithMany(a => a.AssetProjects)
                .HasForeignKey(ap => ap.AssetID)
                .WillCascadeOnDelete(false);

            // AssetProject -> Administrator
            modelBuilder.Entity<AssetProject>()
                .HasRequired(ap => ap.LinkedByAdministrator)
                .WithMany()
                .HasForeignKey(ap => ap.LinkedByAdministratorID)
                .WillCascadeOnDelete(false);

            // AssetHistory -> MunicipalAsset
            modelBuilder.Entity<AssetHistory>()
                .HasRequired(h => h.Asset)
                .WithMany(a => a.AssetHistory)
                .HasForeignKey(h => h.AssetID)
                .WillCascadeOnDelete(false);

            // AssetHistory -> Administrator
            modelBuilder.Entity<AssetHistory>()
                .HasRequired(h => h.Administrator)
                .WithMany()
                .HasForeignKey(h => h.AdministratorID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MunicipalAsset>()
            .HasRequired(a => a.Ward)
            .WithMany()
             .HasForeignKey(a => a.WardID)
            .WillCascadeOnDelete(false);

            modelBuilder.Entity<MunicipalProject>()
           .HasRequired(p => p.Ward)
           .WithMany()
           .HasForeignKey(p => p.WardID)
           .WillCascadeOnDelete(false);

            modelBuilder.Entity<MunicipalProject>()
                .HasRequired(p => p.ResponsibleAdministrator)
                .WithMany()
                .HasForeignKey(p => p.ResponsibleAdministratorID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MunicipalProject>()
                .HasRequired(p => p.CreatedByAdministrator)
                .WithMany()
                .HasForeignKey(p => p.CreatedByAdministratorID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MunicipalProject>()
                .HasOptional(p => p.LastUpdatedByAdministrator)
                .WithMany()
                .HasForeignKey(p => p.LastUpdatedByAdministratorID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AssetProject>()
           .HasRequired(ap => ap.Project)
          .WithMany(p => p.AssetProjects)
            .HasForeignKey(ap => ap.ProjectID)
             .WillCascadeOnDelete(false);


            modelBuilder.Entity<ProjectObjective>()
    .HasRequired(p => p.Project)
    .WithMany(p => p.ProjectObjectives)
    .HasForeignKey(p => p.ProjectID)
    .WillCascadeOnDelete(false);

            modelBuilder.Entity<ProjectObjective>()
                .HasRequired(p => p.CreatedByAdministrator)
                .WithMany()
                .HasForeignKey(p => p.CreatedByAdministratorID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ProjectObjective>()
                .HasOptional(p => p.LastUpdatedByAdministrator)
                .WithMany()
                .HasForeignKey(p => p.LastUpdatedByAdministratorID)
                .WillCascadeOnDelete(false);


            modelBuilder.Entity<ProjectMilestone>()
                .HasRequired(p => p.Project)
                .WithMany(p => p.ProjectMilestones)
                .HasForeignKey(p => p.ProjectID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ProjectMilestone>()
                .HasRequired(p => p.CreatedByAdministrator)
                .WithMany()
                .HasForeignKey(p => p.CreatedByAdministratorID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ProjectMilestone>()
                .HasOptional(p => p.LastUpdatedByAdministrator)
                .WithMany()
                .HasForeignKey(p => p.LastUpdatedByAdministratorID)
                .WillCascadeOnDelete(false);


            modelBuilder.Entity<ProjectProgress>()
                .HasRequired(p => p.Project)
                .WithMany(p => p.ProjectProgressRecords)
                .HasForeignKey(p => p.ProjectID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ProjectProgress>()
                .HasRequired(p => p.RecordedByAdministrator)
                .WithMany()
                .HasForeignKey(p => p.RecordedByAdministratorID)
                .WillCascadeOnDelete(false);


            modelBuilder.Entity<ProjectEvidence>()
                .HasRequired(p => p.Project)
                .WithMany(p => p.ProjectEvidenceRecords)
                .HasForeignKey(p => p.ProjectID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ProjectEvidence>()
                .HasRequired(p => p.RecordedByAdministrator)
                .WithMany()
                .HasForeignKey(p => p.RecordedByAdministratorID)
                .WillCascadeOnDelete(false);


            modelBuilder.Entity<ProjectHistory>()
                .HasRequired(p => p.Project)
                .WithMany(p => p.ProjectHistoryRecords)
                .HasForeignKey(p => p.ProjectID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ProjectHistory>()
                .HasRequired(p => p.PerformedByAdministrator)
                .WithMany()
                .HasForeignKey(p => p.PerformedByAdministratorID)
                .WillCascadeOnDelete(false);


            modelBuilder.Entity<ProjectRequest>()
                .HasRequired(p => p.Project)
                .WithMany(p => p.ProjectRequests)
                .HasForeignKey(p => p.ProjectID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ProjectRequest>()
                .HasRequired(p => p.Request)
                .WithMany()
                .HasForeignKey(p => p.RequestID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ProjectRequest>()
                .HasRequired(p => p.LinkedByAdministrator)
                .WithMany()
                .HasForeignKey(p => p.LinkedByAdministratorID)
                .WillCascadeOnDelete(false);


            modelBuilder.Entity<AssetProject>()
                .HasRequired(p => p.Project)
                .WithMany(p => p.AssetProjects)
                .HasForeignKey(p => p.ProjectID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TechnicianApplication>()
    .HasIndex(
        a => new
        {
            a.OpportunityID,
            a.CitizenID
        })
    .IsUnique();


            modelBuilder.Entity<TechnicianApplicationSelection>()
    .HasRequired(s => s.Application)
    .WithMany()
    .HasForeignKey(s => s.ApplicationID)
    .WillCascadeOnDelete(false);

            modelBuilder.Entity<TechnicianApplicationSelection>()
                .HasRequired(s => s.SelectedByAdministrator)
                .WithMany()
                .HasForeignKey(s => s.SelectedByAdministratorID)
                .WillCascadeOnDelete(false);

            // Optional HR officer selector. Keep nullable to preserve existing data.
            modelBuilder.Entity<TechnicianApplicationSelection>()
                .HasOptional(s => s.SelectedByHROfficer)
                .WithMany()
                .HasForeignKey(s => s.SelectedByHROfficerID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TechnicianApplicationFinalVerification>()
    .HasRequired(v => v.Application)
    .WithMany()
    .HasForeignKey(v => v.ApplicationID)
    .WillCascadeOnDelete(false);

            modelBuilder.Entity<TechnicianApplicationFinalVerification>()
                .HasRequired(v => v.VerifiedByAdministrator)
                .WithMany()
                .HasForeignKey(v => v.VerifiedByAdministratorID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TechnicianOnboarding>()
    .HasRequired(t => t.Application)
    .WithMany()
    .HasForeignKey(t => t.ApplicationID)
    .WillCascadeOnDelete(false);

            modelBuilder.Entity<TechnicianOnboarding>()
                .HasRequired(t => t.Technician)
                .WithMany()
                .HasForeignKey(t => t.TechnicianID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TechnicianOnboarding>()
                .HasRequired(t => t.OnboardedByAdministrator)
                .WithMany()
                .HasForeignKey(t => t.OnboardedByAdministratorID)
                .WillCascadeOnDelete(false);

            // Optional HR officer onboarding record for handover tracking
            modelBuilder.Entity<TechnicianOnboarding>()
                .HasOptional(t => t.OnboardedByHROfficer)
                .WithMany()
                .HasForeignKey(t => t.OnboardedByHROfficerID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Technician>()
                .HasOptional(t => t.Citizen)
                .WithMany()
                .HasForeignKey(t => t.CitizenID)
                .WillCascadeOnDelete(false);

           
modelBuilder.Entity<TechnicianApplicationNotification>()
    .HasRequired(n => n.Application)
    .WithMany()
    .HasForeignKey(n => n.ApplicationID)
    .WillCascadeOnDelete(false);

            modelBuilder.Entity<TechnicianApplicationNotification>()
                .HasRequired(n => n.Citizen)
                .WithMany()
                .HasForeignKey(n => n.CitizenID)
                .WillCascadeOnDelete(false);

            // ============================================================
            // FINANCE OFFICER
            // ============================================================

            modelBuilder.Entity<FinancialAudit>()
                .HasRequired(a => a.FinanceOfficer)
                .WithMany(f => f.FinancialAudits)
                .HasForeignKey(a => a.FinanceOfficerID)
                .WillCascadeOnDelete(false);


            // ============================================================
            // MUNICIPAL SERVICE TYPES
            // ============================================================

            modelBuilder.Entity<ServiceType>()
                .HasMany(s => s.FeeSchedules)
                .WithRequired(f => f.ServiceType)
                .HasForeignKey(f => f.ServiceTypeID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ServiceType>()
                .HasMany(s => s.MunicipalServiceRequests)
                .WithRequired(r => r.ServiceType)
                .HasForeignKey(r => r.ServiceTypeID)
                .WillCascadeOnDelete(false);


            // ============================================================
            // FEE SCHEDULE
            // ============================================================

            modelBuilder.Entity<FeeSchedule>()
                .HasRequired(f => f.CreatedByFinanceOfficer)
                .WithMany()
                .HasForeignKey(f => f.CreatedByFinanceOfficerID)
                .WillCascadeOnDelete(false);


            // ============================================================
            // MUNICIPAL SERVICE REQUEST
            // ============================================================

            // Citizen → Municipal Service Requests
            modelBuilder.Entity<MunicipalServiceRequest>()
                .HasRequired(r => r.Citizen)
                .WithMany()
                .HasForeignKey(r => r.CitizenID)
                .WillCascadeOnDelete(false);

            // Administrator review is optional because a newly submitted
            // service request has not necessarily been reviewed yet.
            modelBuilder.Entity<MunicipalServiceRequest>()
                .HasOptional(r => r.ReviewedByAdministrator)
                .WithMany()
                .HasForeignKey(r => r.ReviewedByAdministratorID)
                .WillCascadeOnDelete(false);

            // Municipal Service Request → Invoice
            //
            // A request can have zero or more invoices.
            // Normally the application will create one invoice, but the
            // database relationship remains flexible for future adjustments.
            modelBuilder.Entity<Invoice>()
                .HasRequired(i => i.MunicipalServiceRequest)
                .WithMany(r => r.Invoices)
                .HasForeignKey(i => i.MunicipalServiceRequestID)
                .WillCascadeOnDelete(false);


            // ============================================================
            // INVOICE
            // ============================================================

            // Invoice → Citizen
            modelBuilder.Entity<Invoice>()
                .HasRequired(i => i.Citizen)
                .WithMany()
                .HasForeignKey(i => i.CitizenID)
                .WillCascadeOnDelete(false);

            // Invoice → Fee Schedule
            modelBuilder.Entity<Invoice>()
                .HasRequired(i => i.FeeSchedule)
                .WithMany()
                .HasForeignKey(i => i.FeeScheduleID)
                .WillCascadeOnDelete(false);


            // ============================================================
            // PAYMENT
            // ============================================================

            // Invoice → Payments
            //
            // An invoice may have multiple payment attempts.
            // Example:
            //
            // Invoice
            //    ├── Failed payment
            //    ├── Failed payment
            //    └── Successful payment
            //
            modelBuilder.Entity<Payment>()
    .HasRequired(p => p.Invoice)
    .WithMany(i => i.Payments)
    .HasForeignKey(p => p.InvoiceID)
    .WillCascadeOnDelete(false);

            // Payment → Citizen
            modelBuilder.Entity<Payment>()
                .HasRequired(p => p.Citizen)
                .WithMany()
                .HasForeignKey(p => p.CitizenID)
                .WillCascadeOnDelete(false);


            // ============================================================
            // RECEIPT
            // ============================================================

            // Payment → Receipt
            //
            // A successful payment can have one receipt.
            modelBuilder.Entity<Receipt>()
    .HasRequired(r => r.Payment)
    .WithMany(p => p.Receipts)
    .HasForeignKey(r => r.PaymentID)
    .WillCascadeOnDelete(false);


            // ============================================================
            // REFUND
            // ============================================================

            // Payment → Refund
            modelBuilder.Entity<Refund>()
                .HasRequired(r => r.Payment)
                .WithMany()
                .HasForeignKey(r => r.PaymentID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Refund>()
    .HasRequired(r => r.Invoice)
    .WithMany(i => i.Refunds)
    .HasForeignKey(r => r.InvoiceID)
    .WillCascadeOnDelete(false); ;

            // Finance Officer → Refund
            //
            // Finance Officer is optional because a refund may exist before
            // it has been processed.
            modelBuilder.Entity<Refund>()
                .HasOptional(r => r.ProcessedByFinanceOfficer)
                .WithMany()
                .HasForeignKey(r => r.ProcessedByFinanceOfficerID)
                .WillCascadeOnDelete(false);


            // ============================================================
            // TECHNICIAN PAYROLL PROFILE
            // ============================================================

            // Technician → Payroll Profile
            //
            // A technician can have zero or one payroll profile.
            modelBuilder.Entity<TechnicianPayrollProfile>()
    .HasRequired(p => p.Technician)
    .WithMany()
    .HasForeignKey(p => p.TechnicianID)
    .WillCascadeOnDelete(false);


            // ============================================================
            // OVERTIME CLAIM
            // ============================================================

            // Technician → Overtime Claims
            modelBuilder.Entity<OvertimeClaim>()
                .HasRequired(o => o.Technician)
                .WithMany()
                .HasForeignKey(o => o.TechnicianID)
                .WillCascadeOnDelete(false);

            // Administrator → Overtime Claims
            //
            // Administrator review is optional until the claim is reviewed.
            modelBuilder.Entity<OvertimeClaim>()
                .HasOptional(o => o.ReviewedByAdministrator)
                .WithMany()
                .HasForeignKey(o => o.ReviewedByAdministratorID)
                .WillCascadeOnDelete(false);

            // Payroll → Overtime Claims
            //
            // An overtime claim can exist without being included in payroll.
            // Once included, PayrollID identifies the payroll period.
            modelBuilder.Entity<OvertimeClaim>()
                .HasOptional(o => o.Payroll)
                .WithMany(p => p.OvertimeClaims)
                .HasForeignKey(o => o.PayrollID)
                .WillCascadeOnDelete(false);


            // ============================================================
            // PAYROLL PERIOD
            // ============================================================

            modelBuilder.Entity<PayrollPeriod>()
                .HasMany(p => p.Payrolls)
                .WithRequired(p => p.PayrollPeriod)
                .HasForeignKey(p => p.PayrollPeriodID)
                .WillCascadeOnDelete(false);


            // ============================================================
            // PAYROLL
            // ============================================================

            // Technician → Payroll
            modelBuilder.Entity<Payroll>()
                .HasRequired(p => p.Technician)
                .WithMany()
                .HasForeignKey(p => p.TechnicianID)
                .WillCascadeOnDelete(false);

            // Finance Officer → Approved Payroll
            //
            // Finance Officer is optional until payroll has been approved.
            modelBuilder.Entity<Payroll>()
                .HasOptional(p => p.ApprovedByFinanceOfficer)
                .WithMany()
                .HasForeignKey(p => p.ApprovedByFinanceOfficerID)
                .WillCascadeOnDelete(false);


            // ============================================================
            // PAYROLL ALLOWANCES
            // ============================================================

            modelBuilder.Entity<PayrollAllowance>()
                .HasRequired(a => a.Payroll)
                .WithMany(p => p.Allowances)
                .HasForeignKey(a => a.PayrollID)
                .WillCascadeOnDelete(false);


            // ============================================================
            // PAYROLL DEDUCTIONS
            // ============================================================

            modelBuilder.Entity<PayrollDeduction>()
                .HasRequired(d => d.Payroll)
                .WithMany(p => p.Deductions)
                .HasForeignKey(d => d.PayrollID)
                .WillCascadeOnDelete(false);


            // ============================================================
            // PAYROLL PAYMENT
            // ============================================================

            // Payroll → Payroll Payment
            //
            modelBuilder.Entity<PayrollPayment>()
    .HasRequired(p => p.Payroll)
    .WithMany()
    .HasForeignKey(p => p.PayrollID)
    .WillCascadeOnDelete(false);


            // ============================================================
            // PAYSLIP
            // ============================================================

            // Payroll → Payslip
            //
            // A completed payroll can have zero or one payslip.
            modelBuilder.Entity<Payslip>()
    .HasRequired(p => p.Payroll)
    .WithMany()
    .HasForeignKey(p => p.PayrollID)
    .WillCascadeOnDelete(false);

            // Technician → Payslip
            modelBuilder.Entity<Payslip>()
                .HasRequired(p => p.Technician)
                .WithMany()
                .HasForeignKey(p => p.TechnicianID)
                .WillCascadeOnDelete(false);

            // ============================================================
            // FINANCE NOTIFICATIONS
            // ============================================================

            // Finance Notification -> Citizen
            modelBuilder.Entity<FinanceNotification>()
                .HasRequired(n => n.Citizen)
                .WithMany()
                .HasForeignKey(n => n.CitizenID)
                .WillCascadeOnDelete(false);

            // Finance Notification -> Invoice
            modelBuilder.Entity<FinanceNotification>()
                .HasOptional(n => n.Invoice)
                .WithMany()
                .HasForeignKey(n => n.InvoiceID)
                .WillCascadeOnDelete(false);

            // Finance Notification -> Payment
            modelBuilder.Entity<FinanceNotification>()
                .HasOptional(n => n.Payment)
                .WithMany()
                .HasForeignKey(n => n.PaymentID)
                .WillCascadeOnDelete(false);

            // Finance Notification -> Refund
            modelBuilder.Entity<FinanceNotification>()
                .HasOptional(n => n.Refund)
                .WithMany()
                .HasForeignKey(n => n.RefundID)
                .WillCascadeOnDelete(false);

            // Finance Notification -> Receipt
            modelBuilder.Entity<FinanceNotification>()
                .HasOptional(n => n.Receipt)
                .WithMany()
                .HasForeignKey(n => n.ReceiptID)
                .WillCascadeOnDelete(false);



        }
    }


}
