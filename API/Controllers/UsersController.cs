﻿using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        // Get current user
        var user = await _userRepository.GetUserbyNameAsync(User.GetUsername());

        if(user == null)
            return NotFound();

        // Get selected photo of current user
        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

        if(photo == null)
            return NotFound();
        
        // Check if Photo is already main
        if(photo.IsMain)
            return BadRequest("This is already your main photo");

        // Set current main photo as Not main and Selected photo as main
        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

        if(currentMain != null)
            currentMain.IsMain = false;
        photo.IsMain = true;

        // Save changes to DB using Entity framework, return Nothing if saved
        if(await _userRepository.SaveAllAsync())
            return NoContent();
        // Else - return bad request
        return BadRequest("Problem setting the main photo");
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        // Get current user
        var user = await _userRepository.GetUserbyNameAsync(User.GetUsername());
        // Get photo to be deleted
        var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);
        // If photo not found
        if(photo == null)
            return NotFound();
        // if Main Photo
        if(photo.IsMain)
            return BadRequest("You cannot delete your main photo");
        // Delete photo
        if(photo.PublicId != null)
        {
            var result = await _photoService.DeleteImageAsync(photo.PublicId);
            // If any error while deleting
            if(result.Error != null)
                return BadRequest(result.Error.Message);
        }

        // Remove from User Photos list
        user.Photos.Remove(photo);
        // Save
        if(await _userRepository.SaveAllAsync())
            return Ok();

        return BadRequest("Problem deleting photo");
    }
}
