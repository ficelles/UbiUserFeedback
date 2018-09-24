using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using UbiUserFeedback.Models;

namespace UbiUserFeedback.Controllers
{
    public class FeedbacksController : ApiController
    {
        private UserFeedbackEntities db = new UserFeedbackEntities();

        // GET: api/Feedbacks
        public IQueryable<Feedback> GetFeedbacks()
        {
            int _filter;
            HttpRequestMessage request = Request;

            try
            {
                System.Net.Http.Headers.HttpRequestHeaders headers = Request.Headers;
                IEnumerable<string> headerValues = request.Headers.GetValues("Ubi-Rating");
                string _rating = headerValues.FirstOrDefault();
                _filter = Convert.ToInt32(_rating);
            }
            catch
            {
                _filter = 0;
            }

            if (_filter == 0)
            {
                return db.Feedbacks.OrderByDescending(o => o.SavedOn).Take(15);
            }
            else
            {
                return db.Feedbacks.Where(w => w.Rating == _filter).OrderByDescending(o => o.SavedOn).Take(15);
            }
        }

        // GET: api/Feedbacks/5
        [ResponseType(typeof(Feedback))]
        public async Task<IHttpActionResult> GetFeedback(string id)
        {
            HttpRequestMessage request = Request;
            System.Net.Http.Headers.HttpRequestHeaders headers = Request.Headers;
            IEnumerable<string> headerValues = request.Headers.GetValues("Ubi-UserId");
            string _userid = headerValues.FirstOrDefault();
            string _sessionid = id;

            object[] _keys = new object[2];
            _keys[0] = _sessionid;
            _keys[1] = _userid;

            Feedback feedback = await db.Feedbacks.FindAsync(_keys);
            if (feedback == null)
            {
                return NotFound();
            }

            return Ok(feedback);
        }

        // PUT: api/Feedbacks/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutFeedback(string id, Feedback feedback)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != feedback.SessionID)
            {
                return BadRequest();
            }

            db.Entry(feedback).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FeedbackExists(feedback.SessionID, feedback.UserID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Feedbacks
        [ResponseType(typeof(Feedback))]
        public async Task<IHttpActionResult> PostFeedback(Feedback feedback)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Feedbacks.Add(feedback);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (FeedbackExists(feedback.SessionID, feedback.UserID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = feedback.SessionID }, feedback);
        }

        // DELETE: api/Feedbacks/5
        [ResponseType(typeof(Feedback))]
        public async Task<IHttpActionResult> DeleteFeedback(string id)
        {
            Feedback feedback = await db.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }

            db.Feedbacks.Remove(feedback);
            await db.SaveChangesAsync();

            return Ok(feedback);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool FeedbackExists(string sessionid, string userid)
        {
            return db.Feedbacks.Count(e => (e.SessionID == sessionid) && (e.UserID == userid)) > 0;
        }
    }
}