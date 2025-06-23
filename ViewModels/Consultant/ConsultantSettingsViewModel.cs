using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Law_Firm_EMS.ViewModels.Consultant
{
	public class ConsultantSettingsViewModel
	{
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(20)]
        public string Phone { get; set; }
        public string CurrentProfilePhotoPath { get; set; }
        public string Email { get; set; }
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        public HttpPostedFileBase ProfileImageFile { get; set; }
    }
}