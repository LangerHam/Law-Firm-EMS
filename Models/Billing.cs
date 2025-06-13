using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Law_Firm_EMS.Models
{
	public class Billing
	{
        [Key, ForeignKey("Client")]
        public int ClientID { get; set; }
        public decimal TotalFees { get; set; }
        public decimal PaidAmount { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal RemainingAmount { get { return TotalFees - PaidAmount; } private set { } }
        public virtual Client Client { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}