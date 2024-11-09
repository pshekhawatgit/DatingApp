using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class LikesRepository : ILikesRepository
{
    private readonly DataContext _context;

    public LikesRepository(DataContext context)
    {
        _context = context;
    }
    /// <summary>
    /// Gets the entry from Likes table where entry for 
    /// User liking (source user) and User liked (target user) already exists
    /// </summary>
    /// <param name="sourceUserId">Id of User liking the target user</param>
    /// <param name="targetUserId">Id of User liked by the source user</param>
    /// <returns></returns>
    public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
    {
        return await _context.Likes.FindAsync(sourceUserId, targetUserId);
    }

    /// <summary>
    /// Gets a list of Users (LikeDTO) representing either the Liked Users or LikedBy Users, 
    /// depending on the input predicate
    /// </summary>
    /// <param name="predicate">predicate to specify either the Liked Users or LikedBy Users are to be returned</param>
    /// <param name="userId">User Id of logged in user</param>
    /// <returns></returns>
    public async Task<IEnumerable<LikeDto>> GetUserLikes(string predicate, int userId)
    {
        var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
        var likes = _context.Likes.AsQueryable();

        if(predicate == "liked")
        {
            likes = likes.Where(l => l.SourceUserId == userId);
            users = likes.Select(l => l.TargetUser);
        }

        if(predicate == "likedBy")
        {
            likes = likes.Where(l => l.TargetUserId == userId);
            users = likes.Select(l => l.SourceUser);
        }

        return await users.Select(user => new LikeDto{
            UserName = user.UserName,
            KnownAs = user.KnownAs,
            Age = user.DateOfBirth.CalculateAge(),
            PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
            City = user.City,
            Id = user.Id
        }).ToListAsync();
    }

    /// <summary>
    /// Gets the Loggedin User from Likes table
    /// Used when getting the user from Likes table to get all the users this user likes 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns>User</returns>
    public async Task<AppUser> GetUserWithLikes(int userId)
    {
        return await _context.Users
            .Include(u => u.LikedUsers)
            .FirstOrDefaultAsync(user => user.Id == userId);
    }
}
