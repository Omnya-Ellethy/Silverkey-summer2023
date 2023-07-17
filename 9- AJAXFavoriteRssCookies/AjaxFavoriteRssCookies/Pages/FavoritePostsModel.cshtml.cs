using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace AjaxFavoriteRssCookies.Pages
{
    public class FavoritePostsModel : PageModel
    {
        public List<string> StarredFeeds { get; set; } = new List<string>();
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; } = 1;

        public IActionResult OnGetAsync(int pageNumber = 1)
        {
            PageNumber = pageNumber;
            StarredFeeds = GetStarredFeedsFromCookie();

            TotalItems = StarredFeeds.Count;
            TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
            CurrentPage = pageNumber;

            StarredFeeds = StarredFeeds
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return Page();
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