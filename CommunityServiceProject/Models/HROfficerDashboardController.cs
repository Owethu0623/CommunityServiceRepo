using System.Web.Mvc;
using CommunityServiceProject.Filters;

namespace CommunityServiceProject.Models
{
    public class HROfficerDashboardController : Controller
    {
        // GET: HROfficerDashboard
        [RoleAuthorize("HROfficer")]
        public ActionResult Index()
        {
            return View();
        }
    }
}
