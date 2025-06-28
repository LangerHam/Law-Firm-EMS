using Law_Firm_EMS.Context;
using Law_Firm_EMS.Models;
using Law_Firm_EMS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Law_Firm_EMS.Controllers.Api
{
    public class HRApiController : ApiController
    {
        private LawContextDb db;

        public HRApiController()
        {
            db = new LawContextDb();
        }

        [HttpGet, Route("api/consultants")]
        public IHttpActionResult GetConsultants()
        {
            var consultants = db.ConsultantEntity
                .Include(c => c.User)
                .OrderBy(c => c.Name)
                .ToList()
                .Select(c => new {
                    c.UserID,
                    c.Name,
                    c.Phone,
                    c.ProfilePhotoPath,
                    Email = c.User.Email
                });
            return Ok(consultants);
        }

        [HttpGet, Route("api/consultants/{id}")]
        public IHttpActionResult GetConsultant(int id)
        {
            var consultant = db.ConsultantEntity.Include(c => c.User).FirstOrDefault(c => c.UserID == id);
            if (consultant == null) return NotFound();

            return Ok(new
            {
                consultant.UserID,
                consultant.Name,
                consultant.Phone,
                consultant.ProfilePhotoPath,
                Email = consultant.User.Email
            });
        }

        [HttpPost, Route("api/consultants")]
        public IHttpActionResult CreateConsultant(ConsultantViewModel viewModel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (db.UsersEntity.Any(u => u.Email == viewModel.Email))
                return BadRequest("An account with this email already exists.");

            var user = new Users {
                Email = viewModel.Email,
                PasswordHash = viewModel.Password, 
                RoleID = 3, 
                CreatedAt = DateTime.UtcNow
            };
            var consultant = new Consultant {
                User = user,
                Name = viewModel.Name,
                Phone = viewModel.Phone,
                ProfilePhotoPath = viewModel.ProfilePhotoPath
            };
            db.ConsultantEntity.Add(consultant);
            db.SaveChanges();
            var ret = new Uri(Request.RequestUri + "/" + consultant.UserID);
            return Created(ret, new { consultant.UserID, consultant.Name, consultant.Phone, Email = user.Email });
        }

        [HttpPut, Route("api/consultants/{id}")]
        public IHttpActionResult UpdateConsultant(int id, Consultant consultantData)
        {
            if (!ModelState.IsValid || id != consultantData.UserID) return BadRequest();
            var consultantInDb = db.ConsultantEntity.Find(id);
            if (consultantInDb == null) return NotFound();

            consultantInDb.Name = consultantData.Name;
            consultantInDb.Phone = consultantData.Phone;
            consultantInDb.ProfilePhotoPath = consultantData.ProfilePhotoPath;

            db.SaveChanges();
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete, Route("api/consultants/{id}")]
        public IHttpActionResult DeleteConsultant(int id)
        {
            Consultant consultant = db.ConsultantEntity.Include(c => c.User).FirstOrDefault(c => c.UserID == id);
            if (consultant == null) return NotFound();

            db.UsersEntity.Remove(consultant.User);
            db.ConsultantEntity.Remove(consultant);
            db.SaveChanges();
            return Ok(consultant);
        }
    }
}
