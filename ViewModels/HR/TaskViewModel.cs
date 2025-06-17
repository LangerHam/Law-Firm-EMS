using Law_Firm_EMS.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Law_Firm_EMS.ViewModels.HR
{
    public class ManageTasksViewModel
    {
        public IEnumerable<Tasks> TaskList { get; set; }
        public TaskViewModel CreateTaskForm { get; set; }
    }
    public class TaskViewModel
	{
        [Required]
        [Display(Name = "Task Type")]
        public string TaskCreationType { get; set; } 

        [Display(Name = "Client")]
        public int? SelectedClientID { get; set; }

        [Display(Name = "Document Type (for new tasks)")]
        public int? NewDocumentTypeID { get; set; }

        [Display(Name = "Existing LoR Draft")]
        public int? SelectedLoRDocumentID { get; set; }

        [Required]
        [Display(Name = "Assign to Consultant")]
        public int AssignedToConsultantID { get; set; }

        public string Instructions { get; set; }

        public IEnumerable<SelectListItem> ClientList { get; set; }
        public IEnumerable<SelectListItem> ConsultantList { get; set; }
        public IEnumerable<SelectListItem> NewDocumentTypeList { get; set; }
        public IEnumerable<SelectListItem> UnassignedLoRList { get; set; }
    }
}