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
                        PasswordHash = "temporary_password",
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
                return RedirectToAction("LandingPagee");
            }
        }
    }
}