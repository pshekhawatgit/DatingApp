using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using CloudinaryDotNet.Actions;
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
    private readonly IPhotoService _photoService;

    public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _photoService = photoService;
    }

    [HttpGet] // https://api/users
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
        IEnumerable<MemberDto> users = await _userRepository.GetMembersAsync();
        
        return Ok(users);

    }

    // [HttpGet("{id}")] // https://api/users/2
    // public async Task<ActionResult<AppUser>> GetUser(int id)
    // {
    //     return await _userRepository.GetUserByIdAsync(id);
    // }

    [HttpGet("{username}")] // https://api/users/username
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
       return await _userRepository.GetMemberAsync(username);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var user = await _userRepository.GetUserbyNameAsync(User.GetUsername());

        if(user == null) return NotFound();

        _mapper.Map(memberUpdateDto, user);

        if(await _userRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to update user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await _userRepository.GetUserbyNameAsync(User.GetUsername());

        if(user == null)
            return NotFound();

        ImageUploadResult result = await _photoService.UploadImageAsync(file);
        
        if(result.Error != null)
            return BadRequest(result.Error.Message);

        Photo photo = new Photo{
            Url = result.Url.AbsoluteUri,
            PublicId = result.PublicId
        };

        if(user.Photos.Count == 0)
            photo.IsMain = true;

        user.Photos.Add(photo);

        if(await _userRepository.SaveAllAsync())
        {
            //return _mapper.Map<PhotoDto>(photo);
            return CreatedAtAction(nameof(GetUser), new {userName = user.UserName}, _mapper.Map<PhotoDto>(photo));
        }
        
        return BadRequest("Problems adding photo");
    }
}
