using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
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
    private readonly IMapper _mapper;

    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    [HttpGet] // https://api/users
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
        IEnumerable<AppUser> users = await _userRepository.GetUsersAsync();
        
        IEnumerable<MemberDto> usersToReturn = _mapper.Map<IEnumerable<MemberDto>>(users);

        return Ok(usersToReturn);

    }

    // [HttpGet("{id}")] // https://api/users/2
    // public async Task<ActionResult<AppUser>> GetUser(int id)
    // {
    //     return await _userRepository.GetUserByIdAsync(id);
    // }

    [HttpGet("{username}")] // https://api/users/username
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        AppUser user = await _userRepository.GetUserbyNameAsync(username);
        
        return _mapper.Map<MemberDto>(user);
    }
}
