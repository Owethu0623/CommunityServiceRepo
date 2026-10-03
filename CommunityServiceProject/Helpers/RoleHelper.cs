using System.Web;

namespace CommunityServiceProject.Helpers
{
    public static class RoleHelper
    {
        public static bool IsCitizen()
        {
            return HttpContext.Current?.Session["CitizenID"] != null;
        }

        public static bool IsAdministrator()
        {
            return HttpContext.Current?.Session["AdministratorID"] != null;
        }

        public static bool IsTechnician()
        {
            return HttpContext.Current?.Session["TechnicianID"] != null;
        }

        public static bool IsFinanceOfficer()
        {
            return HttpContext.Current?.Session["FinanceOfficerID"] != null;
        }

        public static bool IsHROfficer()
        {
            return HttpContext.Current?.Session["HROfficerID"] != null;
        }
    }
}
