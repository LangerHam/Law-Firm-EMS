using Law_Firm_EMS.Context;
using Law_Firm_EMS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Law_Firm_EMS.Controllers.Api
{
    public class ConsultantApiController : ApiController
    {
        private LawContextDb db;

        public ConsultantApiController()
        {
            db = new LawContextDb();
        }

        [HttpDelete]
        [Route("api/tasks/{id}")]
        public IHttpActionResult DeleteTask(int id)
        {
            var session = System.Web.HttpContext.Current.Session;
            if (session["UserID"] == null || (int)session["RoleID"] != 3)
            {
                return Unauthorized();
            }

            int consultantId = (int)session["UserID"];

            var task = db.TasksEntity.Find(id);
            if (task == null) return NotFound();

            if (task.AssignedToConsultantID != consultantId)
            {
                return Unauthorized();
            }

            db.TasksEntity.Remove(task);
            db.SaveChanges();

            return Ok(new { message = "Task removed successfully." });
        }
        
    }
}

