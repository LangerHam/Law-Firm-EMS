using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Law_Firm_EMS.Models
{
	public class Tasks
	{
        [Key, ForeignKey("Document")]
        public int DocumentID { get; set; }
        public string Instructions { get; set; }
        public virtual Document Document { get; set; }
        public int AssignedToConsultantID { get; set; }
        [ForeignKey("AssignedToConsultantID")]
        public virtual Consultant AssignedToConsultant { get; set; }
        public int AssignedByHRID { get; set; }
        [ForeignKey("AssignedByHRID")]
        public virtual HR AssignedByHR { get; set; }
        public int StatusID { get; set; }
        [ForeignKey("StatusID")]
        public virtual StatusType Status { get; set; }
    }
}