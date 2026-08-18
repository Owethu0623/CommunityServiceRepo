using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace CommunityServiceProject.Models
{
    public class CitizensController : Controller
    {
        private Community db = new Community();


        // GET: Citizens
        public ActionResult Index()
        {
            return View(db.Citizens.ToList());
        }


        // GET: Citizens/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest
                );
            }

            Citizen citizen = db.Citizens.Find(id);

            if (citizen == null)
            {
                return HttpNotFound();
            }

            return View(citizen);
        }


        // GET: Citizens/Create
        public ActionResult Create()
        {
            return View();
        }


        // POST: Citizens/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "FirstName,LastName,EmailAddress,PhoneNumber,Password,ConfirmPassword,ResidentialAddress")]
            Citizen citizen)
        {
            if (ModelState.IsValid)
            {
                citizen.DateRegistered = DateTime.Now;
                citizen.AccountStatus = AccountStatus.Active;

                db.Citizens.Add(citizen);
                db.SaveChanges();

                // After successful registration, return the citizen to Login
                return RedirectToAction("Index", "Login");
            }

            return View(citizen);
        }


        // GET: Citizens/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest
                );
            }

            Citizen citizen = db.Citizens.Find(id);

            if (citizen == null)
            {
                return HttpNotFound();
            }

            return View(citizen);
        }


        // POST: Citizens/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "CitizenID,FirstName,LastName,EmailAddress,PhoneNumber,Password,ResidentialAddress,DateRegistered,AccountStatus")]
            Citizen citizen)
        {
            if (ModelState.IsValid)
            {
                db.Entry(citizen).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(citizen);
        }


        // GET: Citizens/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest
                );
            }

            Citizen citizen = db.Citizens.Find(id);

            if (citizen == null)
            {
                return HttpNotFound();
            }

            return View(citizen);
        }


        // POST: Citizens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Citizen citizen = db.Citizens.Find(id);

            db.Citizens.Remove(citizen);
            db.SaveChanges();

            return RedirectToAction("Index", "Login");
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