using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace API.Controllers;

// api/account
public class AccountController: BaseApiController
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    // Endpoint for  User to Register
    [HttpPost("register")] // POST: api/account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if(await UserExists(registerDto.UserName))
            return BadRequest("Username is taken");

        var user = _mapper.Map<AppUser>(registerDto);

        user.UserName = registerDto.UserName.ToLower();

        // _context.Users.Add(user);
        // await _context.SaveChangesAsync();
        // Save user in DB
        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if(!result.Succeeded)
            return BadRequest(result.Errors);

        return new UserDto{
            Username = user.UserName,
            Token = _tokenService.CreateToken(user),
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await _userManager.Users
        .Include(u => u.Photos) // for eager loading of related entity - Photos
        .SingleOrDefaultAsync(x => x.UserName == loginDto.UserName.ToLower());

        if(user == null)
            return Unauthorized("invalid username");

        // Check if valid password
        var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

        if(!result)
            return Unauthorized("Invalid password");

        return new UserDto{
            Username = user.UserName,
            Token = _tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain)?.Url,
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    private async Task<bool> UserExists(string username)
    {
        return await _userManager.Users.AnyAsync(appuser => appuser.UserName == username.ToLower());
    }
}