using Law_Firm_EMS.Context;
using Law_Firm_EMS.Models;
using Law_Firm_EMS.ViewModels;
using Law_Firm_EMS.ViewModels.HR;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Rotativa;

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
                    HRID = (int)Session["UserID"],
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
                if(status.StatusName == "Accpted")
                {
                    var existingbilling = db.BillingEntity.FirstOrDefault(b => b.ClientID == client.UserID);
                    if (existingbilling == null)
                    {
                        var billing = new Billing
                        {
                            ClientID = id,
                            TotalFees = 50000,
                            PaidAmount = 0,
                        };
                        db.BillingEntity.Add(billing);
                    }                    
                }
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

        // --- TASK MANAGEMENT ACTIONS ---

        public ActionResult ManageTasks()
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1) return RedirectToAction("Login", "Login");

            var createTaskForm = new TaskViewModel
            {
                ClientList = new SelectList(db.ClientEntity.ToList(), "UserID", "FullName"),
                ConsultantList = new SelectList(db.ConsultantEntity.ToList(), "UserID", "Name"),
                NewDocumentTypeList = new SelectList(db.DocumentTypeEntity.Where(dt => dt.TypeName == "EIA" || dt.TypeName == "PES").ToList(), "DocumentTypeID", "TypeName"),
                UnassignedLoRList = new List<SelectListItem>()
            };

            var taskList = db.TasksEntity
                .Include(t => t.Document.Client.User)
                .Include(t => t.AssignedToConsultant.User)
                .Include(t => t.Document.DocumentType)
                .Include(t => t.Status)
                .OrderByDescending(t => t.DocumentID)
                .ToList();

            var viewModel = new ManageTasksViewModel
            {
                TaskList = taskList,
                CreateTaskForm = createTaskForm
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTask(ManageTasksViewModel viewModel)
        {
            var formModel = viewModel.CreateTaskForm;

            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1) return RedirectToAction("Login", "Login");

            if (string.IsNullOrEmpty(formModel.TaskCreationType))
            {
                ModelState.AddModelError("CreateTaskForm.TaskCreationType", "Please select a task creation method.");
            }
            else if (formModel.TaskCreationType == "FromLoR" && !formModel.SelectedLoRDocumentID.HasValue)
            {
                ModelState.AddModelError("CreateTaskForm.SelectedLoRDocumentID", "You must select an LoR draft to assign.");
            }
            else if (formModel.TaskCreationType == "NewDocument")
            {
                if (!formModel.SelectedClientID.HasValue) ModelState.AddModelError("CreateTaskForm.SelectedClientID", "Please select a client.");
                if (!formModel.NewDocumentTypeID.HasValue) ModelState.AddModelError("CreateTaskForm.NewDocumentTypeID", "Please select a document type.");
            }

            if (ModelState.IsValid)
            {
                Document documentToTask;
                var hrUserId = (int)Session["UserID"];

                var inProgressStatusId = db.StatusTypeEntity.FirstOrDefault(s => s.StatusName == "In Progress").StatusID;
                var pendingStatusId = db.StatusTypeEntity.FirstOrDefault(s => s.StatusName == "Pending").StatusID;

                if (formModel.TaskCreationType == "FromLoR")
                {
                    var originalDoc = db.DocumentEntity.Find(formModel.SelectedLoRDocumentID.Value);
                    documentToTask = new Document
                    {
                        ClientID = originalDoc.ClientID,
                        UploadedByUserID = hrUserId,
                        DocumentTypeID = originalDoc.DocumentTypeID,
                        StatusID = inProgressStatusId,
                        ParentDocumentID = originalDoc.ParentDocumentID ?? originalDoc.DocumentID,
                        UploadPath = "Awaiting upload..."
                    };
                }
                else // NewDocument
                {
                    documentToTask = new Document
                    {
                        ClientID = formModel.SelectedClientID.Value,
                        UploadedByUserID = hrUserId,
                        DocumentTypeID = formModel.NewDocumentTypeID.Value,
                        StatusID = inProgressStatusId,
                        UploadPath = "Awaiting upload..."
                    };
                }

                db.DocumentEntity.Add(documentToTask);

                var newTask = new Tasks
                {
                    Document = documentToTask,
                    AssignedToConsultantID = formModel.AssignedToConsultantID,
                    AssignedByHRID = hrUserId,
                    Instructions = formModel.Instructions,
                    StatusID = pendingStatusId
                };
                db.TasksEntity.Add(newTask);

                db.SaveChanges();

                return RedirectToAction("ManageTasks");
            }

            var taskList = db.TasksEntity.Include(t => t.Document.Client.User)
                                   .Include(t => t.AssignedToConsultant.User)
                                   .Include(t => t.Document.DocumentType)
                                   .Include(t => t.Status)
                                   .OrderByDescending(t => t.DocumentID).ToList();

            formModel.ClientList = new SelectList(db.ClientEntity.ToList(), "UserID", "FullName", formModel.SelectedClientID);
            formModel.ConsultantList = new SelectList(db.ConsultantEntity.ToList(), "UserID", "Name", formModel.AssignedToConsultantID);
            formModel.NewDocumentTypeList = new SelectList(db.DocumentTypeEntity.Where(dt => dt.TypeName == "EIA" || dt.TypeName == "PES").ToList(), "DocumentTypeID", "TypeName", formModel.NewDocumentTypeID);

            if (formModel.SelectedClientID.HasValue)
            {
                int lorTypeId = db.DocumentTypeEntity.FirstOrDefault(dt => dt.TypeName == "LOR").DocumentTypeID;
                var unassignedLoRs = db.DocumentEntity
                    .Where(d => d.ClientID == formModel.SelectedClientID.Value && d.DocumentTypeID == lorTypeId && d.Task == null)
                    .ToList();
                formModel.UnassignedLoRList = new SelectList(unassignedLoRs, "DocumentID", "DocumentID", formModel.SelectedLoRDocumentID);
            }
            else
            {
                formModel.UnassignedLoRList = new List<SelectListItem>();
            }

            viewModel.TaskList = taskList;
            viewModel.CreateTaskForm = formModel;

            return View("ManageTasks", viewModel);
        }

        public JsonResult GetUnassignedLoRs(int clientId)
        {
            int lorTypeId = db.DocumentTypeEntity.FirstOrDefault(dt => dt.TypeName == "LOR").DocumentTypeID;

            var unassignedLoRs = db.DocumentEntity
                .Where(d => d.ClientID == clientId && d.DocumentTypeID == lorTypeId && d.Task == null)
                .Select(d => new {
                    Value = d.DocumentID,
                    Text = "LoR Draft (ID: " + d.DocumentID + ") - Uploaded: " + d.UploadedByUser.CreatedAt
                }).ToList();

            return Json(unassignedLoRs, JsonRequestBehavior.AllowGet);
        }

        //-------LEAVE MANAGEMENT--------

        public ActionResult ManageLeaveRequests()
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1) return RedirectToAction("Login", "Login");

            var pendingStatusId = db.StatusTypeEntity.FirstOrDefault(s => s.StatusName == "Pending")?.StatusID;

            var viewModel = new LeaveManagementViewModel
            {
                PendingRequests = pendingStatusId.HasValue
                    ? db.LeaveEntity
                        .Where(l => l.StatusID == pendingStatusId.Value)
                        .Include(l => l.RequestedByConsultant)
                        .OrderBy(l => l.FromDate)
                        .ToList()
                    : new List<Leave>(),
                CalendarDate = DateTime.Today
            };

            return View(viewModel);
        }

        public JsonResult GetLeaveEvents(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var approvedStatusId = db.StatusTypeEntity.FirstOrDefault(s => s.StatusName == "Accepted").StatusID;

            var events = db.LeaveEntity
        .Where(l => l.StatusID == approvedStatusId && l.FromDate < endDate && l.ToDate >= startDate)
        .Include(l => l.RequestedByConsultant)
        .ToList()
        .Select(l => new CalendarEvent
        {
            Title = $"{l.RequestedByConsultant.Name} ({l.Type})",
            Start = l.FromDate,
            End = l.ToDate.AddDays(1),
            Type = l.Type
        }).ToList();

            return Json(events, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateLeaveStatus(int leaveId, string decision)
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1) return RedirectToAction("Login", "Login");

            var leave = db.LeaveEntity.Find(leaveId);
            if (leave == null) return HttpNotFound();

            string newStatusName = (decision.ToLower() == "accept") ? "Accepted" : "Rejected";
            var newStatus = db.StatusTypeEntity.FirstOrDefault(s => s.StatusName == newStatusName);

            if (newStatus != null)
            {
                leave.StatusID = newStatus.StatusID;
                leave.ApprovedByHRID = (int)Session["UserID"]; 
                db.SaveChanges();
                TempData["SuccessMessage"] = $"Leave request has been {newStatusName.ToLower()}.";
            }

            return RedirectToAction("ManageLeaveRequests");
        }

        //-------EVALUATION MANAGEMENT--------

        public ActionResult ManageEvaluations()
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1) return RedirectToAction("Login", "Login");

            var evaluations = db.EvaluationEntity
                .Include(e => e.Consultant.User)
                .Include(e => e.EvaluatedByHR.User)
                .OrderByDescending(e => e.DateGiven)
                .ToList();

            return View(evaluations);
        }

        public ActionResult CreateEvaluation()
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1) return RedirectToAction("Login", "Login");

            var viewModel = new EvaluationViewModel
            {
                ConsultantList = new SelectList(db.ConsultantEntity.ToList(), "UserID", "Name")
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEvaluation(EvaluationViewModel viewModel)
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1) return RedirectToAction("Login", "Login");

            if (ModelState.IsValid)
            {
                var newEvaluation = new Evaluation
                {
                    Title = viewModel.Title,
                    ConsultantID = viewModel.ConsultantID,
                    Details = viewModel.Details,
                    DateGiven = System.DateTime.UtcNow,
                    EvaluatedByHRID = (int)Session["UserID"]
                };

                db.EvaluationEntity.Add(newEvaluation);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Evaluation has been saved successfully.";
                return RedirectToAction("ManageEvaluations");
            }

            viewModel.ConsultantList = new SelectList(db.ConsultantEntity.ToList(), "UserID", "Name", viewModel.ConsultantID);
            return View(viewModel);
        }

        //---------INVOICE MANAGEMENT---------

        public ActionResult ManageInvoices()
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1) return RedirectToAction("Login", "Login");

            var today = DateTime.UtcNow;
            var startDate = new DateTime(today.Year, today.Month, 1);
            var endDate = startDate.AddMonths(1);

            int consultantCount = db.ConsultantEntity.Count();
            decimal consultantSalaries = consultantCount * 40000;
            decimal miscExpenses = consultantSalaries * 0.10m; // 10% for misc expenses
            decimal totalExpenses = consultantSalaries + miscExpenses;

            var transactionsThisMonth = db.TransactionEntity
                .Where(t => t.PaymentDate >= startDate && t.PaymentDate < endDate)
                .Include(t => t.Billing.Client)
                .ToList();
            decimal totalIncome = transactionsThisMonth.Sum(t => t.Amount);

            decimal profit = totalIncome - totalExpenses;

            var viewModel = new InvoiceViewModel
            {
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                Profit = profit,
                ConsultantCount = consultantCount,
                ConsultantSalaries = consultantSalaries,
                MiscExpenses = miscExpenses,
                ReportMonth = today,
                IncomeTransactions = transactionsThisMonth
            };

            return View(viewModel);
        }

        public ActionResult GenerateInvoicePdf()
        {
            if (Session["RoleID"] == null || (int)Session["RoleID"] != 1) return RedirectToAction("Login", "Login");

            var today = DateTime.UtcNow;
            var startDate = new DateTime(today.Year, today.Month, 1);
            var endDate = startDate.AddMonths(1);

            int consultantCount = db.ConsultantEntity.Count();
            decimal consultantSalaries = consultantCount * 40000;
            decimal miscExpenses = consultantSalaries * 0.10m;
            decimal totalExpenses = consultantSalaries + miscExpenses;
            decimal totalIncome = db.TransactionEntity.Where(t => t.PaymentDate >= startDate && t.PaymentDate < endDate).Sum(t => (decimal?)t.Amount) ?? 0;
            decimal profit = totalIncome - totalExpenses;

            var viewModel = new InvoiceViewModel
            {
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                Profit = profit,
                ConsultantCount = consultantCount,
                ConsultantSalaries = consultantSalaries,
                MiscExpenses = miscExpenses,
                ReportMonth = today,
            };

            ViewBag.GeneratedBy = Session["Email"]?.ToString() ?? "HR Admin";
            string logoPath = Url.Content("~/Resources/Images/Logo.png");
            string absoluteLogoUrl = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, logoPath);
            ViewBag.LogoUrl = absoluteLogoUrl;

            return new ViewAsPdf("InvoicePdf", viewModel)
            {
                FileName = $"Monthly_Invoice_{today.ToString("MMMM_yyyy")}.pdf"
            };
        }
    }
}