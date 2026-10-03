using System.Web.Mvc;
using CommunityServiceProject.Filters;

namespace CommunityServiceProject.Controllers
{
    [RoleAuthorize("Administrator")]
    [RoleAuthorize("Administrator")]
    public class AdministratorDashboardController : Controller
    {
        // GET: AdministratorDashboard
        public ActionResult Index()
        {
            // Make sure an administrator is logged in
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            return View();
        }
    }
}