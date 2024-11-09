using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class LikesController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly ILikesRepository _likesRepository;

    public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
    {
        _userRepository = userRepository;
        _likesRepository = likesRepository;
    }

    [HttpPost("{username}")]
    public async Task<ActionResult> AddLike(string username)
    {
        // Get the User which the loggedin User wants to like
        var likedUser = await _userRepository.GetUserbyNameAsync(username);
        // Get the currently logged in userID 
        var sourceUserId = User.GetUserId();
        // Get loggedin user from Likes table, to get the list of users this user already likes 
        var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId);

        // If the User to be liked not found
        if(likedUser == null)
            return NotFound();
        
        // If the User to be liked is same as the currently logged in user
        if(username == sourceUser.UserName)
            return BadRequest("You cannot like yourself.");
        
        // Check If the user is already liked
        var userLike = await _likesRepository.GetUserLike(sourceUserId, likedUser.Id);
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
        if(await _userRepository.SaveAllAsync())
            return Ok();

        // Else, return BadRequest
        return BadRequest("Failed to like user");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes(string predicate)
    {
        var users = await _likesRepository.GetUserLikes(predicate, User.GetUserId());

        return Ok(users);
    }
}
