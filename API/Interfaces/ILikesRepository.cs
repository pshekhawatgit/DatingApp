using System;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface ILikesRepository
{
    Task<UserLike> GetUserLike(int sourceUserId, int targetUserId);
    Task<AppUser> GetUserWithLikes(int userId);
    
    // Method to get User likes based on the passed predicate to 
    // implement whether to retun either the user they like, or the user they are liked by. 
    Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams);
}
