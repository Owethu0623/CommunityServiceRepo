using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class PayrollDeduction
    {
        [Key]
        public int PayrollDeductionID { get; set; }

        [Required]
        public int PayrollID { get; set; }

        [Required]
        [StringLength(100)]
        public string DeductionType { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal Amount { get; set; }

        public virtual Payroll Payroll { get; set; }
    }
}