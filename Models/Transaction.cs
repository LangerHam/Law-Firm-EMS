using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Law_Firm_EMS.Models
{
	public class Transaction
	{
        [Key]
        public int TransactionID { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public int BillingID { get; set; }
        [ForeignKey("BillingID")]
        public virtual Billing Billing { get; set; }
    }
}