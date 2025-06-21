using Law_Firm_EMS.Context;
using Law_Firm_EMS.Models;
using Law_Firm_EMS.Models.DTO;
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

        [HttpGet]
        [Route("api/consultants/myclients")]
        public IHttpActionResult GetMyClients()
        {
            var session = HttpContext.Current.Session;
            if (session["UserID"] == null || (int)session["RoleID"] != 3)
            {                
                return Unauthorized();
            }

            int userId = (int)session["UserID"];
            var clients = db.ClientEntity
                .Where(c => c.AssignedConsultantID == userId)
                .Include(c => c.User)
                .Include(c => c.Status)
                .OrderBy(c => c.FullName)
                .ToList();

            var clientDtos = clients.Select(c => new ClientListDto
            {
                UserID = c.UserID,
                FullName = c.FullName,
                Email = c.User.Email,
                Status = c.Status.StatusName
            }).ToList();

            return Ok(clientDtos);
        }
    }
}
