using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using EdgeDB;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEdgeDB(EdgeDBConnection.FromInstanceName("ContactDatabase"), config =>
{
    config.SchemaNamingStrategy = INamingStrategy.SnakeCaseNamingStrategy;
});

builder.Services.AddRouting();

var app = builder.Build();

app.UseStaticFiles(); // Serve static files from the wwwroot folder

// Define a simple HTML template
app.MapGet("/", context =>
{
    // Set the response content type to HTML
    context.Response.ContentType = "text/html";
    // Return the contents of index.html as the response
    return context.Response.SendFileAsync("wwwroot/index.html");
});

app.MapGet("/contactlist", context =>
{
    context.Response.ContentType = "text/html";
    return context.Response.SendFileAsync("wwwroot/contactlist.html");
});


// Handle the form submission and store the contact data
app.MapPost("/submit", async context =>
{
    // Get the form data from the request
    var firstName = context.Request.Form["firstname"].ToString();
    var lastName = context.Request.Form["lastname"].ToString();
    var email = context.Request.Form["email"].ToString();
    var title = context.Request.Form["title"].ToString();
    var birthdate = context.Request.Form["birthdate"].ToString();
    var marriagestatus = context.Request.Form["marriagestatus"].ToString().ToLower(); // Convert to lowercase
    var description = context.Request.Form["description"].ToString();

    // Convert "yes" or "no" to a boolean value
    var isMarried = marriagestatus == "yes";

    // Store the contact data in the database using EdgeDB
    var client = context.RequestServices.GetRequiredService<EdgeDBClient>();

    // Prepare the parameter values as a dictionary
    var parameters = new Dictionary<string, object>
    {
        { "firstName", firstName },
        { "lastName", lastName },
        { "email", email },
        { "title", title },
        { "birthdate", birthdate },
        { "marriagestatus", isMarried },
        { "description", description }
    };

    // Use parameterized query to pass the parameter values
    await client.ExecuteAsync(@"
        INSERT Contact {
            first_name := <str>$firstName,
            last_name := <str>$lastName,
            email := <str>$email,
            title := <str>$title,
            date_of_birth := <str>$birthdate,
            marriage_status := <bool>$marriagestatus,
            description := <str>$description
        }
    ", parameters);

    // Redirect back to the main page after submission
    context.Response.Redirect("/");
});



app.MapGet("/api/contacts", async context =>
{
    var client = context.RequestServices.GetRequiredService<EdgeDBClient>();
    var contacts = await client.QueryAsync<Contact>(@"
        SELECT Contact {
            first_name,
            last_name,
            email,
            title,
            date_of_birth,
            marriage_status,
            description
        }
    ");

    await context.Response.WriteAsJsonAsync(contacts);
});



app.Run();


public class Contact
{
    public string? First_name { get; set; }
    public string? Last_name { get; set; }
    public string? Email { get; set; }
    public string? Title { get; set; }
    public string? Date_of_birth { get; set; }
    public bool? Marriage_status { get; set; }
    public string? Description { get; set; }

}

