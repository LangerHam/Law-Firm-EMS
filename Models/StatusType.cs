using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Law_Firm_EMS.Models
{
	public class StatusType
	{
        [Key]
        public int StatusID { get; set; }
        [Required]
        [StringLength(50)]
        public string StatusName { get; set; }
    }
}