using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public string LOROverallStatus { get; set; }
    }

    public class StatusSummaryVM
    {
        public string Status { get; set; }
        public int Count { get; set; }

        public string Color { get; set; }
    }

    public class LORStatusVM
    {
        public int DocumentID { get; set; }
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

    public class ClientBillingViewModel
    {
        public string ClientName { get; set; }
        public string ConsultantName { get; set; }

        public BillingSummaryViewModel BillingSummary { get; set; }
        public List<TransactionViewModel> Transactions { get; set; }

        public ClientBillingViewModel()
        {
            BillingSummary = new BillingSummaryViewModel();
            Transactions = new List<TransactionViewModel>();
        }
    }

    public class BillingSummaryViewModel
    {
        [Display(Name = "Service Fees")]
        [DisplayFormat(DataFormatString = "{0:C}")] // Currency format
        public decimal TotalFees { get; set; }

        [Display(Name = "Paid")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal PaidAmount { get; set; }

        [Display(Name = "Due")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal DueAmount { get; set; }
    }

    public class TransactionViewModel
    {
        public int TransactionID { get; set; }

        [Display(Name = "Amount Paid")]
        [DisplayFormat(DataFormatString = "{0:C}")] // Currency format
        public decimal Amount { get; set; }

        [Display(Name = "Payment Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")] // Date and time format
        public DateTime PaymentDate { get; set; }
    }

    // ViewModel for payment input (optional, can also use raw properties in action)
    public class ProcessPaymentViewModel
    {
        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal AmountToPay { get; set; }
    }

    public class ClientSettingsViewModel
    {
        public int UserID { get; set; } // The ID of the associated User record (and Client record)

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Phone Number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters.")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } // Now editable

        // For changing password
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; }
    }

}