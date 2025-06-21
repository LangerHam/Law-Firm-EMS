using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Law_Firm_EMS.ViewModels
{
    public class ClientDashboardViewModel
    {
        public string ClientName { get; set; }
        public string ConsultantName { get; set; }
        public List<LORStatusVM> LORs { get; set; }
        public string EIAStatus { get; set; }
        public string PESPStatus { get; set; }
        public List<StatusSummaryVM> StatusSummary { get; set; }
        public decimal Paid { get; set; }
        public decimal Due { get; set; }
        public decimal TotalFees { get; set; }
    }

    public class StatusSummaryVM
    {
        public string Status { get; set; }
        public int Count { get; set; }
    }

    public class LORStatusVM
    {
        public string UploadPath { get; set; }
        public string StatusName { get; set; }
    }

    public class ClientFormsViewModel
    {
        public List<FormDisplayItem> AvailableForms { get; set; }
    }

    public class FormDisplayItem
    {
        public int FormTypeID { get; set; }
        public string FormTypeName { get; set; }
        public string FormTypeDescription { get; set; }
        public bool IsOptional { get; set; } 
        public string TemplatePath { get; set; }
        public string Status { get; set; }
        public string UploadPath { get; set; }
        public DateTime? SubmittedDate { get; set; }
    }

    public class ClientDocumentsViewModel
    {
        public List<DocumentItemVM> LORDocuments { get; set; }
        public List<DocumentItemVM> MiscDocuments { get; set; }
    }

    public class DocumentItemVM
    {
        public int DocumentID { get; set; }
        public string FileName { get; set; }
        public string UploadPath { get; set; }
        public string StatusName { get; set; }
        public DateTime? UploadedDate { get; set; }
    }

}