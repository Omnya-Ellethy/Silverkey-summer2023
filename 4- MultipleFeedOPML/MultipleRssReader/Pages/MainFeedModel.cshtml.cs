using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MultipleRssReader.Pages
{
    public class MainFeedModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMemoryCache _cache;

        public List<MainFeedItem> ContentCollection { get; set; } = new();
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
        public int TotalItems { get; set; } 
        public int TotalPages { get; set; } 
        public int CurrentPage { get; set; } = 1;


        public MainFeedModel(IHttpClientFactory clientFactory, IMemoryCache cache)
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

            return opmlContent ?? string.Empty; // Use null-coalescing operator to handle possible null value
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
                    Text = o.Attribute("text")?.Value,
                    XmlUrl = o.Attribute("xmlUrl")?.Value,
                    HtmlUrl = o.Attribute("htmlUrl")?.Value
                })
                .ToList();
        }
    }

    public class MainFeedItem
    {
        public string? Text { get; set; }
        public string? XmlUrl { get; set; }
        public string? HtmlUrl { get; set; }
    }
}
















































// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.RazorPages;
// using Microsoft.Extensions.Caching.Memory;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Net.Http;
// using System.Threading.Tasks;
// using System.Xml.Linq;

// namespace MultipleRssReader.Pages
// {
//     public class RssFeedModel : PageModel
//     {
//         private readonly IHttpClientFactory _clientFactory;
//         private readonly IMemoryCache _cache;

//         public List<RssItem> ContentCollection { get; set; } = new();
//         public int PageSize { get; set; } = 10;
//         public int PageNumber { get; set; } = 1;
//         public int TotalItems { get; set; }
//         public int TotalPages { get; set; }

//         public RssFeedModel(IHttpClientFactory clientFactory, IMemoryCache cache)
//         {
//             _clientFactory = clientFactory;
//             _cache = cache;
//         }

//         public async Task OnGetAsync(int pageNumber = 1)
//         {
//             PageNumber = pageNumber;

//             var feedUrls = await FetchFeedUrlsAsync();
//             var feeds = await FetchFeedsAsync(feedUrls);

//             TotalItems = feeds.Count;
//             TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);

//             ContentCollection = feeds
//                 .Skip((PageNumber - 1) * PageSize)
//                 .Take(PageSize)
//                 .ToList();
//         }

//         private async Task<List<string>> FetchFeedUrlsAsync()
//         {
//             if (!_cache.TryGetValue("FeedUrls", out List<string> feedUrls))
//             {
//                 var client = _clientFactory.CreateClient();
//                 var response = await client.GetAsync("https://blue.feedland.org/opml?screenname=dave");

//                 if (response.IsSuccessStatusCode)
//                 {
//                     var opmlContent = await response.Content.ReadAsStringAsync();
//                     var doc = XDocument.Parse(opmlContent);

//                     feedUrls = doc.Descendants("outline")
//                         .Where(o => o.Attribute("type")?.Value == "rss")
//                         .Select(o => o.Attribute("xmlUrl")?.Value)
//                         .Where(url => !string.IsNullOrEmpty(url))
//                         .ToList();

//                     _cache.Set("FeedUrls", feedUrls, TimeSpan.FromHours(1));
//                 }
//                 else
//                 {
//                     RedirectToPage("/Error");
//                 }
//             }

//             return feedUrls;
//         }

//         private async Task<List<RssItem>> FetchFeedsAsync(List<string> feedUrls)
//         {
//             var result = new List<RssItem>();
//             var client = _clientFactory.CreateClient();
//             var tasks = new List<Task>();

//             foreach (var url in feedUrls)
//             {
//                 tasks.Add(Task.Run(async () =>
//                 {
//                     var response = await client.GetAsync(url);

//                     if (response.IsSuccessStatusCode)
//                     {
//                         var xmlContent = await response.Content.ReadAsStringAsync();
//                         var doc = XDocument.Parse(xmlContent);

                        
//                             foreach (var item in doc.Descendants("item"))
//                             {
//                                 var link = item.Element("link")?.Value;
//                                 var description = item.Element("description")?.Value;
//                                 var pubDate = item.Element("pubDate")?.Value;

//                                 var rssItem = new RssItem
//                                 {
//                                     Description = description,
//                                     PubDate = pubDate,
//                                     Link = link
//                                 };

//                                 lock (result)
//                                 {
//                                     result.Add(rssItem);
//                                 }
//                             }
                        
//                     }
//                 }));
//             }

//             await Task.WhenAll(tasks);

//             return result;
//         }
//     }

//     public class RssItem
//     {
//         public string Description { get; set; }
//         public string PubDate { get; set; }
//         public string Link { get; set; }
//     }
// }









// using Microsoft.AspNetCore.Mvc.RazorPages;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Net.Http;
// using System.Threading.Tasks;
// using System.Xml.Linq;

// namespace MultipleRssReader.Pages
// {
//     public class RssFeedModel : PageModel
//     {
//         private readonly IHttpClientFactory _clientFactory;

//         public List<RssItem> ContentCollection { get; set; } = new();
//         public int PageSize { get; set; } = 10;
//         public int PageNumber { get; set; } = 1;
//         public int TotalItems { get; set; }
//         public int TotalPages { get; set; }

//         public RssFeedModel(IHttpClientFactory clientFactory)
//         {
//             _clientFactory = clientFactory;
//         }

