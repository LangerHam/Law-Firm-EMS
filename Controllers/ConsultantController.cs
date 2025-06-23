using Law_Firm_EMS.Context;
using Law_Firm_EMS.ViewModels.Consultant;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using static Law_Firm_EMS.ViewModels.Consultant.ConsultantDashboardViewModel;
using Law_Firm_EMS.Models;
using System.Net;

namespace Law_Firm_EMS.Controllers
{
    public class ConsultantController : Controller
    {
        private LawContextDb db;

        public ConsultantController()
        {
            db = new LawContextDb();
        }

        // GET: Consultant
        public ActionResult Dashboard()
        {
            if (Session["UserID"] == null || (int)Session["RoleID"] != 3) 
                return RedirectToAction("Login", "Login");

            int consultantId = (int)Session["UserID"];

            var pendingStatusId = db.StatusTypeEntity.FirstOrDefault(s => s.StatusName == "Pending")?.StatusID;

            int pendingTasksCount = 0;
            if (pendingStatusId.HasValue)
            {
                pendingTasksCount = db.TasksEntity
                    .Count(t => t.AssignedToConsultantID == consultantId && t.StatusID == pendingStatusId.Value);
            }

            int assignedClientsCount = db.ClientEntity.Count(c => c.AssignedConsultantID == consultantId);

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var recentEvaluations = db.EvaluationEntity
                .Where(e => e.ConsultantID == consultantId && e.DateGiven >= thirtyDaysAgo)
                .OrderByDescending(e => e.DateGiven)
                .ToList();

            int paidLeaveAllowance = 15;
            int sickLeaveAllowance = 15;
            int unpaidLeaveAllowance = 20;
            var currentYear = DateTime.UtcNow.Year; 

            var acceptedLeavesThisYear = db.LeaveEntity
                .Where(l => l.RequestedByConsultantID == consultantId &&
                            l.Status.StatusName == "Accepted" &&
                            l.FromDate.Year == currentYear)
                .ToList();

            int paidLeaveTaken = acceptedLeavesThisYear.Where(l => l.Type == "Paid Leave").Sum(l => (l.ToDate - l.FromDate).Days + 1);
            int sickLeaveTaken = acceptedLeavesThisYear.Where(l => l.Type == "Sick Leave").Sum(l => (l.ToDate - l.FromDate).Days + 1);
            int unpaidLeaveTaken = acceptedLeavesThisYear.Where(l => l.Type == "Unpaid Leave").Sum(l => (l.ToDate - l.FromDate).Days + 1);

            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var leaveDataForChart = db.LeaveEntity
                .Where(l => l.RequestedByConsultantID == consultantId && l.Status.StatusName == "Accepted" && l.FromDate > sixMonthsAgo)
                .GroupBy(l => new { l.FromDate.Year, l.FromDate.Month })
                .Select(g => new {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    TotalDays = g.Sum(l => DbFunctions.DiffDays(l.FromDate, l.ToDate) + 1)
                })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToList();

            var chartLabels = string.Join(", ", leaveDataForChart.Select(d => "'" + new DateTime(d.Year, d.Month, 1).ToString("MMM") + "'"));
            var chartData = string.Join(", ", leaveDataForChart.Select(d => d.TotalDays));

            var viewModel = new ConsultantDashboardViewModel
            {
                PendingTasksCount = pendingTasksCount,
                AssignedClientsCount = assignedClientsCount,
                RecentEvaluations = recentEvaluations,
                PaidLeaveRemaining = paidLeaveAllowance - paidLeaveTaken,
                SickLeaveRemaining = sickLeaveAllowance - sickLeaveTaken,
                UnpaidLeaveRemaining = unpaidLeaveAllowance - unpaidLeaveTaken,
                TotalPaidLeaveAllowance = paidLeaveAllowance,
                TotalSickLeaveAllowance = sickLeaveAllowance,
                TotalUnpaidLeaveAllowance = unpaidLeaveAllowance,
                LeaveChartLabels = chartLabels,
                LeaveChartData = chartData
            };

            return View(viewModel);
        }

        public ActionResult MyClients()
        {
            if (Session["UserID"] == null || (int)Session["RoleID"] != 3)
                return RedirectToAction("Login", "Login");

            int consultantId = (int)Session["UserID"];

            var clients = db.ClientEntity
                .Where(c => c.AssignedConsultantID == consultantId)
                .Include(c => c.User)
                .Include(c => c.Status)
                .Select(c => new ConsultantClientListViewModel
                {
                    UserID = c.UserID,
                    FullName = c.FullName,
                    Email = c.User.Email,
                    Status = c.Status.StatusName
                })
                .OrderBy(c => c.FullName)
                .ToList();

            return View(clients);
        }

        // GET: Consultant/ClientDetails/{id}
        public ActionResult ClientDetails(int? id)
        {
            if (Session["UserID"] == null || (int)Session["RoleID"] != 3)
                return RedirectToAction("Login", "Login");

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            int consultantId = (int)Session["UserID"];

            var client = db.ClientEntity
                .Include(c => c.User)
                .Include(c => c.Status)
                .FirstOrDefault(c => c.UserID == id && c.AssignedConsultantID == consultantId);

            if (client == null) return HttpNotFound("Client not found or not assigned to you.");

            var allClientDocuments = db.DocumentEntity
                .Where(d => d.ClientID == id)
                .Include(d => d.DocumentType)
                .Include(d => d.Status)
                .OrderByDescending(d => d.DocumentID) 
                .ToList();

            var documentGroups = new DocumentGroupViewModel
            {
                LoR_Documents = allClientDocuments.Where(d => d.DocumentType.TypeName == "LoR").ToList(),
                EIA_PES_Documents = allClientDocuments.Where(d => d.DocumentType.TypeName == "EIA" || d.DocumentType.TypeName == "PES").ToList(),
                Other_Documents = allClientDocuments.Where(d => d.DocumentType.TypeName != "LoR" && d.DocumentType.TypeName != "EIA" && d.DocumentType.TypeName != "PES").ToList()
            };

            var viewModel = new ConsultantClientDetailViewModel
            {
                ClientProfile = client,
                Documents = documentGroups
            };

            return View(viewModel);
        }

        // GET: Consultant/Evaluations
        public ActionResult Evaluations()
        {
           
            if (Session["UserID"] == null || (int)Session["RoleID"] != 3)
            {
                return RedirectToAction("Login", "Login");
            }

            int consultantId = (int)Session["UserID"];

            var evaluations = db.EvaluationEntity
                                .Include(e => e.EvaluatedByHR) 
                                .Where(e => e.ConsultantID == consultantId)
                                .OrderByDescending(e => e.DateGiven)
                                .ToList();

            var viewModel = new ConsultantEvaluationsViewModel
            {
                Evaluations= evaluations 
            };

            return View(viewModel);
        }

        public ActionResult DeptList()
        {
            {
                var contacts = db.ConsultantEntity
                    .Join(db.UsersEntity,
                    consultant => consultant.UserID,
                    user => user.UserID,
                    (consultant, user) => new ConsultantContactViewModel
                    {
                        Name = consultant.Name,
                        Phone = consultant.Phone,
                        Email = user.Email
                    })
                .ToList();


                return View(contacts);
            }
        }

    }
}