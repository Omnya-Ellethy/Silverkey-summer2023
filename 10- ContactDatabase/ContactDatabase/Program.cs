using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRouting();

var app = builder.Build();

builder.Services.AddRouting();

app.UseStaticFiles(); // Serve static files from the wwwroot folder

// Define a simple HTML template
app.MapGet("/", context =>
{
    // Set the response content type to HTML
    context.Response.ContentType = "text/html";
    // Return the contents of index.html as the response
    return context.Response.SendFileAsync("wwwroot/index.html");
});

app.Run();
