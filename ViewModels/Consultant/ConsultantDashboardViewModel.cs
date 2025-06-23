using Law_Firm_EMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Law_Firm_EMS.ViewModels.Consultant
{
	public class ConsultantDashboardViewModel
	{
        public int PendingTasksCount { get; set; }
        public int AssignedClientsCount { get; set; }
        public IEnumerable<Evaluation> RecentEvaluations { get; set; }
        public int PaidLeaveRemaining { get; set; }
        public int SickLeaveRemaining { get; set; }
        public int UnpaidLeaveRemaining { get; set; }
        public int TotalPaidLeaveAllowance { get; set; }
        public int TotalSickLeaveAllowance { get; set; }
        public int TotalUnpaidLeaveAllowance { get; set; }
        public string LeaveChartLabels { get; set; }
        public string LeaveChartData { get; set; }

        public ConsultantDashboardViewModel()
        {
            RecentEvaluations = new List<Evaluation>();
        }
    }
}