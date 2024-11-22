using System;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers;

// This is an Action filter to save LastActive datetime of a user 
public class LogUserActivity : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // To perform task after Action Execution
        var resultContext = await next();

        if(!resultContext.HttpContext.User.Identity.IsAuthenticated)
            return;

        // Get Username from Httpcontext (System.Security.Claims.ClaimsPrincipal)
        var userId = resultContext.HttpContext.User.GetUserId();
        // Get UserRepository service to make change to User in DB
        var uow = resultContext.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();

        //Get User Entity
        var user = await uow.UserRepository.GetUserByIdAsync(userId);
        // Set Last Active date
        user.LastActive = DateTime.UtcNow;
        // Save it in DB
        await uow.Complete();
    }
}
