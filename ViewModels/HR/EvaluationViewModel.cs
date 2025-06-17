using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Law_Firm_EMS.ViewModels.HR
{
	public class EvaluationViewModel
	{
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please select a consultant to evaluate.")]
        [Display(Name = "Consultant")]
        public int ConsultantID { get; set; }

        [Required(ErrorMessage = "Please rate Question 1.")]
        [Display(Name = "1. Quality of Work & Attention to Detail")]
        public string Q1_Answer { get; set; }

        [Required(ErrorMessage = "Please rate Question 2.")]
        [Display(Name = "2. Client Communication & Professionalism")]
        public string Q2_Answer { get; set; }

        [Required(ErrorMessage = "Please rate Question 3.")]
        [Display(Name = "3. Timeliness & Deadline Adherence")]
        public string Q3_Answer { get; set; }

        [AllowHtml]
        [Display(Name = "Final Assessment Details")]
        public string Details { get; set; }

        public IEnumerable<SelectListItem> ConsultantList { get; set; }
    }
}