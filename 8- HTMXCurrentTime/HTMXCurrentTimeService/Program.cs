var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/CurrentTime", () =>
{
    var currentTime = DateTime.Now.ToString("hh:mm tt  -  dddd, dd MMMM yyyy");
    return Results.Ok(currentTime);
});

app.Run();
