using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Law_Firm_EMS.Models
{
	public class DocumentType
	{
        [Key]
        public int DocumentTypeID { get; set; }
        [Required]
        [StringLength(50)]
        public string TypeName { get; set; }
        public string TemplatePath { get; set; }
    }
}