using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SignalR;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

// Prady - Made it static
public static class ApplicationServiceExtensions
{
    // Prady - Using the 'this' keyword in the first argument below, it indicates that it is an Extension method of IServiceCollection type
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddCors();
        services.AddDbContext<DataContext>(opt => 
        {
            // opt.UseSqlite(config.GetConnectionString("DefaultConnection"));
            opt.UseNpgsql(config.GetConnectionString("DefaultConnection"));
        });
        services.AddScoped<ITokenService, TokenService>(); // Added custom built service to creating user tokens
        // services.AddScoped<IUserRepository, UserRepository>(); // Added to implement repository pattern, to inject repository layer in Controller Layer
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies()); // To inject AutoMapper service into Controllers
        services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings")); // Added to pull Cloudinary (3rd Party API for Images) settings from AppSettings.JSON
        services.AddScoped<IPhotoService, PhotoService>(); // Added service to Add/Delete photos using Cloudinary
        services.AddScoped<LogUserActivity>(); // Added Service to save LastActive datetime of a user 
        // services.AddScoped<ILikesRepository, LikesRepository>(); // added to implement Likes functionality
        // services.AddScoped<IMessageRepository, MessageRepository>(); // added to implement Messages functionality
        services.AddScoped<IUnitOfWork, UnitOfWork>(); // Added to implement Unit of work pattern, instead of Individual Repositories to save changes
        services.AddSignalR();
        services.AddSingleton<PresenceTracker>(); // Added as Singleton because we want this to be accessible to all the users connected to server, to show online presence

        return services;
    }
}
