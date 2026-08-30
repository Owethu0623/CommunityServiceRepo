using System.Web.Mvc;

namespace CommunityServiceProject.Controllers
{
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