using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        // Return if there are any Users already in DB
        if(await userManager.Users.AnyAsync())
            return;

        // Read all content in a String from User Data file
        var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");

        // Create object for Deserializing OPTIONS
        var options = new JsonSerializerOptions{PropertyNameCaseInsensitive = true};
        // Deserialize from json String to C# object
        var users = JsonSerializer.Deserialize<List<AppUser>>(userData, options);
        // Create roles
        var roles = new List<AppRole>
        {
            new AppRole{Name = "Member"},
            new AppRole{Name = "Admin"},
            new AppRole{Name = "Moderator"}
        };

        // Sabe all roles in DB
        foreach(var role in roles)
        {
            await roleManager.CreateAsync(role);
        }

        // Get UserName and Generate Password for each user and add it to DB context
        foreach(var user in users)
        {
            user.UserName = user.UserName.ToLower();

            await userManager.CreateAsync(user, "Pa$$w0rd");
            // map to a role
            await userManager.AddToRoleAsync(user, "Member");
        }

        // Create Admin user
        var admin = new AppUser{
            UserName = "admin"
        };
        // save admin in DB
        await userManager.CreateAsync(admin, "Pa$$w0rd");
        // Map multiple roles to admin
        await userManager.AddToRolesAsync(admin, new[] {"Admin", "Moderator"});
    }
}
