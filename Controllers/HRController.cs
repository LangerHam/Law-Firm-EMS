using Law_Firm_EMS.Context;
using Law_Firm_EMS.Models;
using Law_Firm_EMS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Law_Firm_EMS.Controllers
{
    public class HRController : Controller
    {
        private LawContextDb db;
        public HRController()
        {
            db = new LawContextDb();
        }

        public ActionResult Dashboard()
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1)
            {
                return RedirectToAction("Login", "Login");
            }

            var pendingStatus = db.StatusTypeEntity.FirstOrDefault(s => s.StatusName.ToLower() == "pending");

            var viewModel = new HRDashboardViewModel();

            if (pendingStatus != null)
            {
                int pendingStatusId = pendingStatus.StatusID;

                viewModel.NewClientRequests = db.ClientEntity
                    .Where(c => c.StatusID == pendingStatusId)
                    .Include(c => c.User)
                    .OrderByDescending(c => c.User.CreatedAt)
                    .ToList();

                viewModel.PendingLeaveRequests = db.LeaveEntity
                    .Where(l => l.StatusID == pendingStatusId)
                    .Include(l => l.RequestedByConsultant.User)
                    .OrderBy(l => l.FromDate)
                    .ToList();
            }

            return View(viewModel);
        }

        public ActionResult ManageConsultants()
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1) return RedirectToAction("Login", "Login");

            var consultants = db.ConsultantEntity.Include(c => c.User);
            return View(consultants.ToList());
        }

        public ActionResult CreateConsultant()
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1) return RedirectToAction("Login", "Login");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateConsultant(ConsultantViewModel viewModel)
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1) return RedirectToAction("Login", "Login");

            if (ModelState.IsValid)
            {
                if (db.UsersEntity.Any(u => u.Email == viewModel.Email))
                {
                    ModelState.AddModelError("Email", "An account with this email already exists.");
                    return View(viewModel);
                }

                // Replace with BCrypt
                string hashedPassword = viewModel.Password;

                var user = new Users
                {
                    Email = viewModel.Email,
                    PasswordHash = hashedPassword,
                    RoleID = 3,
                    CreatedAt = System.DateTime.UtcNow
                };

                var consultant = new Consultant
                {
                    Name = viewModel.Name,
                    Phone = viewModel.Phone,
                    //HRID = (int)Session["UserID"],
                    User = user
                };

                db.ConsultantEntity.Add(consultant);
                db.SaveChanges();
                return RedirectToAction("ManageConsultants");
            }
            return View(viewModel);
        }

        public ActionResult ConsultantDetails(int? id)
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1) return RedirectToAction("Login", "Login");
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Consultant consultant = db.ConsultantEntity.Include(c => c.User).FirstOrDefault(c => c.UserID == id);
            if (consultant == null) return HttpNotFound();

            return View(consultant);
        }

        // POST: HR/ConsultantDetails/{id} 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConsultantDetails(Consultant consultant)
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1) return RedirectToAction("Login", "Login");

            if (ModelState.IsValid)
            {
                var consultantInDb = db.ConsultantEntity.Find(consultant.UserID);
                if (consultantInDb == null)
                {
                    return HttpNotFound();
                }
                consultantInDb.Name = consultant.Name;
                consultantInDb.Phone = consultant.Phone;
                consultantInDb.ProfilePhotoPath = consultant.ProfilePhotoPath;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Consultant details updated successfully.";
                return RedirectToAction("ConsultantDetails", new { id = consultant.UserID });
            }
            consultant.User = db.UsersEntity.Find(consultant.UserID);
            return View(consultant);
        }

        // POST: HR/DeleteConsultant/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConsultant(int id)
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1) return RedirectToAction("Login", "Login");

            Consultant consultant = db.ConsultantEntity.Include(c => c.User).FirstOrDefault(c => c.UserID == id);
            if (consultant != null)
            {
                Users user = consultant.User;
                db.ConsultantEntity.Remove(consultant);
                db.UsersEntity.Remove(user);
                db.SaveChanges();
            }
            return RedirectToAction("ManageConsultants");
        }
    }
}