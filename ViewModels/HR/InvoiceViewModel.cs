using Law_Firm_EMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Law_Firm_EMS.ViewModels.HR
{
	public class InvoiceViewModel
	{
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal Profit { get; set; }
        public int ConsultantCount { get; set; }
        public decimal ConsultantSalaries { get; set; }
        public decimal MiscExpenses { get; set; }
        public DateTime ReportMonth { get; set; }
        public IEnumerable<Transaction> IncomeTransactions { get; set; }
    }
}