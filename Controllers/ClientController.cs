using Law_Firm_EMS.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Law_Firm_EMS.Models;
using Law_Firm_EMS.ViewModels;
using System.IO;

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

            if (Session["RoleID"] == null || (int)Session["RoleID"] != 2) return RedirectToAction("Login", "Login");

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

            ViewBag.ClientName = client.FullName;
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

        public ActionResult Forms()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Login");

            int userId = (int)Session["UserID"];
            var client = db.ClientEntity                
                .FirstOrDefault(c => c.UserID == userId);

            ViewBag.ClientName = client.FullName;

            var requiredFormTypes = new List<FormDisplayItem>
        {
            new FormDisplayItem { FormTypeID = 1, FormTypeName = "G-28", FormTypeDescription = "Notice of Entry of Appearance as Attorney or Accredited Representative." },
            new FormDisplayItem { FormTypeID = 2, FormTypeName = "G-1145", FormTypeDescription = "E-Notification of Application/Petition Acceptance." },
            new FormDisplayItem { FormTypeID = 3, FormTypeName = "I-140", FormTypeDescription = "Immigrant Petition for Alien Workers." },
            new FormDisplayItem { FormTypeID = 4, FormTypeName = "I-907", FormTypeDescription = "Request for Premium Processing Service. This is optional.", IsOptional = true },
            new FormDisplayItem { FormTypeID = 5, FormTypeName = "ETA 750 Part B", FormTypeDescription = "Application for Alien Employment Certification." }
        };

            var clientSubmittedForms = db.FormEntity
                .Where(f => f.ClientID == userId)
                .Include(f => f.Status)
                .ToList();

            var allDbFormTypes = db.FormTypeEntity.ToDictionary(ft => ft.FormTypeID);

            foreach (var requiredForm in requiredFormTypes)
            {
                var submittedForm = clientSubmittedForms.FirstOrDefault(f => f.FormTypeID == requiredForm.FormTypeID);
                if (submittedForm != null)
                {
                    requiredForm.Status = submittedForm.Status?.StatusName ?? "Submitted";
                    requiredForm.UploadPath = submittedForm.UploadPath;
                }
                else
                {
                    requiredForm.Status = "Not Submitted";
                }

                if (allDbFormTypes.ContainsKey(requiredForm.FormTypeID))
                {
                    requiredForm.TemplatePath = allDbFormTypes[requiredForm.FormTypeID].TemplatePath;
                }
            }

            var viewModel = new ClientFormsViewModel { AvailableForms = requiredFormTypes };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadForm(int formTypeId, HttpPostedFileBase file)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Login");
            if (file == null || file.ContentLength == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to upload.";
                return RedirectToAction("Forms");
            }

            int userId = (int)Session["UserID"];

            var form = db.FormEntity.FirstOrDefault(f => f.ClientID == userId && f.FormTypeID == formTypeId);

            if (form == null)
            {
                form = new Form
                {
                    ClientID = userId,
                    FormTypeID = formTypeId,
                    StatusID = db.StatusTypeEntity.FirstOrDefault(s => s.StatusName == "Submitted").StatusID
                };
                db.FormEntity.Add(form);
            }

            string uploadDir = Server.MapPath("~/Resources/UploadedClientForms/");
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string physicalPath = Path.Combine(uploadDir, uniqueFileName);
            file.SaveAs(physicalPath);

            form.UploadPath = "/Resources/UploadedClientForms/" + uniqueFileName;
            form.StatusID = db.StatusTypeEntity.FirstOrDefault(s => s.StatusName == "Submitted").StatusID;
            db.SaveChanges();

            TempData["SuccessMessage"] = "Form uploaded successfully!";
            return RedirectToAction("Forms");
        }

        public ActionResult DownloadTemplate(int formTypeId)
        {
            var formType = db.FormTypeEntity.Find(formTypeId);
            if (formType == null || string.IsNullOrEmpty(formType.TemplatePath))
            {
                return HttpNotFound();
            }

            string physicalPath = Server.MapPath(formType.TemplatePath);
            if (!System.IO.File.Exists(physicalPath))
            {
                return HttpNotFound();
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(physicalPath);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(physicalPath));
        }
    }
}