using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.Models
{
    public class PayrollPeriod
    {
        [Key]
        public int PayrollPeriodID { get; set; }

        [Required]
        [StringLength(20)]
        public string PeriodName { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public PayrollStatus Status { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        public virtual ICollection<Payroll> Payrolls { get; set; }

        public PayrollPeriod()
        {
            Payrolls = new HashSet<Payroll>();
        }
    }
}