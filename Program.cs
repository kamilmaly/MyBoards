using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using MyBoards.Dto;
using MyBoards.Entities;
using MyBoards.Migrations;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddDbContext<MyBoardsContext>(
        option => option
        // .UseLazyLoadingProxies()
        .UseSqlServer(builder.Configuration.GetConnectionString("MyBoardsConnectionString")));
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
    .Select(g => new { g.Key, Count = g.Count() })
    .ToListAsync();

    var topAuthor = authorsCommentCounts
    .First(a => a.Count == authorsCommentCounts.Max(acc => acc.Count));

    var userDetails = db.Users.First(u => u.Id == topAuthor.Key);

    return new { userDetails, commentCount = topAuthor.Count };


});

app.MapGet("selectData", async (MyBoardsContext db) =>
{
    var userFullNames = await db.Users
        .Include(u => u.Address)
        .Include(u => u.Comments)
        .Where(u => u.Address.Country == "Albania")
        .SelectMany(u => u.Comments)
        .Select(c => c.Message)
        .ToListAsync();

    return userFullNames;
});
app.MapGet("datanplus1", async (MyBoardsContext db) =>
{
    var users = await db.Users
    .Include(u => u.Address)
    .Include(u => u.Comments)
    .Where(u=> u.Address.Country == "Albania")
    .ToListAsync();

    foreach (var user in users)
    {
        var usercomments = user.Comments;
        foreach (var comment in usercomments)
        {
            //Process (comment);
        }
    }
});


