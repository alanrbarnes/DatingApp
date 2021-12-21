using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class BuggyController : BaseApiController
    {
        private readonly DataContext _context;

        // private readonly DataContext _context;
        public BuggyController(DataContext context)
        {
            _context = context;
        }
        
        //api/buggy/auth
        [Authorize]
        [HttpGet("auth")]
        public ActionResult<string> GetSecret() 
        {
            return "secret text";
        }

        [HttpGet("not-found")]
        public ActionResult<AppUser> GetNotFound() 
        {
            var thing = _context.Users.Find(-1);

            if(thing == null) return NotFound();

            return Ok(thing);
        }

        [HttpGet("server-error")]
        public ActionResult<string> GetServerError()  //uses string because of ToString() method
        {
            try
            {
                var thing = _context.Users.Find(-1);

                //generates null reference exception
                var thingToReturn = thing.ToString();

                return thingToReturn;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Computer says no!");
            }



        }

        [HttpGet("bad-request")]
        public ActionResult<string> GetBadRequest() 
        {
            return BadRequest("This was not a good request");
        }
    }
}