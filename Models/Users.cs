using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Law_Firm_EMS.Models
{
	public class Users
	{
        [Key]
        public int UserID { get; set; }
        [Required]
        [Index(IsUnique = true)]
        [StringLength(256)]
        public string Email { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int RoleID { get; set; }
        [ForeignKey("RoleID")]
        public virtual Role Role { get; set; }

        public virtual Client Client { get; set; }
        public virtual Consultant Consultant { get; set; }
        public virtual HR HR { get; set; }
    }
}