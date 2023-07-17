using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace AjaxFavoriteRssCookies.Pages;
public class FavoritePostsModel : PageModel
{
    private readonly IMemoryCache _cache;

    public List<MainFeedItem> StarredFeeds { get; set; } = new List<MainFeedItem>();
    public int PageSize { get; set; } = 10;
    public int PageNumber { get; set; } = 1;
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; } = 1;

    public FavoritePostsModel(IMemoryCache cache)
    {
        _cache = cache;
    }

    public IActionResult OnGet(int pageNumber = 1)
    {
        PageNumber = pageNumber;
        var favoriteFeedsCookie = HttpContext.Request.Cookies["FavoriteFeeds"];

        if (!string.IsNullOrEmpty(favoriteFeedsCookie))
        {
            var favoriteFeeds = favoriteFeedsCookie.Split(',').ToList();
            StarredFeeds = GetStarredFeeds(favoriteFeeds);
        }

        TotalItems = StarredFeeds.Count;
        TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
        CurrentPage = pageNumber;

        StarredFeeds = StarredFeeds
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        return Page();
    }

    private List<MainFeedItem> GetStarredFeeds(List<string> favoriteFeeds)
    {
        var cachedFeedsJson = _cache.Get<string>("starredFeeds");
        if (!string.IsNullOrEmpty(cachedFeedsJson))
        {
            var allFeeds = JsonSerializer.Deserialize<List<MainFeedItem>>(cachedFeedsJson);
            if (allFeeds != null)
            {
                var starredFeeds = allFeeds.Where(feed => feed.XmlUrl != null && favoriteFeeds.Contains(feed.XmlUrl)).ToList();
                starredFeeds = starredFeeds.OrderBy(feed => favoriteFeeds.IndexOf(feed.XmlUrl)).ToList();
                return starredFeeds;
            }
        }

        return new List<MainFeedItem>();
    }
}
