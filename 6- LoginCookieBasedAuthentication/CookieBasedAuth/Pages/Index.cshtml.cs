using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace CookieBasedAuth.Pages;
public class IndexModel : PageModel
{
    [BindProperty]
    public LoginInputs LoginInputs { get; set; }

    public IndexModel()
    {
    }

    public void OnGet()
    {

    }
    public async Task<IActionResult> OnPost()
    {
        string username = LoginInputs.Username;
        string password = LoginInputs.Password;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ModelState.AddModelError("LoginError", "Enter Username and password");
            return Page();
        }
        if (username == "intern" && password == "summer 2023 july")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "User")
            };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
            );

            return RedirectToPage();
        }
        else
        {
            ModelState.AddModelError("LoginError", "Invalid username or password.");
            return Page();
        }
    }
    public async Task<IActionResult> OnPostLogout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        return RedirectToPage("/Index");
    }
}

public class LoginInputs
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}