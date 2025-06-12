using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Law_Firm_EMS.Models
{
	public class Leave
	{
        [Key]
        public int LeaveID { get; set; }
        [Required]
        [StringLength(50)]
        public string Type { get; set; } 
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int RequestedByConsultantID { get; set; }
        [ForeignKey("RequestedByConsultantID")]
        public virtual Consultant RequestedByConsultant { get; set; }
        public int? ApprovedByHRID { get; set; }
        [ForeignKey("ApprovedByAdminID")]
        public virtual HR ApprovedByHR { get; set; }
        public int StatusID { get; set; }
        [ForeignKey("StatusID")]
        public virtual StatusType Status { get; set; }
    }
}