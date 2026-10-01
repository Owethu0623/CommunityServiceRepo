namespace CommunityServiceProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddMunicipalFinanceAndPayroll : DbMigration
    {
        public override void Up()
        {
            // =========================================================
            // 1. SERVICE TYPES
            // =========================================================

            CreateTable(
                "dbo.ServiceTypes",
                c => new
                {
                    ServiceTypeID = c.Int(nullable: false, identity: true),
                    ServiceCode = c.String(nullable: false, maxLength: 30),
                    ServiceName = c.String(nullable: false, maxLength: 150),
                    Description = c.String(maxLength: 500),
                    IsChargeable = c.Boolean(nullable: false),
                    IsActive = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.ServiceTypeID);


            // =========================================================
            // 2. FEE SCHEDULES
            // Depends on ServiceTypes and FinanceOfficers
            // =========================================================

            CreateTable(
                "dbo.FeeSchedules",
                c => new
                {
                    FeeScheduleID = c.Int(nullable: false, identity: true),
                    ServiceTypeID = c.Int(nullable: false),
                    FeeType = c.String(nullable: false, maxLength: 50),
                    Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    EffectiveFrom = c.DateTime(nullable: false),
                    EffectiveTo = c.DateTime(),
                    IsActive = c.Boolean(nullable: false),
                    CreatedByFinanceOfficerID = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.FeeScheduleID)
                .ForeignKey("dbo.FinanceOfficers", t => t.CreatedByFinanceOfficerID)
                .ForeignKey("dbo.ServiceTypes", t => t.ServiceTypeID)
                .Index(t => t.ServiceTypeID)
                .Index(t => t.CreatedByFinanceOfficerID);


            // =========================================================
            // 3. FINANCIAL AUDIT
            // Depends on FinanceOfficers
            // =========================================================

            CreateTable(
                "dbo.FinancialAudits",
                c => new
                {
                    FinancialAuditID = c.Int(nullable: false, identity: true),
                    FinanceOfficerID = c.Int(nullable: false),
                    EntityName = c.String(nullable: false, maxLength: 100),
                    EntityID = c.Int(nullable: false),
                    Action = c.Int(nullable: false),
                    Description = c.String(maxLength: 1000),
                    DateCreated = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.FinancialAuditID)
                .ForeignKey("dbo.FinanceOfficers", t => t.FinanceOfficerID)
                .Index(t => t.FinanceOfficerID);


            // =========================================================
            // 4. MUNICIPAL SERVICE REQUESTS
            // Depends on Citizens, Administrators and ServiceTypes
            // =========================================================

            CreateTable(
                "dbo.MunicipalServiceRequests",
                c => new
                {
                    MunicipalServiceRequestID = c.Int(nullable: false, identity: true),
                    ReferenceNumber = c.String(nullable: false, maxLength: 30),
                    CitizenID = c.Int(nullable: false),
                    ServiceTypeID = c.Int(nullable: false),
                    Title = c.String(nullable: false, maxLength: 200),
                    Description = c.String(nullable: false, maxLength: 2000),
                    AdditionalInformation = c.String(maxLength: 500),
                    Status = c.Int(nullable: false),
                    DateSubmitted = c.DateTime(nullable: false),
                    DateReviewed = c.DateTime(),
                    DateApproved = c.DateTime(),
                    DateCompleted = c.DateTime(),
                    ReviewedByAdministratorID = c.Int(),
                })
                .PrimaryKey(t => t.MunicipalServiceRequestID)
                .ForeignKey("dbo.Citizens", t => t.CitizenID)
                .ForeignKey("dbo.Administrators", t => t.ReviewedByAdministratorID)
                .ForeignKey("dbo.ServiceTypes", t => t.ServiceTypeID)
                .Index(t => t.ReferenceNumber, unique: true)
                .Index(t => t.CitizenID)
                .Index(t => t.ServiceTypeID)
                .Index(t => t.ReviewedByAdministratorID);


            // =========================================================
            // 5. INVOICES
            // Depends on MunicipalServiceRequests, Citizens,
            // and FeeSchedules
            // =========================================================

            CreateTable(
                "dbo.Invoices",
                c => new
                {
                    InvoiceID = c.Int(nullable: false, identity: true),
                    InvoiceNumber = c.String(nullable: false, maxLength: 30),
                    MunicipalServiceRequestID = c.Int(nullable: false),
                    CitizenID = c.Int(nullable: false),
                    FeeScheduleID = c.Int(nullable: false),
                    Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    AmountPaid = c.Decimal(nullable: false, precision: 18, scale: 2),
                    Balance = c.Decimal(nullable: false, precision: 18, scale: 2),
                    Status = c.Int(nullable: false),
                    IssueDate = c.DateTime(nullable: false),
                    DueDate = c.DateTime(),
                    PaidDate = c.DateTime(),
                })
                .PrimaryKey(t => t.InvoiceID)
                .ForeignKey("dbo.Citizens", t => t.CitizenID)
                .ForeignKey("dbo.FeeSchedules", t => t.FeeScheduleID)
                .ForeignKey("dbo.MunicipalServiceRequests", t => t.MunicipalServiceRequestID)
                .Index(t => t.InvoiceNumber, unique: true)
                .Index(t => t.MunicipalServiceRequestID)
                .Index(t => t.CitizenID)
                .Index(t => t.FeeScheduleID);


            // =========================================================
            // 6. PAYMENTS
            // Depends on Invoices and Citizens
            // =========================================================

            CreateTable(
                "dbo.Payments",
                c => new
                {
                    PaymentID = c.Int(nullable: false, identity: true),
                    TransactionReference = c.String(nullable: false, maxLength: 40),
                    InvoiceID = c.Int(nullable: false),
                    CitizenID = c.Int(nullable: false),
                    Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    Status = c.Int(nullable: false),
                    PaymentDate = c.DateTime(nullable: false),
                    ProcessedDate = c.DateTime(),
                    PaymentMethod = c.String(maxLength: 100),
                    FailureReason = c.String(maxLength: 500),
                })
                .PrimaryKey(t => t.PaymentID)
                .ForeignKey("dbo.Citizens", t => t.CitizenID)
                .ForeignKey("dbo.Invoices", t => t.InvoiceID)
                .Index(t => t.TransactionReference, unique: true)
                .Index(t => t.InvoiceID)
                .Index(t => t.CitizenID);


            // =========================================================
            // 7. RECEIPTS
            // Depends on Payments
            // =========================================================

            CreateTable(
                "dbo.Receipts",
                c => new
                {
                    ReceiptID = c.Int(nullable: false, identity: true),
                    ReceiptNumber = c.String(nullable: false, maxLength: 40),
                    PaymentID = c.Int(nullable: false),
                    IssueDate = c.DateTime(nullable: false),
                    Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                })
                .PrimaryKey(t => t.ReceiptID)
                .ForeignKey("dbo.Payments", t => t.PaymentID)
                .Index(t => t.PaymentID, unique: true, name: "IX_Receipt_PaymentID");


            // =========================================================
            // 8. REFUNDS
            // Depends on Payments, Invoices and FinanceOfficers
            // =========================================================

            CreateTable(
                "dbo.Refunds",
                c => new
                {
                    RefundID = c.Int(nullable: false, identity: true),
                    RefundReference = c.String(nullable: false, maxLength: 40),
                    PaymentID = c.Int(nullable: false),
                    InvoiceID = c.Int(nullable: false),
                    Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    Status = c.Int(nullable: false),
                    RequestDate = c.DateTime(nullable: false),
                    ProcessedDate = c.DateTime(),
                    ProcessedByFinanceOfficerID = c.Int(),
                    Reason = c.String(maxLength: 500),
                })
                .PrimaryKey(t => t.RefundID)
                .ForeignKey("dbo.Invoices", t => t.InvoiceID)
                .ForeignKey("dbo.Payments", t => t.PaymentID)
                .ForeignKey("dbo.FinanceOfficers", t => t.ProcessedByFinanceOfficerID)
                .Index(t => t.PaymentID)
                .Index(t => t.InvoiceID)
                .Index(t => t.ProcessedByFinanceOfficerID);


            // =========================================================
            // 9. PAYROLL PERIODS
            // Must exist before Payrolls
            // =========================================================

            CreateTable(
                "dbo.PayrollPeriods",
                c => new
                {
                    PayrollPeriodID = c.Int(nullable: false, identity: true),
                    PeriodName = c.String(nullable: false, maxLength: 20),
                    StartDate = c.DateTime(nullable: false),
                    EndDate = c.DateTime(nullable: false),
                    Status = c.Int(nullable: false),
                    DateCreated = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.PayrollPeriodID);


            // =========================================================
            // 10. PAYROLLS
            // Depends on PayrollPeriods, Technicians,
            // and FinanceOfficers
            //
            // IMPORTANT:
            // No PayrollPayment_PayrollPaymentID
            // No Payslip_PayslipID
            // =========================================================

            CreateTable(
                "dbo.Payrolls",
                c => new
                {
                    PayrollID = c.Int(nullable: false, identity: true),
                    PayrollPeriodID = c.Int(nullable: false),
                    TechnicianID = c.Int(nullable: false),
                    BasicSalary = c.Decimal(nullable: false, precision: 18, scale: 2),
                    OvertimeAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    TotalAllowances = c.Decimal(nullable: false, precision: 18, scale: 2),
                    TotalDeductions = c.Decimal(nullable: false, precision: 18, scale: 2),
                    GrossPay = c.Decimal(nullable: false, precision: 18, scale: 2),
                    NetPay = c.Decimal(nullable: false, precision: 18, scale: 2),
                    Status = c.Int(nullable: false),
                    DateCreated = c.DateTime(nullable: false),
                    CalculatedDate = c.DateTime(),
                    ApprovedDate = c.DateTime(),
                    PaidDate = c.DateTime(),
                    ApprovedByFinanceOfficerID = c.Int(),
                })
                .PrimaryKey(t => t.PayrollID)
                .ForeignKey("dbo.FinanceOfficers", t => t.ApprovedByFinanceOfficerID)
                .ForeignKey("dbo.PayrollPeriods", t => t.PayrollPeriodID)
                .ForeignKey("dbo.Technicians", t => t.TechnicianID)
                .Index(t => t.PayrollPeriodID)
                .Index(t => t.TechnicianID)
                .Index(t => t.ApprovedByFinanceOfficerID);


            // =========================================================
            // 11. PAYROLL ALLOWANCES
            // Depends on Payrolls
            // =========================================================

            CreateTable(
                "dbo.PayrollAllowances",
                c => new
                {
                    PayrollAllowanceID = c.Int(nullable: false, identity: true),
                    PayrollID = c.Int(nullable: false),
                    AllowanceType = c.String(nullable: false, maxLength: 100),
                    Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                })
                .PrimaryKey(t => t.PayrollAllowanceID)
                .ForeignKey("dbo.Payrolls", t => t.PayrollID)
                .Index(t => t.PayrollID);


            // =========================================================
            // 12. PAYROLL DEDUCTIONS
            // Depends on Payrolls
            // =========================================================

            CreateTable(
                "dbo.PayrollDeductions",
                c => new
                {
                    PayrollDeductionID = c.Int(nullable: false, identity: true),
                    PayrollID = c.Int(nullable: false),
                    DeductionType = c.String(nullable: false, maxLength: 100),
                    Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                })
                .PrimaryKey(t => t.PayrollDeductionID)
                .ForeignKey("dbo.Payrolls", t => t.PayrollID)
                .Index(t => t.PayrollID);


            // =========================================================
            // 13. PAYROLL PAYMENTS
            // Depends on Payrolls
            //
            // Unique PayrollID = one payroll payment per payroll
            // =========================================================

            CreateTable(
                "dbo.PayrollPayments",
                c => new
                {
                    PayrollPaymentID = c.Int(nullable: false, identity: true),
                    PayrollID = c.Int(nullable: false),
                    TransactionReference = c.String(nullable: false, maxLength: 40),
                    Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    Status = c.Int(nullable: false),
                    PaymentDate = c.DateTime(nullable: false),
                    ProcessedDate = c.DateTime(),
                    FailureReason = c.String(maxLength: 500),
                })
                .PrimaryKey(t => t.PayrollPaymentID)
                .ForeignKey("dbo.Payrolls", t => t.PayrollID)
                .Index(t => t.PayrollID, unique: true, name: "IX_PayrollPayment_PayrollID");


            // =========================================================
            // 14. PAYSLIPS
            // Depends on Payrolls and Technicians
            //
            // Unique PayrollID = one payslip per payroll
            // =========================================================

            CreateTable(
                "dbo.Payslips",
                c => new
                {
                    PayslipID = c.Int(nullable: false, identity: true),
                    PayslipNumber = c.String(nullable: false, maxLength: 40),
                    PayrollID = c.Int(nullable: false),
                    TechnicianID = c.Int(nullable: false),
                    GrossPay = c.Decimal(nullable: false, precision: 18, scale: 2),
                    TotalDeductions = c.Decimal(nullable: false, precision: 18, scale: 2),
                    NetPay = c.Decimal(nullable: false, precision: 18, scale: 2),
                    IssueDate = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.PayslipID)
                .ForeignKey("dbo.Payrolls", t => t.PayrollID)
                .ForeignKey("dbo.Technicians", t => t.TechnicianID)
                .Index(t => t.PayrollID, unique: true, name: "IX_Payslip_PayrollID")
                .Index(t => t.TechnicianID);


            // =========================================================
            // 15. OVERTIME CLAIMS
            // Depends on Payrolls, Technicians and Administrators
            //
            // PayrollID is nullable because approved overtime
            // does not have to be attached to payroll immediately.
            // =========================================================

            CreateTable(
                "dbo.OvertimeClaims",
                c => new
                {
                    OvertimeClaimID = c.Int(nullable: false, identity: true),
                    TechnicianID = c.Int(nullable: false),
                    PayrollID = c.Int(),
                    OvertimeDate = c.DateTime(nullable: false),
                    Hours = c.Decimal(nullable: false, precision: 18, scale: 2),
                    Reason = c.String(nullable: false, maxLength: 1000),
                    Status = c.Int(nullable: false),
                    DateSubmitted = c.DateTime(nullable: false),
                    ReviewDate = c.DateTime(),
                    ReviewedByAdministratorID = c.Int(),
                    AdministratorComment = c.String(maxLength: 500),
                })
                .PrimaryKey(t => t.OvertimeClaimID)
                .ForeignKey("dbo.Payrolls", t => t.PayrollID)
                .ForeignKey("dbo.Administrators", t => t.ReviewedByAdministratorID)
                .ForeignKey("dbo.Technicians", t => t.TechnicianID)
                .Index(t => t.TechnicianID)
                .Index(t => t.PayrollID)
                .Index(t => t.ReviewedByAdministratorID);


            // =========================================================
            // 16. TECHNICIAN PAYROLL PROFILES
            // Depends on Technicians
            //
            // Unique TechnicianID = one active payroll profile
            // per technician.
            // =========================================================

            CreateTable(
                "dbo.TechnicianPayrollProfiles",
                c => new
                {
                    TechnicianPayrollProfileID = c.Int(nullable: false, identity: true),
                    TechnicianID = c.Int(nullable: false),
                    BasicSalary = c.Decimal(nullable: false, precision: 18, scale: 2),
                    IsActive = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.TechnicianPayrollProfileID)
                .ForeignKey("dbo.Technicians", t => t.TechnicianID)
                .Index(
                    t => t.TechnicianID,
                    unique: true,
                    name: "IX_TechnicianPayrollProfile_TechnicianID");
        }

        public override void Down()
        {
            DropForeignKey("dbo.TechnicianPayrollProfiles", "TechnicianID", "dbo.Technicians");

            DropForeignKey("dbo.OvertimeClaims", "TechnicianID", "dbo.Technicians");
            DropForeignKey("dbo.OvertimeClaims", "ReviewedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.OvertimeClaims", "PayrollID", "dbo.Payrolls");

            DropForeignKey("dbo.Payslips", "TechnicianID", "dbo.Technicians");
            DropForeignKey("dbo.Payslips", "PayrollID", "dbo.Payrolls");

            DropForeignKey("dbo.PayrollPayments", "PayrollID", "dbo.Payrolls");

            DropForeignKey("dbo.PayrollDeductions", "PayrollID", "dbo.Payrolls");
            DropForeignKey("dbo.PayrollAllowances", "PayrollID", "dbo.Payrolls");

            DropForeignKey("dbo.Payrolls", "TechnicianID", "dbo.Technicians");
            DropForeignKey("dbo.Payrolls", "PayrollPeriodID", "dbo.PayrollPeriods");
            DropForeignKey("dbo.Payrolls", "ApprovedByFinanceOfficerID", "dbo.FinanceOfficers");

            DropForeignKey("dbo.Refunds", "ProcessedByFinanceOfficerID", "dbo.FinanceOfficers");
            DropForeignKey("dbo.Refunds", "PaymentID", "dbo.Payments");
            DropForeignKey("dbo.Refunds", "InvoiceID", "dbo.Invoices");

            DropForeignKey("dbo.Receipts", "PaymentID", "dbo.Payments");

            DropForeignKey("dbo.Payments", "CitizenID", "dbo.Citizens");
            DropForeignKey("dbo.Payments", "InvoiceID", "dbo.Invoices");

            DropForeignKey("dbo.Invoices", "MunicipalServiceRequestID", "dbo.MunicipalServiceRequests");
            DropForeignKey("dbo.Invoices", "FeeScheduleID", "dbo.FeeSchedules");
            DropForeignKey("dbo.Invoices", "CitizenID", "dbo.Citizens");

            DropForeignKey("dbo.MunicipalServiceRequests", "ServiceTypeID", "dbo.ServiceTypes");
            DropForeignKey("dbo.MunicipalServiceRequests", "ReviewedByAdministratorID", "dbo.Administrators");
            DropForeignKey("dbo.MunicipalServiceRequests", "CitizenID", "dbo.Citizens");

            DropForeignKey("dbo.FinancialAudits", "FinanceOfficerID", "dbo.FinanceOfficers");

            DropForeignKey("dbo.FeeSchedules", "ServiceTypeID", "dbo.ServiceTypes");
            DropForeignKey("dbo.FeeSchedules", "CreatedByFinanceOfficerID", "dbo.FinanceOfficers");


            DropIndex(
                "dbo.TechnicianPayrollProfiles",
                "IX_TechnicianPayrollProfile_TechnicianID");

            DropIndex("dbo.OvertimeClaims", new[] { "ReviewedByAdministratorID" });
            DropIndex("dbo.OvertimeClaims", new[] { "PayrollID" });
            DropIndex("dbo.OvertimeClaims", new[] { "TechnicianID" });

            DropIndex("dbo.Payslips", new[] { "TechnicianID" });
            DropIndex("dbo.Payslips", "IX_Payslip_PayrollID");

            DropIndex("dbo.PayrollPayments", "IX_PayrollPayment_PayrollID");

            DropIndex("dbo.PayrollDeductions", new[] { "PayrollID" });
            DropIndex("dbo.PayrollAllowances", new[] { "PayrollID" });

            DropIndex("dbo.Payrolls", new[] { "ApprovedByFinanceOfficerID" });
            DropIndex("dbo.Payrolls", new[] { "TechnicianID" });
            DropIndex("dbo.Payrolls", new[] { "PayrollPeriodID" });

            DropIndex("dbo.Refunds", new[] { "ProcessedByFinanceOfficerID" });
            DropIndex("dbo.Refunds", new[] { "InvoiceID" });
            DropIndex("dbo.Refunds", new[] { "PaymentID" });

            DropIndex("dbo.Receipts", "IX_Receipt_PaymentID");

            DropIndex("dbo.Payments", new[] { "CitizenID" });
            DropIndex("dbo.Payments", new[] { "InvoiceID" });
            DropIndex("dbo.Payments", new[] { "TransactionReference" });

            DropIndex("dbo.Invoices", new[] { "FeeScheduleID" });
            DropIndex("dbo.Invoices", new[] { "CitizenID" });
            DropIndex("dbo.Invoices", new[] { "MunicipalServiceRequestID" });
            DropIndex("dbo.Invoices", new[] { "InvoiceNumber" });

            DropIndex("dbo.MunicipalServiceRequests", new[] { "ReviewedByAdministratorID" });
            DropIndex("dbo.MunicipalServiceRequests", new[] { "ServiceTypeID" });
            DropIndex("dbo.MunicipalServiceRequests", new[] { "CitizenID" });
            DropIndex("dbo.MunicipalServiceRequests", new[] { "ReferenceNumber" });

            DropIndex("dbo.FinancialAudits", new[] { "FinanceOfficerID" });

            DropIndex("dbo.FeeSchedules", new[] { "CreatedByFinanceOfficerID" });
            DropIndex("dbo.FeeSchedules", new[] { "ServiceTypeID" });


            DropTable("dbo.TechnicianPayrollProfiles");
            DropTable("dbo.OvertimeClaims");
            DropTable("dbo.Payslips");
            DropTable("dbo.PayrollPayments");
            DropTable("dbo.PayrollDeductions");
            DropTable("dbo.PayrollAllowances");
            DropTable("dbo.Payrolls");
            DropTable("dbo.PayrollPeriods");
            DropTable("dbo.Refunds");
            DropTable("dbo.Receipts");
            DropTable("dbo.Payments");
            DropTable("dbo.Invoices");
            DropTable("dbo.MunicipalServiceRequests");
            DropTable("dbo.FinancialAudits");
            DropTable("dbo.FeeSchedules");
            DropTable("dbo.ServiceTypes");
        }
    }
}