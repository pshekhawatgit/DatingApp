using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class LikesController : BaseApiController
{
    private readonly IUnitOfWork _uow;

    public LikesController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpPost("{username}")]
    public async Task<ActionResult> AddLike(string username)
    {
        // Get the User which the loggedin User wants to like
        var likedUser = await _uow.UserRepository.GetUserbyNameAsync(username);
        // Get the currently logged in userID 
        var sourceUserId = User.GetUserId();
        // Get loggedin user from Likes table, to get the list of users this user already likes 
        var sourceUser = await _uow.LikesRepository.GetUserWithLikes(sourceUserId);

        // If the User to be liked not found
        if(likedUser == null)
            return NotFound();
        
        // If the User to be liked is same as the currently logged in user
        if(username == sourceUser.UserName)
            return BadRequest("You cannot like yourself.");
        
        // Check If the user is already liked
        var userLike = await _uow.LikesRepository.GetUserLike(sourceUserId, likedUser.Id);
        if(userLike != null)
            return BadRequest("You already like this user");

        // Prepare an Entry to be added in LIKES table
        userLike = new UserLike{
            SourceUserId = sourceUserId,
            TargetUserId = likedUser.Id
        };
        // Add the Like Entry into Liked Users of LoggedIn user  
        if(sourceUser.LikedUsers != null)
            sourceUser.LikedUsers.Add(userLike);
        else
            sourceUser.LikedUsers = new List<UserLike>{ userLike };


        // Save entry in Database
        if(await _uow.Complete())
            return Ok();

        // Else, return BadRequest
        return BadRequest("Failed to like user");
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
    {
        likesParams.UserId = User.GetUserId();
        var users = await _uow.LikesRepository.GetUserLikes(likesParams);
        Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

        return Ok(users);
    }
}
