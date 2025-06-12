using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Law_Firm_EMS.Models
{
	public class Evaluation
	{
        [Key]
        public int EvalID { get; set; }
        [Required]
        public string Title { get; set; }
        public string Details { get; set; }
        public DateTime DateGiven { get; set; }
        public int ConsultantID { get; set; }
        [ForeignKey("ConsultantID")]
        public virtual Consultant Consultant { get; set; }
        public int EvaluatedByHRID { get; set; }
        [ForeignKey("EvaluatedByHRID")]
        public virtual HR EvaluatedByHR { get; set; }
    }
}