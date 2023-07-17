using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;

namespace AjaxFavoriteRssCookies.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        public List<MainFeedItem> ContentCollection { get; set; } = new List<MainFeedItem>();
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; } = 1;

        public IndexModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }
        public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
        {
            PageNumber = pageNumber;
            var opmlContent = await FetchMainFeedContent();
            ContentCollection = ParseMainFeedContent(opmlContent);

            TotalItems = ContentCollection.Count;
            TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
            CurrentPage = PageNumber;

            // var starredFeedsJson = JsonSerializer.Serialize(ContentCollection);
            // SetStarredFeedsInCookie(starredFeedsJson);

            ContentCollection = ContentCollection
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return Page();
        }

        public IActionResult OnPostToggleFavorite(string link, int pageNumber)
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

            SetFavoriteFeedsInCookie(favoriteFeeds);

            var isFavorite = favoriteFeeds.Contains(link);

            return new JsonResult(new { IsFavorite = isFavorite });
        }

        private async Task<string?> FetchMainFeedContent()
        {
             var client = _clientFactory.CreateClient();
            var response = await client.GetAsync("https://blue.feedland.org/opml?screenname=dave");

            if (response.IsSuccessStatusCode)
            {
                var opmlContent = await response.Content.ReadAsStringAsync();
                return opmlContent;
            }

            return string.Empty; // Return a default value in case of error
        }

        private List<MainFeedItem> ParseMainFeedContent(string? opmlContent)
        {
            if (string.IsNullOrEmpty(opmlContent))
            {
                return new List<MainFeedItem>();
            }

            var doc = XDocument.Parse(opmlContent);

            var favoriteFeeds = GetFavoriteFeedsFromCookie();

            return doc.Descendants("outline")
                .Where(o => o.Attribute("type")?.Value == "rss")
                .Select(o => new MainFeedItem
                {
                    Id = int.Parse(o.Attribute("id")?.Value ?? "0"),
                    Text = o.Attribute("text")?.Value,
                    XmlUrl = o.Attribute("xmlUrl")?.Value,
                    HtmlUrl = o.Attribute("htmlUrl")?.Value,
                    IsFavorite = favoriteFeeds.Contains(o.Attribute("xmlUrl")?.Value ?? string.Empty)
                })
                .ToList();
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

        private void SetFavoriteFeedsInCookie(List<string> favoriteFeeds)
        {
            var favoriteFeedsCookieValue = string.Join(',', favoriteFeeds);
            HttpContext.Response.Cookies.Append("FavoriteFeeds", favoriteFeedsCookieValue);
        }

        // private void SetStarredFeedsInCookie(string starredFeedsJson)
        // {
        //     HttpContext.Response.Cookies.Append("starredFeeds", starredFeedsJson);
        // }
    }

    public class MainFeedItem
    {
        public int Id { get; set; }
        public string? Text { get; set; }
        public string? XmlUrl { get; set; }
        public string? HtmlUrl { get; set; }
        public bool IsFavorite { get; set; }
    }
}



// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.RazorPages;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.Json;
// using System.Xml.Linq;

// namespace AjaxFavoriteRssCookies.Pages
// {
//     public class IndexModel : PageModel
//     {
//         private readonly IHttpClientFactory _clientFactory;
//         public List<MainFeedItem> ContentCollection { get; set; } = new List<MainFeedItem>();
//         public int PageSize { get; set; } = 10;
//         public int PageNumber { get; set; } = 1;
//         public int TotalItems { get; set; }
//         public int TotalPages { get; set; }
//         public int CurrentPage { get; set; } = 1;

//         public IndexModel(IHttpClientFactory clientFactory)
//         {
//             _clientFactory = clientFactory;
//         }

//         public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
//         {
//             PageNumber = pageNumber;
//             var opmlContent = await FetchMainFeedContent();
//             ContentCollection = ParseMainFeedContent(opmlContent);

//             TotalItems = ContentCollection.Count;
//             TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
//             CurrentPage = PageNumber;

//             var starredFeedsJson = JsonSerializer.Serialize(ContentCollection);
//             SetStarredFeedsInCookie(starredFeedsJson);

//             ContentCollection = ContentCollection
//                 .Skip((PageNumber - 1) * PageSize)
//                 .Take(PageSize)
//                 .ToList();

//             return Page();
//         }

//         public IActionResult OnPostToggleFavorite(string link, int pageNumber)
//         {
//             var favoriteFeeds = GetFavoriteFeedsFromCookie();

//             if (favoriteFeeds.Contains(link))
//             {
//                 favoriteFeeds.Remove(link);
//             }
//             else
//             {
//                 favoriteFeeds.Add(link);
//             }

//             SetFavoriteFeedsInCookie(favoriteFeeds);

//             var isFavorite = favoriteFeeds.Contains(link);

//             return new JsonResult(new { IsFavorite = isFavorite });
//         }

//         private async Task<string?> FetchMainFeedContent()
//         {
//             var client = _clientFactory.CreateClient();
//             var response = await client.GetAsync("https://blue.feedland.org/opml?screenname=dave");

//             if (response.IsSuccessStatusCode)
//             {
//                 var opmlContent = await response.Content.ReadAsStringAsync();
//                 return opmlContent;
//             }

//             return string.Empty; // Return a default value in case of error
//         }

//         private List<MainFeedItem> ParseMainFeedContent(string? opmlContent)
//         {
//             if (string.IsNullOrEmpty(opmlContent))
//             {
//                 return new List<MainFeedItem>();
//             }

//             var doc = XDocument.Parse(opmlContent);

//             var favoriteFeeds = GetFavoriteFeedsFromCookie();

//             return doc.Descendants("outline")
//                 .Where(o => o.Attribute("type")?.Value == "rss")
//                 .Select(o => new MainFeedItem
//                 {
//                     Id = int.Parse(o.Attribute("id")?.Value ?? "0"),
//                     Text = o.Attribute("text")?.Value,
//                     XmlUrl = o.Attribute("xmlUrl")?.Value,
//                     HtmlUrl = o.Attribute("htmlUrl")?.Value,
//                     IsFavorite = favoriteFeeds.Contains(o.Attribute("xmlUrl")?.Value ?? string.Empty)
//                 })
//                 .ToList();
//         }

//         private List<string> GetFavoriteFeedsFromCookie()
//         {
//             var favoriteFeedsCookie = HttpContext.Request.Cookies["FavoriteFeeds"];
//             if (!string.IsNullOrEmpty(favoriteFeedsCookie))
//             {
//                 return favoriteFeedsCookie.Split(',').ToList();
//             }
//             return new List<string>();
//         }

//         private void SetFavoriteFeedsInCookie(List<string> favoriteFeeds)
//         {
//             var feedObjects = ContentCollection
//                 .Where(feed => favoriteFeeds.Contains(feed.XmlUrl))
//                 .Select(feed => new { XmlUrl = feed.XmlUrl, Text = feed.Text });

//             var feedJson = JsonSerializer.Serialize(feedObjects);

//             HttpContext.Response.Cookies.Append("FavoriteFeeds", feedJson);
//         }

//         private void SetStarredFeedsInCookie(string starredFeedsJson)
//         {
//             HttpContext.Response.Cookies.Append("starredFeeds", starredFeedsJson);
//         }
//     }

//     public class MainFeedItem
//     {
//         public int Id { get; set; }
//         public string? Text { get; set; }
//         public string? XmlUrl { get; set; }
//         public string? HtmlUrl { get; set; }
//         public bool IsFavorite { get; set; }
//     }
// }
