using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ReadRss.Pages
{
    public class RssFeedModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public RssFeedModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public List<RssItem> ContentCollection { get; set; } = new();

        public async Task OnGetAsync()
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync("http://scripting.com/rss.xml");

            if (response.IsSuccessStatusCode)
            {
                var xmlContent = await response.Content.ReadAsStringAsync();

                var doc = XDocument.Parse(xmlContent);

                // Extract the content from XML and add to the collection
                foreach (var item in doc.Descendants("item"))
                {
                    var title = item.Element("title")?.Value;
                    var link = item.Element("link")?.Value;
                    var description = item.Element("description")?.Value;
                    var pubDate = item.Element("pubDate")?.Value;

                    var RssItem = new RssItem
                    {
                        Title = title,
                        Description = description,
                        PubDate = pubDate,
                        Link = link
                    };

                    ContentCollection.Add(RssItem);
                }
            }
            else
            {
                ContentCollection = new List<RssItem>();
            }
        }
    }

    public class RssItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string PubDate { get; set; }
        public string Link { get; set; }
    }
}
