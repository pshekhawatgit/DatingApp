using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SQLitePCL;

namespace API.Controllers;
// Class with some error creating methods/endpoint to create bugs, which needs to be handled
public class BuggyController : BaseApiController
{
    private readonly DataContext _context;
    public BuggyController(DataContext context)
    {
        _context = context;
    }

     [Authorize]
     [HttpGet("auth")] // 401
     public ActionResult<string> GetSecret()
     {
        return "some secret text";
     }

     [HttpGet("server-error")] // 500
     public ActionResult<string> GetServerError()
     {
        var thing = _context.Users.Find(-1);

        var returnVal = thing.ToString();

        return returnVal;
     }

     [HttpGet("not-found")] // 404
     public ActionResult<AppUser> GetNotFound()
     {
        var thing = _context.Users.Find(-1);

        if(thing == null) return NotFound();

        return thing;
     }

    [HttpGet("bad-request")] // 400
     public ActionResult<string> GetBadRequest()
     {
        return BadRequest("This is a Bad Request.");
     }
}
