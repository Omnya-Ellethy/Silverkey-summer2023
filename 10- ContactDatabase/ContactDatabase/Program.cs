using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using EdgeDB;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

// Add services to the container.
builder.Services.AddEdgeDB(EdgeDBConnection.FromInstanceName("ContactDatabase"), config =>
{
    config.SchemaNamingStrategy = INamingStrategy.SnakeCaseNamingStrategy;
});

builder.Services.AddRouting();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "AuthCookie";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<IPasswordHasher<Contact>, PasswordHasher<Contact>>();
var app = builder.Build();

app.UseAuthentication();
app.UseRouting();
app.UseStaticFiles();

app.MapGet("/", () =>
{
    // Set the response content type to HTML
    return Results.Content(File.ReadAllText("wwwroot/index.html"), "text/html");
});

app.MapPost("/login", async (HttpContext context, EdgeDBClient edgeDbClient, IPasswordHasher<Contact> passwordHasher) =>
{
    var username = context.Request.Form["username"].ToString();
    var password = context.Request.Form["password"].ToString();

    var result = await edgeDbClient.QueryAsync<Contact>(
        "SELECT Contact {role := .role, password := .password, username := .username} FILTER .username = <str>$username",
        new Dictionary<string, object> { { "username", username } });

    var user = result.FirstOrDefault();
    if (user == null || user.Password == null)
    {
        context.Response.StatusCode = 401; // Unauthorized status code
    }

    var passwordVerification = passwordHasher.VerifyHashedPassword(null, user.Password, password);
    if (passwordVerification != PasswordVerificationResult.Success)
    {
        context.Response.StatusCode = 401; // Unauthorized status code
    }

    var responseObj = new{
        user.Username,
        user.Role
    };
    return Results.Json(responseObj);

});

app.MapPost("/submit", async (EdgeDBClient edgeDbClient, IPasswordHasher<Contact> passwordHasher, HttpContext context) =>
{
    // Validate anti-forgery token
    if (!context.Request.HasFormContentType)
    {
        context.Response.StatusCode = 400; // Bad Request
    }

    var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();

    try
    {
        await antiforgery.ValidateRequestAsync(context);
    }
    catch (AntiforgeryValidationException)
    {
        context.Response.StatusCode = 403; // Forbidden
    }
    // Get the form data from the request
    var newContact = new Contact
    {
        FirstName = context.Request.Form["firstname"],
        LastName = context.Request.Form["lastname"],
        Email = context.Request.Form["email"],
        Title = context.Request.Form["title"],
        Description = context.Request.Form["description"],
        DateOfBirth = DateTime.TryParse(context.Request.Form["birthdate"], out DateTime dateOfBirth)? dateOfBirth: default(DateTime), // Set default value on failed conversion
        MarriageStatus = bool.TryParse(context.Request.Form["marriagestatus"], out var isMarried) && isMarried,
        Username = context.Request.Form["username"],
        Password = context.Request.Form["password"],
        Role = context.Request.Form["role"]
    };

    // Perform a null check on the Password property
    if (newContact.Password != null)
    {
        // Hash the user's password using PasswordHasher
        newContact.Password = passwordHasher.HashPassword(null, newContact.Password);
    }

    await edgeDbClient.ExecuteAsync(@"
        INSERT Contact {
            first_name := <str>$first_name,
            last_name := <str>$last_name,
            email := <str>$email,
            title := <str>$title,
            description := <str>$description,
            date_of_birth := <datetime>$date_of_birth,
            marriage_status := <bool>$marriage_status,
            username := <str>$username,
            password := <str>$password,
            role := <str>$role
        }
    ", new Dictionary<string, object?>
    {
        { "first_name", newContact.FirstName },
        { "last_name", newContact.LastName },
        { "email", newContact.Email },
        { "title", newContact.Title },
        { "description", newContact.Description },
        { "date_of_birth", newContact.DateOfBirth },
        { "marriage_status", newContact.MarriageStatus },
        { "username", newContact.Username },
        { "password", newContact.Password },
        { "role", newContact.Role },
    });

    // context.Response.Redirect("/api/contacts");
    return Results.Redirect("/api/contacts");
});

app.MapGet("/api/contacts", async (EdgeDBClient client, HttpContext context) =>
{
    var contacts = await client.QueryAsync<Contact>(@"
        SELECT Contact {
            id,
            first_name,
            last_name,
            email,
            title,
            date_of_birth,
            marriage_status,
            description,
            username,
            password,
            role
        }
    ");

    await context.Response.WriteAsJsonAsync(contacts);
});

app.MapGet("/edit", async (EdgeDBClient client, HttpContext context) =>
{
    var contactId = context.Request.Query["id"].ToString();

    if (!Guid.TryParse(contactId, out var id))
    {
        context.Response.StatusCode = 400; // Bad Request
        return;
    }

    var contact = await client.QuerySingleAsync<Contact>(
        "SELECT Contact {id, first_name, last_name, email, title, date_of_birth, marriage_status, description, username, role} FILTER .id = <uuid>$id",
        new Dictionary<string, object> { { "id", id } });

    if (contact is null)
    {
        context.Response.StatusCode = 404; // Not Found
        return;
    }

    context.Response.StatusCode = 200; // OK
    await context.Response.WriteAsJsonAsync(contact);
});

app.MapPut("/api/contacts/{id}", async (EdgeDBClient client, HttpContext context) =>
{
    var contactId = context.Request.RouteValues["id"].ToString();

    if (!Guid.TryParse(contactId, out var id))
    {
        context.Response.StatusCode = 400; // Bad Request
        return;
    }

    var requestBody = await context.Request.ReadFromJsonAsync<Contact>();

    if (requestBody is null)
    {
        context.Response.StatusCode = 400; // Bad Request
        return;
    }

    await client.ExecuteAsync(@"
        UPDATE Contact
        FILTER .id = <uuid>$id
        SET {
            first_name := <str>$first_name,
            last_name := <str>$last_name,
            email := <str>$email,
            title := <str>$title,
            date_of_birth := <datetime>$date_of_birth,
            marriage_status := <bool>$marriage_status,
            description := <str>$description,
            username := <str>$username,
            role := <str>$role
        }
    ", new Dictionary<string, object?>
    {
        { "id", id },
        { "first_name", requestBody.FirstName },
        { "last_name", requestBody.LastName },
        { "email", requestBody.Email },
        { "title", requestBody.Title },
        { "date_of_birth", requestBody.DateOfBirth },
        { "marriage_status", requestBody.MarriageStatus },
        { "description", requestBody.Description },
        { "username", requestBody.Username },
        { "role", requestBody.Role },

    });

    context.Response.StatusCode = 204; // No Content
});

app.MapPost("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(
        CookieAuthenticationDefaults.AuthenticationScheme
    );

    context.Response.Redirect("/");
});

app.Run();

public class Contact
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Title { get; set; }
    public DateTime  DateOfBirth { get; set; }
    public bool? MarriageStatus { get; set; }
    public string? Description { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; }

}