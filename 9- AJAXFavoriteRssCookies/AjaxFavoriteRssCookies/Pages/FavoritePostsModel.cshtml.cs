using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AjaxFavoriteRssCookies.Pages
{
    public class FavoritePostsModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        public List<string> StarredFeeds { get; set; } = new List<string>();
        public List<MainFeedItem> StarredFeedsList { get; set; } = new List<MainFeedItem>();
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; } = 1;

        public FavoritePostsModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
        {
            PageNumber = pageNumber;
            StarredFeeds = GetStarredFeedsFromCookie();

            TotalItems = StarredFeeds.Count;
            TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
            CurrentPage = pageNumber;

            // Fetch the full content from the server asynchronously
            var opmlContent = await FetchMainFeedContent();
            var StaredContentCollection = ParseMainFeedContent(opmlContent);

            // Filter the full content based on the starred feeds
            StarredFeedsList = StaredContentCollection
                .Where(item => StarredFeeds.Contains(item.XmlUrl))
                .OrderBy(item => StarredFeeds.IndexOf(item.XmlUrl)) // Order by the index of the XmlUrl in the StarredFeeds list
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return Page();
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

            return doc.Descendants("outline")
                .Where(o => o.Attribute("type")?.Value == "rss")
                .Select(o => new MainFeedItem
                {
                    Id = int.Parse(o.Attribute("id")?.Value ?? "0"),
                    Text = o.Attribute("text")?.Value,
                    XmlUrl = o.Attribute("xmlUrl")?.Value,
                    HtmlUrl = o.Attribute("htmlUrl")?.Value,
                })
                .ToList();
        }

        private List<string> GetStarredFeedsFromCookie()
        {
            var starredFeedsCookie = HttpContext.Request.Cookies["FavoriteFeeds"];
            if (!string.IsNullOrEmpty(starredFeedsCookie))
            {
                return starredFeedsCookie.Split(',').ToList();
            }
            return new List<string>();
        }
    }
}
