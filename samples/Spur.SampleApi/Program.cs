using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Spur.AspNetCore;
using Spur.SampleApi.Features.Users;
using Spur.SampleApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=sampleapi.db"));

builder.Services.AddSpur(options =>
{
    options.ProblemDetailsTypeBaseUri = "https://api.example.com/errors/";
    options.IncludeErrorCode = true;
    options.IncludeErrorCategory = true;
    options.IncludeInnerErrors = true;
});

// Register FluentValidation validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Map endpoints
var users = app.MapGroup("/users").WithTags("Users");

users.MapGet("/", ListUsers.Handle).WithName("ListUsers");
users.MapGet("/{id:int}", GetUser.Handle).WithName("GetUser");
users.MapPost("/", CreateUser.Handle).WithName("CreateUser");
users.MapPut("/{id:int}", UpdateUser.Handle).WithName("UpdateUser");
users.MapDelete("/{id:int}", DeleteUser.Handle).WithName("DeleteUser");

app.Run();
