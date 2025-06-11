using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Law_Firm_EMS.Models
{
	public class Role
	{
		[Key]
		public int RoleId { get; set; }
		[StringLength(50)]
        public string RoleName { get; set; }
		public virtual ICollection<Users> User { get; set; }
    }
}