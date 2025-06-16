using Law_Firm_EMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Law_Firm_EMS.ViewModels
{
	public class HRDashboardViewModel
	{
        public List<Client> NewClientRequests { get; set; }
        public List<Leave> PendingLeaveRequests { get; set; }

        public HRDashboardViewModel()
        {
            NewClientRequests = new List<Client>();
            PendingLeaveRequests = new List<Leave>();
        }
    }
}