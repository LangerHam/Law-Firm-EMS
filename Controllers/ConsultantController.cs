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
using System.IO;

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
                .Include(c => c.Billing) 
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

            var submittedForms = db.FormEntity
                .Where(f => f.ClientID == id)
                .Include(f => f.FormType)
                .ToList();

            var viewModel = new ConsultantClientDetailViewModel
            {
                ClientProfile = client,
                Documents = documentGroups,
                BillingSummary = client.Billing, 
                SubmittedForms = submittedForms  
            };
            return View(viewModel);
        }

        public ActionResult MyTasks()
        {
            if (Session["UserID"] == null || (int)Session["RoleID"] != 3)
                return RedirectToAction("Login", "Login");

            int consultantId = (int)Session["UserID"];

            var assignedClients = db.ClientEntity
                .Where(c => c.AssignedConsultantID == consultantId)
                .OrderBy(c => c.FullName)
                .ToList();

            var tasks = db.TasksEntity
                .Where(t => t.AssignedToConsultantID == consultantId)
                .Include(t => t.Document.Client)
                .Include(t => t.Document.DocumentType)
                .Include(t => t.Status)
                .OrderByDescending(t => t.DocumentID)
                .ToList();

            var viewModel = new MyTasksViewModel
            {
                AssignedClients = new SelectList(assignedClients, "UserID", "FullName"),
                Tasks = tasks
            };

            ViewBag.StatusList = new SelectList(db.StatusTypeEntity.ToList(), "StatusID", "StatusName");

            return View(viewModel);
        }

        [HttpPost]
        public JsonResult UpdateTaskStatus(int taskId, int statusId)
        {
            if (Session["UserID"] == null || (int)Session["RoleID"] != 3)
                return Json(new { success = false, message = "Unauthorized" });

            int consultantId = (int)Session["UserID"];
            var task = db.TasksEntity.FirstOrDefault(t => t.DocumentID == taskId && t.AssignedToConsultantID == consultantId);           

            if (task == null)
                return Json(new { success = false, message = "Task not found." });

            var doc = db.DocumentEntity.Find(taskId);
            if (doc == null)
            {
                return Json(new { success = false, message = "Associated document could not be found." });
            }

            task.StatusID = statusId;
            doc.StatusID = statusId;
            db.SaveChanges();

            return Json(new { success = true, message = "Status updated." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitTaskDocument(int documentId, HttpPostedFileBase file)
        {
            if (Session["UserID"] == null || (int)Session["RoleID"] != 3)
                return RedirectToAction("Login", "Login");

            if (file == null || file.ContentLength == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to submit.";
                return RedirectToAction("MyTasks");
            }

            int consultantId = (int)Session["UserID"];
            var document = db.DocumentEntity.Include(d => d.Task).FirstOrDefault(d => d.DocumentID == documentId);

            if (document == null || document.Task.AssignedToConsultantID != consultantId)
            {
                TempData["ErrorMessage"] = "Document not found or you are not authorized to modify it.";
                return RedirectToAction("MyTasks");
            }

            string uploadDir = Server.MapPath("~/Uploads/ConsultantDocuments/");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string physicalPath = Path.Combine(uploadDir, uniqueFileName);
            file.SaveAs(physicalPath);
            document.UploadPath = "/Uploads/ConsultantDocuments/" + uniqueFileName;
            document.UploadedByUserID = consultantId;

            var submittedStatus = db.StatusTypeEntity.FirstOrDefault(s => s.StatusName == "Submitted");
            if (submittedStatus != null)
            {
                document.Task.StatusID = submittedStatus.StatusID;
            }

            db.SaveChanges();

            TempData["SuccessMessage"] = "Task submitted successfully!";
            return RedirectToAction("MyTasks");
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
            if (Session["UserID"] == null || (int)Session["RoleID"] != 3)
            {
                return RedirectToAction("Login", "Login");
            }
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

        public ActionResult Settings()
        {
            if (Session["UserID"] == null || (int)Session["RoleID"] != 3)
                return RedirectToAction("Login", "Login");

            int consultantId = (int)Session["UserID"];

            var consultant = db.ConsultantEntity.Include(c => c.User).FirstOrDefault(c => c.UserID == consultantId);
            if (consultant == null) return HttpNotFound();

            var viewModel = new ConsultantSettingsViewModel
            {
                Name = consultant.Name,
                Phone = consultant.Phone,
                Email = consultant.User.Email,
                CurrentProfilePhotoPath = consultant.ProfilePhotoPath
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Settings(ConsultantSettingsViewModel viewModel)
        {
            if (Session["UserID"] == null || (int)Session["RoleID"] != 3)
                return RedirectToAction("Login", "Login");

            int consultantId = (int)Session["UserID"];

            if (string.IsNullOrEmpty(viewModel.NewPassword) && string.IsNullOrEmpty(viewModel.ConfirmPassword))
            {
                ModelState.Remove("NewPassword");
                ModelState.Remove("ConfirmPassword");
            }

            if (ModelState.IsValid)
            {
                var consultantInDb = db.ConsultantEntity.Find(consultantId);
                var userInDb = db.UsersEntity.Find(consultantId);

                if (consultantInDb == null || userInDb == null) return HttpNotFound();

                consultantInDb.Name = viewModel.Name;
                consultantInDb.Phone = viewModel.Phone;

                if (!string.IsNullOrEmpty(viewModel.NewPassword))
                {
                    userInDb.PasswordHash = viewModel.NewPassword;
                }

                if (viewModel.ProfileImageFile != null && viewModel.ProfileImageFile.ContentLength > 0)
                {
                    string uploadDir = Server.MapPath("~/Resources/Images/");
                    if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(viewModel.ProfileImageFile.FileName);
                    string physicalPath = Path.Combine(uploadDir, uniqueFileName);
                    viewModel.ProfileImageFile.SaveAs(physicalPath);

                    consultantInDb.ProfilePhotoPath = "/Resources/Images/" + uniqueFileName;

                    Session["ProfilePhotoPath"] = consultantInDb.ProfilePhotoPath;
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Your profile has been updated successfully.";
                return RedirectToAction("Settings");
            }

            return View(viewModel);
        }


        // GET: Consultant/LeaveRequests
        public ActionResult LeaveRequests()
        {
            
            if (Session["UserID"] == null || (int)Session["RoleID"] != 3)
                return RedirectToAction("Login", "Login");

            int consultantId = (int)Session["UserID"];

            
            var leaveRequests = db.LeaveEntity
                .Where(l => l.RequestedByConsultantID == consultantId)
                .Include(l => l.Status) 
                .OrderByDescending(l => l.FromDate) 
                .ToList();

            
            var viewModel = new ConsultantLeaveViewModel
            {
                MyLeaveRequests = leaveRequests,
                
                AvailableLeaveTypes = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Paid Leave", Value = "Paid Leave" },
                    new SelectListItem { Text = "Sick Leave", Value = "Sick Leave" },
                    new SelectListItem { Text = "Unpaid Leave", Value = "Unpaid Leave" }
                }
            };

            return View(viewModel);
        }

        // POST: Consultant/ApplyForLeave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApplyForLeave(ConsultantLeaveViewModel model)
        {
           
            if (Session["UserID"] == null || (int)Session["RoleID"] != 3)
                return RedirectToAction("Login", "Login");

            int consultantId = (int)Session["UserID"];

            
            model.AvailableLeaveTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Paid Leave", Value = "Paid Leave" },
                new SelectListItem { Text = "Sick Leave", Value = "Sick Leave" },
                new SelectListItem { Text = "Unpaid Leave", Value = "Unpaid Leave" }
            };

           
            if (model.FromDate == default(DateTime) || model.ToDate == default(DateTime))
            {
                ModelState.AddModelError("", "Please select valid start and end dates.");
            }
            else if (model.FromDate > model.ToDate)
            {
                ModelState.AddModelError("", "Start Date cannot be after End Date.");
            }
            else if (model.FromDate < DateTime.Today) 
            {
                ModelState.AddModelError("", "Start Date cannot be in the past.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    
                    var pendingStatus = db.StatusTypeEntity.FirstOrDefault(s => s.StatusName == "Pending");
                    if (pendingStatus == null)
                    {
                        
                        TempData["ErrorMessage"] = "System error: 'Pending' status not found. Please contact support.";
                       
                        model.MyLeaveRequests = db.LeaveEntity
                                                 .Where(l => l.RequestedByConsultantID == consultantId)
                                                 .Include(l => l.Status)
                                                 .OrderByDescending(l => l.FromDate)
                                                 .ToList();
                        return View("LeaveRequests", model);
                    }

                    
                    var newLeave = new Leave
                    {
                        Type = model.LeaveType,
                        FromDate = model.FromDate,
                        ToDate = model.ToDate,
                        RequestedByConsultantID = consultantId,
                        StatusID = pendingStatus.StatusID,
                        
                    };

                    db.LeaveEntity.Add(newLeave);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Your leave request has been submitted successfully and is pending approval!";
                    return RedirectToAction("LeaveRequests"); 
                }
                catch (Exception ex)
                {
                    
                    System.Diagnostics.Debug.WriteLine($"Error submitting leave request: {ex.Message}");
                    TempData["ErrorMessage"] = $"An unexpected error occurred: {ex.Message}. Please try again.";

                    
                    model.MyLeaveRequests = db.LeaveEntity
                                             .Where(l => l.RequestedByConsultantID == consultantId)
                                             .Include(l => l.Status)
                                             .OrderByDescending(l => l.FromDate)
                                             .ToList();
                    return View("LeaveRequests", model);
                }
            }

          
            model.MyLeaveRequests = db.LeaveEntity
                                     .Where(l => l.RequestedByConsultantID == consultantId)
                                     .Include(l => l.Status)
                                     .OrderByDescending(l => l.FromDate)
                                     .ToList();
            return View("LeaveRequests", model); 
        }
    }
}