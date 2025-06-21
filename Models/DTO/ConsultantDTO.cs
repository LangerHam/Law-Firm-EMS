using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Law_Firm_EMS.Models.DTO
{
	public class ConsultantDTO
	{}

    public class ClientListDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
    }
}