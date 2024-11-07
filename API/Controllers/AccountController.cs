using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace API.Controllers;

// api/account
public class AccountController: BaseApiController
{
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
    {
        _context = context;
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

        using var hmac = new HMACSHA512();

        user.UserName = registerDto.UserName.ToLower();
        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
        user.PasswordSalt = hmac.Key;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

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
        var user = await _context.Users
        .Include(u => u.Photos) // for eager loading of related entity - Photos
        .SingleOrDefaultAsync(x => x.UserName == loginDto.UserName.ToLower());

        if(user == null)
            return Unauthorized("invalid username");

        // Use the Matching User's Password Salt from Database to create HMAC obj
        using var hmac = new HMACSHA512(user.PasswordSalt);
        // Get Hash of the Password from Login Screen
        var computedhash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        // Compare both Hash byte arrays
        for(int i=0; i< computedhash.Length; i++)
        {
            if(computedhash[i] != user.PasswordHash[i])
                return Unauthorized("invalid password");
        }

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
        return await _context.Users.AnyAsync(appuser => appuser.UserName == username.ToLower());
    }
}