using Microsoft.EntityFrameworkCore;
using MyBoards.Entities;
using MyBoards.Migrations;
using System.Diagnostics.Metrics;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MyBoardsContext>(
        option => option.UseSqlServer(builder.Configuration.GetConnectionString("MyBoardsConnectionString")));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetService<MyBoardsContext>();

var pendingMigrations = dbContext.Database.GetPendingMigrations();
if (pendingMigrations.Any())
{
    dbContext.Database.Migrate();
}

var users = dbContext.Users.ToList();
if (!users.Any())
{
    var user1 = new User()
    {
        FullName = "David Tool",
        Email = "David@vp.pl",
        Address = new Address
        {
            Country = "England",
            City = "London",
            Street = "Green",
            PostalCode = "AK400"
        }
    };
    var user2 = new User()
    {
        FullName = "Pavlo Ther",
        Email = "pavlo@vp.pl",
        Address = new Address
        {
            Country = "Italy",
            City = "Mediolan",
            Street = "Perro",
            PostalCode = "GK300-30"
        }
    };

    dbContext.AddRange(user1, user2);
    dbContext.SaveChanges();
}

app.Run();
