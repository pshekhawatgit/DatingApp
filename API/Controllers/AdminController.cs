using System;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController : BaseApiController
{
    private readonly UserManager<AppUser> _userManager;

    public AdminController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.UserName)
            .Select(u => new // return a new Object with Properties we want to return
            {
                u.Id,
                Username = u.UserName,
                Roles = u.UserRoles.Select(ur => ur.Role.Name) // We only need Role Names
            }).ToListAsync();

        return Ok(users);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRoles(string username, [FromQuery]string roles)
    {
        // Get user
        var user = await _userManager.FindByNameAsync(username);
        if(user == null)
            return NotFound();

        // selected roles from Query String
        if(String.IsNullOrEmpty(roles))
            BadRequest("You must select at least one role");
        var selectedRoles = roles.Split(',').ToArray();     

        // Get current roles already in DB
        var userRoles = await _userManager.GetRolesAsync(user);

        // Add selected new roles to User Roles in DB(Except the ones already there)
        var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
        if(!result.Succeeded)
            BadRequest("Failed to add to roles");
        
        // Remove Existing Roles from User Roles in DB (Except the new ones selected)
        result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
        if(!result.Succeeded)
            BadRequest("Failed to remove from roles");

        // Return the updated Roles list
        return Ok(await _userManager.GetRolesAsync(user)); 
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public ActionResult GetPhotosForModeration()
    {
        return Ok("If you see this, you are either an Admin or a Moderator");
    }
}
