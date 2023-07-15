using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MultipleRssReader.Pages
{
    public class XmlFeedModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public List<XmlFeedItem> ContentCollection { get; set; } = new();
        public int PageSize { get; set; } = 20;
        public int PageNumber { get; set; } = 1;
        public int TotalItems { get; set; } 
        public int TotalPages { get; set; } 
        public int CurrentPage { get; set; } = 1;


        public XmlFeedModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<IActionResult> OnGetAsync(string url, int pageNumber = 1)
        {
            if (string.IsNullOrEmpty(url))
            {
                return NotFound();
            }

            PageNumber = pageNumber;
            var xmlContent = await FetchContent(url);
            ContentCollection = ParseContent(xmlContent);
            TotalItems = ContentCollection.Count;
            TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
            CurrentPage = PageNumber;

            ContentCollection = ContentCollection
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return Page();
        }

        private async Task<string> FetchContent(string url)
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return string.Empty;
            }
        }

        private List<XmlFeedItem> ParseContent(string xmlContent)
        {
            var doc = XDocument.Parse(xmlContent);

            return doc.Descendants("item")
                .Select(item => new XmlFeedItem
                {
                    Title = item.Element("title")?.Value,
                    Description = item.Element("description")?.Value,
                    PubDate = item.Element("pubDate")?.Value,
                    Link = item.Element("link")?.Value
                })
                .ToList();
        }
    }

    public class XmlFeedItem
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? PubDate { get; set; }
        public string? Link { get; set; }
    }
}
