﻿using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
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
            opt.UseSqlite(config.GetConnectionString("DefaultConnection"));
        });
        services.AddScoped<ITokenService, TokenService>(); // Added custom built service to creating user tokens
        services.AddScoped<IUserRepository, UserRepository>(); // Added to implement repository pattern, to inject repository layer in Controller Layer
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies()); // To inject AutoMapper service into Controllers
        services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings")); // Added to pull Cloudinary (3rd Party API for Images) settings from AppSettings.JSON
        services.AddScoped<IPhotoService, PhotoService>(); // Added service to Add/Delete photos using Cloudinary

        return services;
    }
}
