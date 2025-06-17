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

        //---------CONSULTANT MANAGEMENT---------

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

        //---------CLIENT MANAGEMENT---------

        public ActionResult ManageClients(string searchTerm, string sortBy)
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1) return RedirectToAction("Login", "Login");

            var clients = db.ClientEntity.Include(c => c.User).Include(c => c.Status);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                clients = clients.Where(c => c.FullName.Contains(searchTerm) || c.User.Email.Contains(searchTerm));
            }

            ViewBag.SortByName = string.IsNullOrEmpty(sortBy) ? "name_desc" : "";
            ViewBag.SortByDate = sortBy == "date" ? "date_desc" : "date";

            switch (sortBy)
            {
                case "name_desc":
                    clients = clients.OrderByDescending(c => c.FullName);
                    break;
                case "date":
                    clients = clients.OrderBy(c => c.User.CreatedAt);
                    break;
                case "date_desc":
                    clients = clients.OrderByDescending(c => c.User.CreatedAt);
                    break;
                default:
                    clients = clients.OrderBy(c => c.FullName);
                    break;
            }
            ViewBag.CurrentSearch = searchTerm;

            return View(clients.ToList());
        }

        public PartialViewResult SearchClients(string searchTerm, string sortBy)
        {
            var clients = db.ClientEntity.Include(c => c.User).Include(c => c.Status);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                clients = clients.Where(c => c.FullName.Contains(searchTerm) || c.User.Email.Contains(searchTerm));
            }

            switch (sortBy)
            {
                case "name_desc":
                    clients = clients.OrderByDescending(c => c.FullName);
                    break;
                case "date":
                    clients = clients.OrderBy(c => c.User.CreatedAt);
                    break;
                case "date_desc":
                    clients = clients.OrderByDescending(c => c.User.CreatedAt);
                    break;
                default:
                    clients = clients.OrderBy(c => c.FullName);
                    break;
            }

            return PartialView("_ClientListTable", clients.ToList());
        }

        // GET: HR/ClientDetails/{id}
        public ActionResult ClientDetails(int? id)
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1) return RedirectToAction("Login", "Login");
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Client client = db.ClientEntity
                .Include(c => c.User)
                .Include(c => c.Status)
                .Include(c => c.AssignedConsultant.User) 
                .FirstOrDefault(c => c.UserID == id);

            if (client == null) return HttpNotFound();

            if (client.Status.StatusName == "Accepted")
            {
                ViewBag.ConsultantList = new SelectList(db.ConsultantEntity.ToList(), "UserID", "Name", client.AssignedConsultantID);
            }

            return View(client);
        }

        // POST: HR/ClientDetails/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClientDetails(Client client, string newPassword)
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1) return RedirectToAction("Login", "Login");

            if (ModelState.IsValid)
            {
                var clientInDb = db.ClientEntity.Find(client.UserID);
                if (clientInDb == null) return HttpNotFound();
                var userInDb = db.UsersEntity.Find(client.UserID);
                if (userInDb == null) return HttpNotFound();

                clientInDb.FullName = client.FullName;
                clientInDb.Phone = client.Phone;
                clientInDb.CaseDetails = client.CaseDetails;
                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    // Change with BCrpyt Later
                    userInDb.PasswordHash = newPassword;
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Client details updated successfully!";
                return RedirectToAction("ClientDetails", new { id = client.UserID });
            }

            client.User = db.UsersEntity.Find(client.UserID);
            client.Status = db.StatusTypeEntity.Find(db.ClientEntity.AsNoTracking().FirstOrDefault(c => c.UserID == client.UserID).StatusID);
            if (client.Status.StatusName == "Accepted")
            {
                ViewBag.ConsultantList = new SelectList(db.ConsultantEntity.ToList(), "UserID", "Name", client.AssignedConsultantID);
            }
            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeClientStatus(int id, string newStatus)
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1) return RedirectToAction("Login", "Login");

            var client = db.ClientEntity.Find(id);
            var status = db.StatusTypeEntity.FirstOrDefault(s => s.StatusName == newStatus);

            if (client != null && status != null)
            {
                client.StatusID = status.StatusID;
                db.SaveChanges();
                TempData["SuccessMessage"] = $"Client status changed to {newStatus}.";
            }

            return RedirectToAction("ClientDetails", new { id = id });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignConsultant(int clientId, int? consultantId)
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1) return RedirectToAction("Login", "Login");

            var client = db.ClientEntity.Find(clientId);
            if (client != null && consultantId.HasValue)
            {
                client.AssignedConsultantID = consultantId.Value;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Consultant assigned successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Please select a consultant to assign.";
            }
            return RedirectToAction("ClientDetails", new { id = clientId });
        }
    }
}