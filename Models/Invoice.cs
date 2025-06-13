using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Law_Firm_EMS.Models
{
	public class Invoice
	{
        [Key]
        public int InvoiceID { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string ExportPath { get; set; }
        public decimal SalaryPaid { get; set; }
        public decimal ClientPaymentReceived { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal CompanyProfit { get { return ClientPaymentReceived - SalaryPaid; } private set { } }
        public int ClientID { get; set; }
        [ForeignKey("ClientID")]
        public virtual Client Client { get; set; }
        public int ConsultantID { get; set; }
        [ForeignKey("ConsultantID")]
        public virtual Consultant Consultant { get; set; }
        public int GeneratedByHRID { get; set; }
        [ForeignKey("GeneratedByHRID")]
        public virtual HR GeneratedByHR { get; set; }
    }
}