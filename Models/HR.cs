using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Threading.Tasks;
using System.Web;

namespace Law_Firm_EMS.Models
{
	public class HR
	{
        [Key, ForeignKey("User")]
        public int UserID { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        public virtual Users User { get; set; }
        public virtual ICollection<Consultant> ManagedConsultants { get; set; }
        public virtual ICollection<Tasks> AssignedTasks { get; set; }
        public virtual ICollection<Leave> ApprovedLeaves { get; set; }
        public virtual ICollection<Evaluation> ConductedEvaluations { get; set; }
        public virtual ICollection<Invoice> GeneratedInvoices { get; set; }
    }
}