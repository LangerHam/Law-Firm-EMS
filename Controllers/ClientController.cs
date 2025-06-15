using Law_Firm_EMS.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Law_Firm_EMS.Models;
using Law_Firm_EMS.ViewModels;

namespace Law_Firm_EMS.Controllers
{
    public class ClientController : Controller
    {
        private LawContextDb db;
        public ClientController()
        {
            this.db = new LawContextDb();
        }
        // GET: Client
        public ActionResult Dashboard()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Login");

            int userId = (int)Session["UserID"];

            var client = db.ClientEntity
                .Include(c => c.AssignedConsultant)
                .Include(c => c.Billing)
                .Include(c => c.Documents.Select(d => d.DocumentType))
                .Include(c => c.Documents.Select(d => d.Status))
                .Include(c => c.Forms.Select(f => f.FormType))
                .Include(c => c.Forms.Select(f => f.Status))
                .FirstOrDefault(c => c.UserID == userId);

            if (client == null)
                return Content("You are logged in, but no client profile found. Please contact admin.");

            var viewModel = new ClientDashboardViewModel
            {
                ClientName = client.FullName,
                ConsultantName = client.AssignedConsultant?.Name ?? "Not Assigned",
                TotalFees = client.Billing?.TotalFees ?? 0,
                Paid = client.Billing?.PaidAmount ?? 0,
                Due = (client.Billing?.TotalFees ?? 0) - (client.Billing?.PaidAmount ?? 0),
                LORs = client.Documents?
                    .Where(d => d.DocumentType?.TypeName != null && d.DocumentType.TypeName.Contains("LOR"))
                    .Select(d => new LORStatusVM
                    {
                        UploadPath = d.UploadPath,
                        StatusName = d.Status?.StatusName ?? "N/A"
                    }).ToList() ?? new List<LORStatusVM>(),
                EIAStatus = client.Forms?
                    .FirstOrDefault(f => f.FormType?.TypeName != null && f.FormType.TypeName.Contains("EIA"))?.Status?.StatusName ?? "N/A",
                PESPStatus = client.Forms?
                    .FirstOrDefault(f => f.FormType?.TypeName != null && f.FormType.TypeName.Contains("PESP"))?.Status?.StatusName ?? "N/A",
                StatusSummary = client.Documents?
                    .GroupBy(d => d.Status?.StatusName ?? "Unknown")
                    .Select(g => new StatusSummaryVM
                    {
                        Status = g.Key,
                        Count = g.Count()
                    }).ToList() ?? new List<StatusSummaryVM>()
            };

            return View(viewModel);
        }
    }
}