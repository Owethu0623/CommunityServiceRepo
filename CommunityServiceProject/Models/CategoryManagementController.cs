using System.Linq;
using System.Web.Mvc;
using CommunityServiceProject.Models;

namespace CommunityServiceProject.Controllers
{
    public class CategoryManagementController : Controller
    {
        private Community db = new Community();

        // GET: CategoryManagement
        public ActionResult Index()
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var categories = db.Categories
                .OrderBy(c => c.CategoryName)
                .ToList();

            return View(categories);
        }


        // GET: CategoryManagement/Details/5
        public ActionResult Details(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var category = db.Categories
                .FirstOrDefault(c => c.CategoryID == id);

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);

        }

        // GET: CategoryManagement/Create
        public ActionResult Create()
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            return View();
        }

        // POST: CategoryManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category category)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (ModelState.IsValid)
            {
                string categoryName = category.CategoryName?.Trim();

                bool duplicateExists = db.Categories.Any(c =>
                    c.CategoryName.ToLower() == categoryName.ToLower()
                );

                if (duplicateExists)
                {
                    ModelState.AddModelError(
                        "CategoryName",
                        "A category with this name already exists."
                    );

                    return View(category);
                }

                category.CategoryName = categoryName;

                db.Categories.Add(category);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(category);
        }


        // GET: CategoryManagement/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var category = db.Categories
                .FirstOrDefault(c => c.CategoryID == id);

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }


        // POST: CategoryManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category category)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            if (ModelState.IsValid)
            {
                string categoryName = category.CategoryName?.Trim();

                bool duplicateExists = db.Categories.Any(c =>
                    c.CategoryID != category.CategoryID &&
                    c.CategoryName.ToLower() == categoryName.ToLower()
                );

                if (duplicateExists)
                {
                    ModelState.AddModelError(
                        "CategoryName",
                        "A category with this name already exists."
                    );

                    return View(category);
                }

                category.CategoryName = categoryName;

                db.Entry(category).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction(
                    "Details",
                    new { id = category.CategoryID }
                );
            }

            return View(category);
        }

        // POST: CategoryManagement/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (Session["AdministratorID"] == null)
            {
                return RedirectToAction("Login", "Administrators");
            }

            var category = db.Categories.Find(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            // Do not delete a category that is being used by requests
            if (category.Requests.Any())
            {
                TempData["DeleteError"] = "This category cannot be deleted because it is being used by existing requests.";
                return RedirectToAction("Index");
            }

            db.Categories.Remove(category);
            db.SaveChanges();

            return RedirectToAction("Index");
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}