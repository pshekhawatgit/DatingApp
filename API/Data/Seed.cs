using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsers(DataContext context)
    {
        // Return if there are any Users already in DB
        if(await context.Users.AnyAsync())
            return;

        // Read all content in a String from User Data file
        var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");

        // Create object for Deserializing OPTIONS
        var options = new JsonSerializerOptions{PropertyNameCaseInsensitive = true};
        // Deserialize from json String to C# object
        var users = JsonSerializer.Deserialize<List<AppUser>>(userData, options);
        // Get UserName and Generate Password for each user and add it to DB context
        foreach(var user in users)
        {
            user.UserName = user.UserName.ToLower();

            context.Users.Add(user);
        }
        // Save all users to Database
        await context.SaveChangesAsync();
    }
}
