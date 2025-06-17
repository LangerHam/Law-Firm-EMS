using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Law_Firm_EMS.ViewModels
{
	public class ConsultantViewModel
	{
        public int UserID { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; }
        [StringLength(20)]
        public string Phone { get; set; }
        public string ProfilePhotoPath { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6), DataType(DataType.Password)]
        public string Password { get; set; }
    }
}