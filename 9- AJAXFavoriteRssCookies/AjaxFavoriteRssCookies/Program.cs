using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

// Map the "/togglefavorite" endpoint for handling the POST request
app.MapPost("/togglefavorite", async (HttpContext context) =>
{
    var link = context.Request.Form["link"];
    var pageNumber = int.Parse(context.Request.Form["pageNumber"]);

    var favoriteFeeds = GetFavoriteFeedsFromCookie(context);

    if (favoriteFeeds.Contains(link))
    {
        favoriteFeeds.Remove(link);
    }
    else
    {
        favoriteFeeds.Add(link);
    }

    SetFavoriteFeedsInCookie(context, favoriteFeeds);

    var isFavorite = favoriteFeeds.Contains(link);

    await context.Response.WriteAsJsonAsync(new { IsFavorite = isFavorite });
});

app.Run();

static List<string> GetFavoriteFeedsFromCookie(HttpContext context)
{
    var favoriteFeedsCookie = context.Request.Cookies["FavoriteFeeds"];
    if (!string.IsNullOrEmpty(favoriteFeedsCookie))
    {
        return favoriteFeedsCookie.Split(',').ToList();
    }
    return new List<string>();
}

static void SetFavoriteFeedsInCookie(HttpContext context, List<string> favoriteFeeds)
{
    var favoriteFeedsCookieValue = string.Join(',', favoriteFeeds);
    context.Response.Cookies.Append("FavoriteFeeds", favoriteFeedsCookieValue);
}
