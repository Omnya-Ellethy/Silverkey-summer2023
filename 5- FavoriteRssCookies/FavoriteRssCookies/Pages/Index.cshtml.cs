using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System.Xml.Linq;

namespace FavoriteRssCookies.Pages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IMemoryCache _cache;

    public List<MainFeedItem> ContentCollection { get; set; } = new();
    public int PageSize { get; set; } = 10;
    public int PageNumber { get; set; } = 1;
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; } = 1;

    public IndexModel(IHttpClientFactory clientFactory, IMemoryCache cache)
    {
        _clientFactory = clientFactory;
        _cache = cache;
    }

    public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
    {
        PageNumber = pageNumber;
        var opmlContent = await FetchMainFeedContent();
        ContentCollection = ParseMainFeedContent(opmlContent);

        TotalItems = ContentCollection.Count;
        TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
        CurrentPage = PageNumber;

        ContentCollection = ContentCollection
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList();
            
        return Page();
    }
    
    private async Task<string?> FetchMainFeedContent()
    {
        if (!_cache.TryGetValue("OpmlContent", out string? opmlContent))
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync("https://blue.feedland.org/opml?screenname=dave");

            if (response.IsSuccessStatusCode)
            {
                opmlContent = await response.Content.ReadAsStringAsync();
                _cache.Set("OpmlContent", opmlContent, TimeSpan.FromHours(1));
            }
            else
            {
                return string.Empty; // Return a default value in case of error
            }
        }

        return opmlContent ?? string.Empty;
    }

    private List<MainFeedItem> ParseMainFeedContent(string? opmlContent)
    {
        if (string.IsNullOrEmpty(opmlContent))
        {
            return new List<MainFeedItem>();
        }

        var doc = XDocument.Parse(opmlContent);

        return doc.Descendants("outline")
            .Where(o => o.Attribute("type")?.Value == "rss")
            .Select(o => new MainFeedItem
            {
                Id = int.Parse(o.Attribute("id")?.Value ?? "0"),
                Text = o.Attribute("text")?.Value,
                XmlUrl = o.Attribute("xmlUrl")?.Value,
                HtmlUrl = o.Attribute("htmlUrl")?.Value,
                IsFavorite =  GetFavoriteFeedsFromCookie().Contains(o.Attribute("xmlUrl")?.Value ?? string.Empty)
            })
            .ToList();
    }
    public async Task<IActionResult> OnPostToggleFavorite(string link, int pageNumber)
    {
        var favoriteFeeds = GetFavoriteFeedsFromCookie();

        if (favoriteFeeds.Contains(link))
        {
            favoriteFeeds.Remove(link);
        }
        else
        {
            favoriteFeeds.Add(link);
        }

        await SetFavoriteFeedsInCookie(favoriteFeeds);

        return RedirectToPage("/Index", new { pageNumber });
    }

    private List<string> GetFavoriteFeedsFromCookie()
    {
        var favoriteFeedsCookie = HttpContext.Request.Cookies["FavoriteFeeds"];
        if (!string.IsNullOrEmpty(favoriteFeedsCookie))
        {
            return favoriteFeedsCookie.Split(',').ToList();
        }
        return new List<string>();
    }
    private Task SetFavoriteFeedsInCookie(List<string> favoriteFeeds)
    {
        var favoriteFeedsCookieValue = string.Join(',', favoriteFeeds);
        HttpContext.Response.Cookies.Append("FavoriteFeeds", favoriteFeedsCookieValue);

        return Task.CompletedTask;
    }
}

public class MainFeedItem
{
    public int Id { get; set; }
    public string? Text { get; set; }
    public string? XmlUrl { get; set; }
    public string? HtmlUrl { get; set; }
    public bool IsFavorite { get; set; }
}

