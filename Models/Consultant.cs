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
	public class Consultant
	{
        [Key, ForeignKey("User")]
        public int UserID { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(20)]
        public string Phone { get; set; }
        public string ProfilePhotoPath { get; set; }
        public int? HRID { get; set; }
        [ForeignKey("HRID")]
        public virtual HR HR { get; set; }
        public virtual Users User { get; set; }
        public virtual ICollection<Client> AssignedClients { get; set; }
        public virtual ICollection<Tasks> AssignedTasks { get; set; }
        public virtual ICollection<Leave> LeaveRequests { get; set; }
        public virtual ICollection<Evaluation> Evaluations { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }
    }
}