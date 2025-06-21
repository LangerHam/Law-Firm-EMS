using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Law_Firm_EMS.Context;
using Law_Firm_EMS.Models;

namespace Law_Firm_EMS.Controllers
{
    public class LoginController : Controller
    {
        private LawContextDb db;
        public LoginController()
        {
            this.db = new LawContextDb();
        }
        // GET: Login
        public ActionResult LandingPage()
        {
            db.SaveChanges();
            return View();
        }

        [HttpPost]
        public ActionResult RequestConsultation(string name, string email, string caseDetails)
        {
            try
            {
                using (var db = new LawContextDb())
                {
                    var newUser = new Users
                    {
                        Email = email,
                        PasswordHash = "12345",
                        RoleID = 2, 
                        CreatedAt = System.DateTime.UtcNow
                    };

                    var newClient = new Client
                    {
                        FullName = name,
                        CaseDetails = caseDetails,
                        StatusID = 1, 
                        User = newUser
                    };
                    db.ClientEntity.Add(newClient);

                    db.SaveChanges();
                }

                return RedirectToAction("LandingPage");
            }
            catch (System.Exception ex)
            {
                return RedirectToAction("LandingPage");
            }
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Users model)
        {
            var user = db.UsersEntity.FirstOrDefault(u => u.Email == model.Email && u.PasswordHash == model.PasswordHash);

            if (user != null)
            {

                Session["UserID"] = user.UserID;
                Session["Email"] = user.Email;
                Session["RoleID"] = user.RoleID;

                switch (user.RoleID)
                {
                    case 1: return RedirectToAction("Dashboard", "HR");
                    case 2: return RedirectToAction("Dashboard", "Client");
                    case 3:
                        var consultantProfile = db.ConsultantEntity.Find(user.UserID);
                        Session["ProfilePhotoPath"] = consultantProfile?.ProfilePhotoPath;
                        return RedirectToAction("Dashboard", "Consultant");
                    default: return RedirectToAction("Login");
                }
            }

            ViewBag.Error = "Invalid email or password";
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();  
            Session.Abandon(); 
            return RedirectToAction("LandingPage");
        }

    }
}