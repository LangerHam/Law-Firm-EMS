using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Services.Description;
using System.Xml.Linq;

namespace Law_Firm_EMS.Models
{
	public class Client
	{
        [Key,ForeignKey("User")]
        public int UserID { get; set; }
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }
        [StringLength(20)]
        public string Phone { get; set; }
        public string CaseDetails { get; set; }
        public int StatusID { get; set; }
        [ForeignKey("StatusID")]
        public virtual StatusType Status { get; set; }
        public int? AssignedConsultantID { get; set; }
        [ForeignKey("AssignedConsultantID")]
        public virtual Consultant AssignedConsultant { get; set; }
        public virtual Users User { get; set; }
        public virtual ICollection<Document> Documents { get; set; }
        public virtual Billing Billing { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }
    }
}