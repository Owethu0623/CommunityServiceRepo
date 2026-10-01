using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class Payroll
    {
        [Key]
        public int PayrollID { get; set; }

        [Required]
        public int PayrollPeriodID { get; set; }

        [Required]
        public int TechnicianID { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal BasicSalary { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal OvertimeAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal TotalAllowances { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal TotalDeductions { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal GrossPay { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal NetPay { get; set; }

        [Required]
        public PayrollStatus Status { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        public DateTime? CalculatedDate { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public DateTime? PaidDate { get; set; }

        public int? ApprovedByFinanceOfficerID { get; set; }

        public virtual PayrollPeriod PayrollPeriod { get; set; }

        public virtual Technician Technician { get; set; }

        public virtual FinanceOfficer ApprovedByFinanceOfficer { get; set; }

        public virtual ICollection<PayrollAllowance> Allowances { get; set; }

        public virtual ICollection<PayrollDeduction> Deductions { get; set; }

        public virtual ICollection<OvertimeClaim> OvertimeClaims { get; set; }


 

        public Payroll()
        {
            Allowances = new HashSet<PayrollAllowance>();
            Deductions = new HashSet<PayrollDeduction>();
            OvertimeClaims = new HashSet<OvertimeClaim>();
        }
    }
}