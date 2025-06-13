using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Law_Firm_EMS.Models
{
	public class FormType
	{
        [Key]
        public int FormTypeID { get; set; }
        [Required]
        [StringLength(100)]
        public string TypeName { get; set; }
        public string TemplatePath { get; set; }
    }
}