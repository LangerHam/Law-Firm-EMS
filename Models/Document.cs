using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Law_Firm_EMS.Models
{
	public class Document
	{
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DocumentID { get; set; }
        [Required]
        public string UploadPath { get; set; }
        public int ClientID { get; set; }
        [ForeignKey("ClientID")]
        public virtual Client Client { get; set; }
        public int UploadedByUserID { get; set; }
        [ForeignKey("UploadedByUserID")]
        public virtual Users UploadedByUser { get; set; }
        public int DocumentTypeID { get; set; }
        [ForeignKey("DocumentTypeID")]
        public virtual DocumentType DocumentType { get; set; }
        public int StatusID { get; set; }
        [ForeignKey("StatusID")]
        public virtual StatusType Status { get; set; }
        public int? ParentDocumentID { get; set; }
        [ForeignKey("ParentDocumentID")]
        public virtual Document ParentDocument { get; set; }
        public virtual ICollection<Document> Revisions { get; set; }
        //[Required]
        //[InverseProperty("Document")]
        public virtual Tasks Task { get; set; }

    }
}