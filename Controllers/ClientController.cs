﻿using Law_Firm_EMS.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Law_Firm_EMS.Models;
using Law_Firm_EMS.ViewModels;
using System.IO;
using System.Data.Entity.Validation;

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
                    .Where(d => d.DocumentType?.TypeName == "LoR")
                    .Select(d => new LORStatusVM
                    {
                        DocumentID = d.DocumentID,
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
                        Count = g.Count(),
                        Color = GetStatusColor(g.Key)
                    }).ToList() ?? new List<StatusSummaryVM>()
            };

            return View(viewModel);
        }


        public string GetStatusColor(string statusName)
        {
            switch (statusName.ToLower())
            {
                case "pending":
                    return "#FECACA";
                case "completed":
                    return "#008000";
                case "reviewing":
                    return "#FFA500";
                case "submitted":
                    return "#0000FF";
                default:
                    return "#808080";
            }
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

        public ActionResult DownloadUploadedForm(int formTypeId)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Login");

            int userId = (int)Session["UserID"];

            var form = db.FormEntity
                .FirstOrDefault(f => f.ClientID == userId && f.FormTypeID == formTypeId);

            if (form == null || string.IsNullOrEmpty(form.UploadPath))
            {
                TempData["ErrorMessage"] = "Uploaded form not found.";
                return RedirectToAction("Forms");
            }

            string physicalPath = Server.MapPath(form.UploadPath);
            if (!System.IO.File.Exists(physicalPath))
            {
                TempData["ErrorMessage"] = "File not found on server.";
                return RedirectToAction("Forms");
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(physicalPath);
            string fileName = Path.GetFileName(physicalPath);

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public ActionResult Documents()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Login");

            int userId = (int)Session["UserID"];
            var client = db.ClientEntity
                .FirstOrDefault(c => c.UserID == userId);

            if (client == null)
            {
                TempData["ErrorMessage"] = "Client profile not found.";
                return RedirectToAction("Dashboard");
            }

            ViewBag.ClientName = client.FullName;

            var clientDocuments = db.DocumentEntity
                .Where(d => d.ClientID == userId)
                .Include(d => d.DocumentType)
                .Include(d => d.Status)
                .ToList();

            var lorDocuments = clientDocuments
                .Where(d => d.DocumentType?.TypeName == "LoR")
                .Select(d => new DocumentItemVM
                {
                    DocumentID = d.DocumentID,
                    FileName = Path.GetFileName(d.UploadPath),
                    UploadPath = d.UploadPath
                })
                .ToList();

            var miscDocuments = clientDocuments
                .Where(d => d.DocumentType?.TypeName == "MISC")
                .Select(d => new DocumentItemVM
                {
                    DocumentID = d.DocumentID,
                    FileName = Path.GetFileName(d.UploadPath),
                    UploadPath = d.UploadPath
                })
                .ToList();

            var viewModel = new ClientDocumentsViewModel
            {
                LORDocuments = lorDocuments,
                MiscDocuments = miscDocuments
            };

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadDocument(HttpPostedFileBase file, string documentCategory)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Login");

            int loggedInUserId = (int)Session["UserID"];
            var client = db.ClientEntity.FirstOrDefault(c => c.UserID == loggedInUserId);

            if (client == null)
            {
                TempData["ErrorMessage"] = "Client profile not found. Unable to upload document.";
                return RedirectToAction("Documents");
            }

            if (file == null || file.ContentLength == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to upload.";
                return RedirectToAction("Documents");
            }

            try
            {
                string uploadsFolder = Server.MapPath("~/Resources/ClientDocuments/");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string fileName = Path.GetFileName(file.FileName);
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                string dbFilePath = "~/Resources/ClientDocuments/" + uniqueFileName;

                file.SaveAs(filePath);

                int documentTypeID;
                if (documentCategory == "LoR")
                {
                    documentTypeID = db.DocumentTypeEntity.FirstOrDefault(dt => dt.TypeName == "LoR")?.DocumentTypeID ?? 0;
                    if (documentTypeID == 0)
                    {
                        TempData["ErrorMessage"] = "LoR document type not found in database. Please contact admin.";
                        return RedirectToAction("Documents");
                    }
                }
                else if (documentCategory == "MISC")
                {
                    documentTypeID = db.DocumentTypeEntity.FirstOrDefault(dt => dt.TypeName == "MISC")?.DocumentTypeID ?? 0;
                    if (documentTypeID == 0)
                    {
                        TempData["ErrorMessage"] = "Miscellaneous document type not found in database. Please contact admin.";
                        return RedirectToAction("Documents");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid document category selected.";
                    return RedirectToAction("Documents");
                }


                int pendingStatusID = db.StatusTypeEntity.FirstOrDefault(s => s.StatusName == "Pending")?.StatusID ?? 0;
                if (pendingStatusID == 0)
                {
                    TempData["ErrorMessage"] = "Default status 'Pending' not found. Please contact admin.";
                    return RedirectToAction("Documents");
                }


                Document newDocument = new Document
                {
                    UploadPath = dbFilePath,
                    ClientID = client.UserID,
                    UploadedByUserID = loggedInUserId,
                    DocumentTypeID = documentTypeID,
                    StatusID = pendingStatusID,
                    ParentDocumentID = null
                };
                db.DocumentEntity.Add(newDocument);



                db.SaveChanges();

                TempData["SuccessMessage"] = "Document uploaded successfully!";

                return RedirectToAction("Documents");
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.PropertyName + ": " + x.ErrorMessage);
                var fullErrorMessage = string.Join("; ", errorMessages);
                TempData["ErrorMessage"] = $"Validation errors uploading document: {fullErrorMessage}";
                System.Diagnostics.Debug.WriteLine($"Document Upload Validation Error (Main): {fullErrorMessage}");
                return RedirectToAction("Documents");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An unexpected error occurred uploading document: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Document Upload Error (Main): {ex.Message} - StackTrace: {ex.StackTrace}");
                return RedirectToAction("Documents");
            }
        }

        public ActionResult DownloadDocument(int id)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Login");

            int userId = (int)Session["UserID"];
            var client = db.ClientEntity.FirstOrDefault(c => c.UserID == userId);

            if (client == null)
            {
                TempData["ErrorMessage"] = "Client profile not found.";
                return RedirectToAction("Dashboard");
            }

            // Ensure client can only download their own documents
            var document = db.DocumentEntity
                .FirstOrDefault(d => d.DocumentID == id && d.ClientID == client.UserID);

            if (document == null || string.IsNullOrEmpty(document.UploadPath))
            {
                TempData["ErrorMessage"] = "Document not found or you do not have permission to download it.";
                return RedirectToAction("Documents");
            }

            string physicalPath = Server.MapPath(document.UploadPath);
            if (!System.IO.File.Exists(physicalPath))
            {
                TempData["ErrorMessage"] = "File not found on server.";
                return RedirectToAction("Documents");
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(physicalPath);
            string fileName = Path.GetFileName(physicalPath);

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public ActionResult Billing()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Login");

            if (Session["RoleID"] == null || (int)Session["RoleID"] != 2) return RedirectToAction("Login", "Login");

            int userId = (int)Session["UserID"];

            var client = db.ClientEntity
                .Include(c => c.AssignedConsultant)
                .Include(c => c.Billing.Transactions) 
                .FirstOrDefault(c => c.UserID == userId);

            ViewBag.ClientName = client.FullName;

            if (client == null)
            {
                TempData["ErrorMessage"] = "Client profile not found. Please contact admin.";
                return RedirectToAction("Dashboard"); /
            }

            var billingData = client.Billing;

            var viewModel = new ClientBillingViewModel
            {
                ClientName = client.FullName,
                ConsultantName = client.AssignedConsultant?.Name ?? "Not Assigned",
                BillingSummary = new BillingSummaryViewModel
                {
                    TotalFees = billingData?.TotalFees ?? 0M,
                    PaidAmount = billingData?.PaidAmount ?? 0M,
                    DueAmount = (billingData?.TotalFees ?? 0M) - (billingData?.PaidAmount ?? 0M) 
                },
                Transactions = billingData?.Transactions
                                .OrderByDescending(t => t.PaymentDate) 
                                .Select(t => new TransactionViewModel
                                {
                                    TransactionID = t.TransactionID,
                                    Amount = t.Amount,
                                    PaymentDate = t.PaymentDate
                                }).ToList() ?? new List<TransactionViewModel>()
            };

            return View(viewModel);
        }

        // POST: Client/ProcessPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessPayment(ProcessPaymentViewModel paymentModel)
        {
            if (Session["UserID"] == null)
                return Json(new { success = false, message = "Authentication required." }); 

            if (!ModelState.IsValid)
            {
               
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Invalid payment amount.", errors = errors });
            }

            int userId = (int)Session["UserID"];

            var billing = db.BillingEntity
                            .Include(b => b.Transactions)
                            .FirstOrDefault(b => b.ClientID == userId);

            if (billing == null)
            {
                
                billing = new Billing
                {
                    ClientID = userId,
                    TotalFees = 0M, 
                    PaidAmount = 0M,
                    Transactions = new List<Transaction>()
                };
                db.BillingEntity.Add(billing);
            }

            if (paymentModel.AmountToPay <= 0)
            {
                return Json(new { success = false, message = "Payment amount must be greater than zero." });
            }

            
            decimal currentDue = billing.TotalFees - billing.PaidAmount;
            if (paymentModel.AmountToPay > currentDue && currentDue > 0)
            {
                return Json(new { success = false, message = $"Payment amount (${paymentModel.AmountToPay:N2}) exceeds remaining due (${currentDue:N2}). Please adjust the amount." });
            }


            try
            {
                billing.PaidAmount += paymentModel.AmountToPay;

                var newTransaction = new Transaction
                {
                    Amount = paymentModel.AmountToPay,
                    PaymentDate = DateTime.Now,
                    BillingID = billing.ClientID 
                };
                db.TransactionEntity.Add(newTransaction);

                db.SaveChanges();

                
                var updatedSummary = new BillingSummaryViewModel
                {
                    TotalFees = billing.TotalFees,
                    PaidAmount = billing.PaidAmount,
                    DueAmount = billing.TotalFees - billing.PaidAmount
                };

                
                var updatedTransactions = billing.Transactions
                                            .OrderByDescending(t => t.PaymentDate)
                                            .Select(t => new TransactionViewModel
                                            {
                                                TransactionID = t.TransactionID,
                                                Amount = t.Amount,
                                                PaymentDate = t.PaymentDate
                                            }).ToList();

                return Json(new { success = true, message = "Payment processed successfully!", summary = updatedSummary, transactions = updatedTransactions });
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.PropertyName + ": " + x.ErrorMessage);
                var fullErrorMessage = string.Join("; ", errorMessages);
                return Json(new { success = false, message = $"Validation error: {fullErrorMessage}" });
            }
            catch (Exception ex)
            {
                
                System.Diagnostics.Debug.WriteLine($"Error processing payment: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while processing your payment. Please try again." });
            }
        }

        // GET: Client/GenerateTransactionPdf (actually HTML for printing)
        public ActionResult GenerateTransactionPdf()
        {
            if (Session["UserID"] == null)
                return new HttpStatusCodeResult(401, "Unauthorized");

            int userId = (int)Session["UserID"];

            var client = db.ClientEntity
                .Include(c => c.Billing.Transactions)
                .FirstOrDefault(c => c.UserID == userId);

            if (client == null)
                return new HttpStatusCodeResult(404, "Client not found");

            var transactions = client.Billing?.Transactions
                                .OrderByDescending(t => t.PaymentDate)
                                .Select(t => new TransactionViewModel
                                {
                                    TransactionID = t.TransactionID,
                                    Amount = t.Amount,
                                    PaymentDate = t.PaymentDate
                                }).ToList() ?? new List<TransactionViewModel>();

            ViewBag.ClientName = client.FullName;
            ViewBag.CurrentDate = DateTime.Now.ToString("MMMM dd, yyyy");

           
            return View("_TransactionHistoryPrintable", transactions);
        }
    }
}
    
