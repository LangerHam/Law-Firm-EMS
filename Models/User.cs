using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Law_Firm_EMS.Models
{
	public class User
	{
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } 
        public long PhoneNumber { get; set; }
        public bool IsActive { get; set; }
    }
}