//         public async Task OnGetAsync(int pageNumber = 1)
//         {
//             PageNumber = pageNumber;
//             var client = _clientFactory.CreateClient();
//             var response = await client.GetAsync("https://blue.feedland.org/opml?screenname=dave");

//             if (response.IsSuccessStatusCode)
//             {
//                 var opmlContent = await response.Content.ReadAsStringAsync();
//                 var doc = XDocument.Parse(opmlContent);
//                 var feedUrls = new List<string>();

//                 foreach (var outline in doc.Descendants("outline").Where(o => o.Attribute("type")?.Value == "rss"))
//                 {
//                     var xmlUrl = outline.Attribute("xmlUrl")?.Value;
//                     var text = outline.Attribute("text")?.Value;

//                     if (!string.IsNullOrEmpty(xmlUrl))
//                     {
//                         feedUrls.Add(xmlUrl);
//                     }
//                 }

//                 ContentCollection = await ProcessFeedUrlsAsync(feedUrls);
//                 TotalItems = ContentCollection.Count;
//                 TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);

//                 ContentCollection = ContentCollection
//                     .Skip((PageNumber - 1) * PageSize)
//                     .Take(PageSize)
//                     .ToList();
//             }
//             else
//             {
//                 RedirectToPage("/Error");
//             }
//         }

//         public async Task<List<RssItem>> ProcessFeedUrlsAsync(List<string> feedUrls)
//         {
//             var result = new List<RssItem>();
//             var client = _clientFactory.CreateClient();

//             foreach (var url in feedUrls)
//             {
//                 var response = await client.GetAsync(url);

//                 if (response.IsSuccessStatusCode)
//                 {
//                     var xmlContent = await response.Content.ReadAsStringAsync();
//                     var doc = XDocument.Parse(xmlContent);
//                     var ns = doc.Root?.GetDefaultNamespace();

//                     if (ns != null)
//                     {
//                         foreach (var item in doc.Descendants(ns + "item"))
//                         {
//                             var link = item.Element(ns + "link")?.Value;
//                             var description = item.Element(ns + "description")?.Value;
//                             var pubDate = item.Element(ns + "pubDate")?.Value;

//                             var rssItem = new RssItem
//                             {
//                                 Description = GetBriefContent(description),
//                                 PubDate = pubDate,
//                                 Link = link
//                             };

//                             result.Add(rssItem);
//                         }
//                     }
//                 }
//             }

//             return result;
//         }

//         private string GetBriefContent(string description)
//         {
//             const int maxLength = 150;
//             if (description.Length <= maxLength)
//                 return description;
//             return description.Substring(0, maxLength) + "...";
//         }
//     }

//     public class RssItem
//     {
//         public string Title { get; set; }
//         public string Description { get; set; }
//         public string PubDate { get; set; }
//         public string Link { get; set; }
//     }
// }





// using Microsoft.AspNetCore.Mvc.RazorPages;
// using System.Collections.Generic;
// using System.Linq;
// using System.Net.Http;
// using System.Threading.Tasks;
// using System.Xml.Linq;

// namespace MultipleRssReader.Pages
// {
//     public class RssFeedModel : PageModel
//     {
//         private readonly IHttpClientFactory _clientFactory;

//         public RssFeedModel(IHttpClientFactory clientFactory)
//         {
//             _clientFactory = clientFactory;
//         }

//         public List<RssItem> ContentCollection { get; set; } = new();
//         public int PageSize { get; set; } = 10;
//         public int PageNumber { get; set; } = 1;
//         public int TotalItems { get; set; }
//         public int CurrentPage { get; set; } = 1;
//         public int TotalPages { get; set; } = 1;

//         public async Task OnGetAsync(int PageNumber = 1)
//         {
//             var client = _clientFactory.CreateClient();
//             var response = await client.GetAsync("https://blue.feedland.org/opml?screenname=dave");

//             if (response.IsSuccessStatusCode)
//             {
//                 var xmlContent = await response.Content.ReadAsStringAsync();
//                 var doc = XDocument.Parse(xmlContent);

//                 ContentCollection = new List<RssItem>();

//                 foreach (var outline in doc.Descendants("outline").Where(o => o.Attribute("type")?.Value == "rss"))
//                 {
//                     var xmlUrl = outline.Attribute("xmlUrl")?.Value;
//                     var htmlUrl = outline.Attribute("htmlUrl")?.Value;
//                     var text = outline.Attribute("text")?.Value;

//                     var rssItem = new RssItem
//                     {
//                         Title = text,
//                         XmlUrl = xmlUrl,
//                         HtmlUrl = htmlUrl
//                     };

//                     ContentCollection.Add(rssItem);
//                 }

//                 TotalItems = ContentCollection.Count;
//                 TotalPages = (int)Math.Ceiling((double)ContentCollection.Count / PageSize);
//                 CurrentPage = PageNumber;
                
//                 ContentCollection = ContentCollection
//                     .Skip((PageNumber - 1) * PageSize)
//                     .Take(PageSize)
//                     .ToList();
//             }
//             else
//             {
//                 ContentCollection = new List<RssItem>();
//             }
//         }
//     }

//     public class RssItem
//     {
//         public string Title { get; set; }
//         public string XmlUrl { get; set; }
//         public string HtmlUrl { get; set; }
//     }
// }







