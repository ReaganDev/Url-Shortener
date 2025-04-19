using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApiContext>(opt =>
{
    opt.UseSqlServer(connString);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("shortUrl", async (UrlRequestDto urlRequest, ApiContext context, HttpContext ctx) =>
{
    // VALIDATE URL
    if (Uri.TryCreate(urlRequest.LongUrl, UriKind.Absolute, out var inputUrl))
    {
        Results.BadRequest("Invalid URL");
    }

    // SHORTEN URL
    var rand = new Random();
    const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    var randomString = new string(Enumerable.Repeat(chars, 8)
        .Select(s => s[rand.Next(s.Length)]).ToArray());

    // mapping short url to long url    
    var url = new Url
    {
        LongUrl = urlRequest.LongUrl,
        CompactUrl = randomString
    };

    // save to db
    context.Urls.Add(url);
    await context.SaveChangesAsync();

    // construct url
    var responseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}/{url.CompactUrl}";
    return Results.Ok(new UrlResponseDto { CompactUrl = responseUrl });
});

app.MapFallback(async (ApiContext context, HttpContext ctx) =>
{
    var path = ctx.Request.Path.ToUriComponent().Trim('/');
    // check if url exists
    var url = await context.Urls.FirstOrDefaultAsync(x => x.CompactUrl.Trim() == path.Trim());
    if (url == null)
    {
        return Results.NotFound();
    }
    // redirect to long url
    return Results.Redirect(url.LongUrl);
});

app.Run();

class ApiContext : DbContext
{
    public ApiContext(DbContextOptions<ApiContext> options) : base(options)
    {
    }
    public virtual DbSet<Url> Urls { get; set; }
}