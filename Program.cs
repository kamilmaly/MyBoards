using Microsoft.EntityFrameworkCore;
using MyBoards.Entities;
using MyBoards.Migrations;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;

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

app.MapGet("data", async (MyBoardsContext db) =>
{
    //var tags = db.Tags.ToList();
    //return tags;

    //var epic = db.Epics.First();
    //var user = db.Users.First(a => a.FullName == "Pavlo Ther");
    //return new {epic, user};

    //var toDoWorkItems = db.WorkItems.Where(w => w.StateId == 1).ToList();
    //return new { toDoWorkItems };

    //var newComments = await db
    //.Comments
    //.Where(c => c.CreatedDate > new DateTime(2022, 7, 23))
    //.ToListAsync();

    //return newComments;

    //var top5NewestComments = await db.Comments
    //.OrderByDescending(c => c.CreatedDate)
    //.Take(5)
    //.ToListAsync();

    //return top5NewestComments;

    //var statesCount = await db.WorkItems
    //.GroupBy(x => x.StateId)
    //.Select(g => new { stateId = g.Key, count = g.Count() })
    //.ToListAsync();

    //return statesCount;

    //var epicList = await db.Epics
    //.Where(e=>e.StateId == 4)
    //.OrderByDescending(w => w.Priority)
    //.ToListAsync();

    //return epicList;

    var authorsCommentCounts = await db.Comments
    .GroupBy(x => x.AuthorId)
    .Select(g => new {g.Key, Count = g.Count() })
    .ToListAsync();

    var topAuthor = authorsCommentCounts
    .First(a => a.Count == authorsCommentCounts.Max(acc => acc.Count));

    var userDetails = db.Users.First(u => u.Id == topAuthor.Key);

    return new { userDetails, commentCount = topAuthor.Count };


});

app.Run();
