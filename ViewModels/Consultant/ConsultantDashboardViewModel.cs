    using Law_Firm_EMS.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Law_Firm_EMS.ViewModels.Consultant
    {
	    public class ConsultantDashboardViewModel
	    {
            public int PendingTasksCount { get; set; }
            public int AssignedClientsCount { get; set; }
            public IEnumerable<Evaluation> RecentEvaluations { get; set; }
            public int PaidLeaveRemaining { get; set; }
            public int SickLeaveRemaining { get; set; }
            public int UnpaidLeaveRemaining { get; set; }
            public int TotalPaidLeaveAllowance { get; set; }
            public int TotalSickLeaveAllowance { get; set; }
            public int TotalUnpaidLeaveAllowance { get; set; }
            public string LeaveChartLabels { get; set; }
            public string LeaveChartData { get; set; }

            public ConsultantDashboardViewModel()
            {
                RecentEvaluations = new List<Evaluation>();
            }
        }

        public class ConsultantEvaluationsViewModel
        {
            public List<Evaluation> Evaluations { get; set; }
        }
        public class ConsultantContactViewModel
        {
            public string Name { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
        }

        public class ConsultantClientListViewModel
        {
            public int UserID { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Status { get; set; }
        }

        public class ConsultantClientDetailViewModel
        {
            public Client ClientProfile { get; set; }
            public DocumentGroupViewModel Documents { get; set; }
            public Billing BillingSummary { get; set; }
            public List<Form> SubmittedForms { get; set; }
        }

        public class DocumentGroupViewModel
        {
            public List<Document> LoR_Documents { get; set; }
            public List<Document> EIA_PES_Documents { get; set; }
            public List<Document> Other_Documents { get; set; }

            public DocumentGroupViewModel()
            {
                LoR_Documents = new List<Document>();
                EIA_PES_Documents = new List<Document>();
                Other_Documents = new List<Document>();
            }
        }
        public class MyTasksViewModel
        {
            public int? SelectedClientID { get; set; }
            public IEnumerable<SelectListItem> AssignedClients { get; set; }
            public IEnumerable<Tasks> Tasks { get; set; }
    }
}