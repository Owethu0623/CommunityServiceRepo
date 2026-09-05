using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianAssignmentViewModel
    {
        public int RequestID { get; set; }

        public string Title { get; set; }

        public string CategoryName { get; set; }

        public string WardName { get; set; }

        public string ProblemLocation { get; set; }

        public CommunityServiceProject.Models.Priority Priority { get; set; }

        public string PriorityReason { get; set; }

        [Required(ErrorMessage = "Please select a technician.")]
        [Display(Name = "Technician")]
        public int TechnicianID { get; set; }

        public IEnumerable<SelectListItem> Technicians { get; set; }


        // ===========================================================
        // REQUIRED SKILLS
        // ===========================================================

        [Display(Name = "Required Skills")]
        public IEnumerable<int> SelectedSkillIDs { get; set; }

        public IEnumerable<SelectListItem> Skills { get; set; }
    }
}
