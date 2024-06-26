using API;
using API.Data;
using API.Extensions;
using API.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration); // Prady - Calling code from custom built Extension method
builder.Services.AddIdentityServices(builder.Configuration); // Prady - Calling code from custom built Extension method
// // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

// app.UseAuthorization();
// Configure the HTTP request pipeline
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200"));
// Prady: Intentionally added between UseCors and MapControllers
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed Data in DB for tests
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
     var context = services.GetRequiredService<DataContext>();
     await context.Database.MigrateAsync();
     await Seed.SeedUsers(context);
}
catch(Exception ex)
{
    var logger = services.GetService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration");
}
app.Run();