app.MapGet("dataSqlRaw", async (MyBoardsContext db) =>
{
    var minWorkItemsCount = "85";

    //    var states = db.WorkItemStates
    //    .FromSqlRaw(@"
    //SELECT wis.Id,wis.Value
    //FROM WorkItemStates wis
    //JOIN WorkItems wi on wi.StateId = wis.Id
    //GROUP BY wis.Id,wis.Value
    //HAVING COUNT(*) > 85
    //")
    var states = db.WorkItemStates
    .FromSqlInterpolated($@"
SELECT wis.Id,wis.Value
FROM WorkItemStates wis
JOIN WorkItems wi on wi.StateId = wis.Id
GROUP BY wis.Id,wis.Value
HAVING COUNT(*) > {minWorkItemsCount}")
    .ToList();

    db.Database.ExecuteSqlRaw(@"
UPDATE Comments
SET UpdatedDate = GETDATE()
WHERE AuthorId = '6EB04543-F56B-4827-CC11-08DA10AB0E61'");

    var entries = db.ChangeTracker.Entries();
    return states;

});

app.MapGet("dataViewKeyLess", async (MyBoardsContext db) =>
{
    var topAuthors = db.ViewTopAuthors.ToList();
    return topAuthors;
});

app.MapGet("pagination", async (MyBoardsContext db) =>
{
    var filter = "a";
    string sortBy = "FullName";
    bool sortByDescending = false;
    int pageNumber = 1;
    int pageSize = 10;

    var query = db.Users
            .Where(u => filter == null ||
            (u.Email.Contains(filter.ToLower()) || u.FullName.Contains(filter.ToLower())));
    var totalCount = query.Count();

    if (sortBy != null)
    {
        var columnsSelector = new Dictionary<string, Expression<Func<User, object>>>
        {
            {nameof(User.Email), user => user.Email },
            {nameof(User.FullName), user => user.FullName },
        };

        var sortByExpression = columnsSelector[sortBy];
        query = sortByDescending
            ? query.OrderByDescending(sortByExpression)
            : query.OrderBy(sortByExpression);
    }

    var result = query.Skip(pageSize * (pageNumber - 1))
        .Take(pageSize)
        .ToList();

    var pagedResult = new PagedResult<User>(result, totalCount, pageSize, pageNumber);

    return pagedResult;
});


app.MapGet("dataLazyLoading", async (MyBoardsContext db) =>
{
    var withAddress = true;

    var user = db.Users
        .First(u => u.Id == Guid.Parse("E72E6A2E-40A2-41AA-CBC0-08DA10AB0E61"));

    if (withAddress)
    {
        var result = new { FullName = user.FullName, Address = $"{user.Address.Street} {user.Address.City}" };
        return result;
    }

    return new { FullName = user.FullName, Address = "-" };
});

app.MapGet("changeTracker", async (MyBoardsContext db) =>
{
    var user = await db.Users
    .FirstAsync(u => u.Id == Guid.Parse("8ACE902E-A25C-4168-CBC1-08DA10AB0E61"));

    var entries1 = db.ChangeTracker.Entries();

    user.Email = "testttest@test.com";

    var entries2 = db.ChangeTracker.Entries();

    db.SaveChanges();

    return user;

});

app.MapGet("changeTrackerDeleted", async (MyBoardsContext db) =>
{
    var workItem = new Epic()
    {
        Id = 2
    };

    var entry = db.Attach(workItem);
    entry.State = EntityState.Deleted;

    db.SaveChanges();
    return workItem;

});

app.MapGet("noTracking", async (MyBoardsContext db) =>
{
    var states = db.WorkItemStates
    .AsNoTracking()
    .ToList();

    var entries1 = db.ChangeTracker.Entries();

    return states;

});


app.MapGet("getUserComments", async (MyBoardsContext db) =>
{
    var user = await db.Users
      .Include(u => u.Comments).ThenInclude(c => c.WorkItem)
      .Include(u => u.Address)
      .FirstAsync(u => u.Id == Guid.Parse("68366DBE-0809-490F-CC1D-08DA10AB0E61"));

    return user;
});

app.MapPost("update", async (MyBoardsContext db) =>
{
    Epic epic = await db.Epics.FirstAsync(epic => epic.Id == 1);

    var rejectedState = await db.WorkItemStates
    .FirstAsync(s => s.Value == "Rejected");

    epic.State = rejectedState;

    await db.SaveChangesAsync();

    return epic;
});

app.MapPost("createTags", async (MyBoardsContext db) =>
{
    Tag mvctag = new Tag()
    {
        Value = "MVC"
    };

    Tag asptag = new Tag()
    {
        Value = "ASP"
    };
    var tags = new List<Tag>() { mvctag, asptag };
    //await db. AddAsync(tag);
    //await db.Tags.AddAsync(mvctag);
    await db.Tags.AddRangeAsync(tags);
    await db.SaveChangesAsync();

    return tags;
});

app.MapPost("createUsers", async (MyBoardsContext db) =>
{
    var address = new Address()
    {
        Id = Guid.Parse("74A154C0-69B6-4B1F-47C1-08DA10AB0E33"),
        Country = "Brazil",
        City = "Bako",
        Street = "Bako street",
    };

    var user = new User()
    {
        Email = "tot@test.com",
        FullName = "Test User",
        Address = address,
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return user;
});

app.MapDelete("deleteWorkItemTags", async (MyBoardsContext db) =>
{
    var workItemTags = await db.WorkItemTag.Where(c => c.WorkItemId == 12).ToListAsync();
    db.WorkItemTag.RemoveRange(workItemTags);

    var workItem = await db.WorkItems.FirstAsync(c => c.Id == 16);

    db.RemoveRange(workItem);

    await db.SaveChangesAsync();
});

app.MapDelete("deleteUser", async (MyBoardsContext db) =>
{
    var user = await db.Users
    .Include(u => u.Comments)
    .FirstAsync(u => u.Id == Guid.Parse("6D834BAE-67FE-4A1C-CBD8-08DA10AB0E61"));
    //var userComments = await db.Comments.Where(c => c.AuthorId == user.Id).ToListAsync();

    //db.Comments.RemoveRange(userComments);
    //await db.SaveChangesAsync();

    db.Users.Remove(user);
    await db.SaveChangesAsync();
});

app.Run();
