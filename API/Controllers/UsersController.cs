using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace API.Controllers;
//[AllowAnonymous] // This should not be used at the top level. This will bypass any [Authorize] attribute on method/endpoint level. 
//Ref- https://learn.microsoft.com/en-us/aspnet/core/security/authorization/simple?view=aspnetcore-8.0
[Authorize]
public class UsersController : BaseApiController
{
    private readonly DataContext _context;

    public UsersController(DataContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    [HttpGet] // https://api/users
    public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
    {
        var users = await _context.Users.ToListAsync();

        return users;
    }

    [HttpGet("{id}")] // https://api/users/2
    public async Task<ActionResult<AppUser>> GetUser(int id)
    {
        return await _context.Users.FindAsync(id);
    }
}
