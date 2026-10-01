using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.ViewModels
{
    public class TechnicianSkillManagementViewModel
    {
        public int TechnicianID { get; set; }

        public string TechnicianName { get; set; }

        public string MunicipalEmail { get; set; }

        public string PhoneNumber { get; set; }

        public string AccountStatus { get; set; }

        public string CurrentSkillName { get; set; }

        public int? CurrentSkillID { get; set; }

        [Required(ErrorMessage = "Please select a primary skill.")]
        [Display(Name = "Primary Skill")]
        public int SelectedSkillID { get; set; }

        public List<SkillOptionViewModel> AvailableSkills { get; set; }

        public TechnicianSkillManagementViewModel()
        {
            AvailableSkills = new List<SkillOptionViewModel>();
        }
    }

    public class SkillOptionViewModel
    {
        public int SkillID { get; set; }

        public string SkillName { get; set; }

        public string Description { get; set; }
    }
}