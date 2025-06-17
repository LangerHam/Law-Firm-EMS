using Law_Firm_EMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Law_Firm_EMS.ViewModels.HR
{
	public class LeaveManagementViewModel
	{
        public IEnumerable<Leave> PendingRequests { get; set; }
        public DateTime CalendarDate { get; set; }
    }

    public class CalendarEvent
    {
        public string Title { get; set; } 
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Type { get; set; }
    }
}
