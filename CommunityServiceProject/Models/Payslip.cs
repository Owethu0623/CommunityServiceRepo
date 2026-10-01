using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class Payslip
    {
        [Key]
        public int PayslipID { get; set; }

        [Required]
        [StringLength(40)]
        public string PayslipNumber { get; set; }

        [Required]
        [Index("IX_Payslip_PayrollID", IsUnique = true)]
        public int PayrollID { get; set; }

        [Required]
        public int TechnicianID { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal GrossPay { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal TotalDeductions { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal NetPay { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        public virtual Payroll Payroll { get; set; }

        public virtual Technician Technician { get; set; }
    }
}