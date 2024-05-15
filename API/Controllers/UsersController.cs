using API.Data;
using API.Entities;
using API.Interfaces;
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
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet] // https://api/users
    public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
    {
        return Ok(await _userRepository.GetUsersAsync());

    }

    // [HttpGet("{id}")] // https://api/users/2
    // public async Task<ActionResult<AppUser>> GetUser(int id)
    // {
    //     return await _userRepository.GetUserByIdAsync(id);
    // }

    [HttpGet("{username}")] // https://api/users/username
    public async Task<ActionResult<AppUser>> GetUser(string username)
    {
        return await _userRepository.GetUserbyNameAsync(username);
    }
}
