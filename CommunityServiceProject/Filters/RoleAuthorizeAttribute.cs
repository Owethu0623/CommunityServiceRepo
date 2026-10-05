using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CommunityServiceProject.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RoleAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public RoleAuthorizeAttribute(string roles)
        {
            _roles = roles?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim()).ToArray() ?? new string[0];
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Respect AllowAnonymous attribute on actions or controllers
            if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true) ||
                filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true))
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            var session = filterContext.HttpContext.Session;

            bool authorized = false;

            foreach (var role in _roles)
            {
                switch (role)
                {
                    case "Citizen":
                        if (session["CitizenID"] != null) authorized = true;
                        break;
                    case "Administrator":
                        if (session["AdministratorID"] != null) authorized = true;
                        break;
                    case "Technician":
                        if (session["TechnicianID"] != null) authorized = true;
                        break;
                    case "FinanceOfficer":
                        if (session["FinanceOfficerID"] != null) authorized = true;
                        break;
                    case "HROfficer":
                        if (session["HROfficerID"] != null) authorized = true;
                        break;
                }

                if (authorized) break;
            }

            if (!authorized)
            {
                // Not authorized - redirect to appropriate login by priority
                // If any role present in session, but not matching required, return 401
                if (session["CitizenID"] != null || session["AdministratorID"] != null || session["TechnicianID"] != null || session["FinanceOfficerID"] != null || session["HROfficerID"] != null)
                {
                    filterContext.Result = new HttpUnauthorizedResult();
                }
                else
                {
                    // Redirect to a generic Login page
                    filterContext.Result = new RedirectToRouteResult(
                        new System.Web.Routing.RouteValueDictionary(
                            new { controller = "Login", action = "Index" }
                        )
                    );
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
