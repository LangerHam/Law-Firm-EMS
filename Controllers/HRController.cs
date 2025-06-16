using Law_Firm_EMS.Context;
using Law_Firm_EMS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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
    }
}