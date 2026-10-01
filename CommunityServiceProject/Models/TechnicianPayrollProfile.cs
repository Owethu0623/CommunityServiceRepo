using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityServiceProject.Models
{
    public class TechnicianPayrollProfile
    {
        [Key]
        public int TechnicianPayrollProfileID { get; set; }

        [Required]
        [Index("IX_TechnicianPayrollProfile_TechnicianID", IsUnique = true)]
        public int TechnicianID { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal BasicSalary { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public virtual Technician Technician { get; set; }
    }
}