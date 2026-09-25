using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CommunityServiceProject.ViewModels
{
    public class MunicipalProjectScopeViewModel
    {
        public int ProjectID { get; set; }

        [Display(Name = "Project Code")]
        public string ProjectCode { get; set; }

        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }

        [Display(Name = "Project Type")]
        public string ProjectType { get; set; }

        [Required(ErrorMessage = "Please define the project scope.")]
        [StringLength(
            4000,
            MinimumLength = 20,
            ErrorMessage = "Project scope must be between 20 and 4000 characters."
        )]
        [Display(Name = "Project Scope")]
        public string ProjectScope { get; set; }
    }
}