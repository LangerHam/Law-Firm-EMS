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

   

}