using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Services.Description;

namespace Law_Firm_EMS.Models
{
	public class Form
	{
        [Key]
        public int FormID { get; set; }
        [Required]
        public string UploadPath { get; set; } 
        public int ClientID { get; set; }
        [ForeignKey("ClientID")]
        public virtual Client Client { get; set; }
        public int FormTypeID { get; set; }
        [ForeignKey("FormTypeID")]
        public virtual FormType FormType { get; set; }
        public int StatusID { get; set; }
        [ForeignKey("StatusID")]
        public virtual StatusType Status { get; set; }
    }
}