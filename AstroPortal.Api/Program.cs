using AstroPortal.Api.Contracts;
using AstroPortal.Api.Data;
using AstroPortal.Api.Models;
using AstroPortal.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AstroDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AstroPortalDb")));

builder.Services.AddHttpClient<IHoroscopeService, FreeHoroscopeService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AstroFrontend", policy =>
    {
        var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:5173", "https://localhost:5173" };

        policy.WithOrigins(configuredOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AstroFrontend");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AstroDbContext>();
    await DbInitializer.SeedAsync(dbContext);
}

app.MapGet("/api/health", () => Results.Ok(new { status = "Healthy", service = "AstroPortal.Api" }));

app.MapGet("/api/horoscopes/daily/{sign}", async (string sign, IHoroscopeService horoscopeService, CancellationToken cancellationToken) =>
{
    try
    {
        var horoscope = await horoscopeService.GetDailyHoroscopeAsync(sign, cancellationToken);
        return Results.Ok(horoscope);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapGet("/api/blogs", async (AstroDbContext dbContext, bool includeDrafts = false) =>
{
    var query = dbContext.BlogPosts.AsNoTracking();

    if (!includeDrafts)
    {
        query = query.Where(blog => blog.IsPublished);
    }

    var blogs = await query
        .OrderByDescending(blog => blog.CreatedAt)
        .ToListAsync();

    return Results.Ok(blogs);
});

app.MapGet("/api/blogs/{slug}", async (string slug, AstroDbContext dbContext) =>
{
    var blog = await dbContext.BlogPosts
        .AsNoTracking()
        .FirstOrDefaultAsync(item => item.Slug == slug && item.IsPublished);

    if (blog is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(blog);
});

app.MapPost("/api/admin/blogs", async (BlogPostRequest request, AstroDbContext dbContext) =>
{
    var blog = new BlogPost
    {
        Title = request.Title.Trim(),
        Slug = CreateSlug(request.Title),
        Excerpt = request.Excerpt.Trim(),
        Content = request.Content.Trim(),
        Category = request.Category.Trim(),
        ImageUrl = request.ImageUrl.Trim(),
        IsPublished = request.IsPublished,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    dbContext.BlogPosts.Add(blog);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/api/blogs/{blog.Slug}", blog);
});

app.MapPut("/api/admin/blogs/{id:int}", async (int id, BlogPostRequest request, AstroDbContext dbContext) =>
{
    var blog = await dbContext.BlogPosts.FindAsync(id);

    if (blog is null)
    {
        return Results.NotFound();
    }

    blog.Title = request.Title.Trim();
    blog.Slug = CreateSlug(request.Title);
    blog.Excerpt = request.Excerpt.Trim();
    blog.Content = request.Content.Trim();
    blog.Category = request.Category.Trim();
    blog.ImageUrl = request.ImageUrl.Trim();
    blog.IsPublished = request.IsPublished;
    blog.UpdatedAt = DateTime.UtcNow;

    await dbContext.SaveChangesAsync();
    return Results.Ok(blog);
});

app.MapDelete("/api/admin/blogs/{id:int}", async (int id, AstroDbContext dbContext) =>
{
    var blog = await dbContext.BlogPosts.FindAsync(id);

    if (blog is null)
    {
        return Results.NotFound();
    }

    dbContext.BlogPosts.Remove(blog);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});

app.MapGet("/api/products", async (AstroDbContext dbContext) =>
{
    var products = await dbContext.Products
        .AsNoTracking()
        .OrderBy(product => product.Category)
        .ThenBy(product => product.Name)
        .ToListAsync();

    return Results.Ok(products);
});

app.MapGet("/api/testimonials", async (AstroDbContext dbContext) =>
{
    var testimonials = await dbContext.Testimonials
        .AsNoTracking()
        .OrderByDescending(testimonial => testimonial.Rating)
        .ToListAsync();

    return Results.Ok(testimonials);
});

app.MapPost("/api/contact", async (ContactRequestDto request, AstroDbContext dbContext) =>
{
    var contact = new ContactRequest
    {
        FullName = request.FullName.Trim(),
        Email = request.Email.Trim(),
        Phone = request.Phone.Trim(),
        Subject = request.Subject.Trim(),
        Message = request.Message.Trim(),
        CreatedAt = DateTime.UtcNow
    };

    dbContext.ContactRequests.Add(contact);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/api/contact/{contact.Id}", new { contact.Id, message = "Contact request submitted." });
});

app.MapPost("/api/kundali-requests", async (KundaliRequestDto request, AstroDbContext dbContext) =>
{
    var kundaliRequest = new KundaliRequest
    {
        FullName = request.FullName.Trim(),
        Email = request.Email.Trim(),
        Phone = request.Phone.Trim(),
        BirthDate = request.BirthDate,
        BirthTime = request.BirthTime,
        BirthPlace = request.BirthPlace.Trim(),
        ServiceType = request.ServiceType.Trim(),
        Notes = request.Notes.Trim(),
        CreatedAt = DateTime.UtcNow
    };

    dbContext.KundaliRequests.Add(kundaliRequest);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/api/kundali-requests/{kundaliRequest.Id}", new { kundaliRequest.Id, message = "Kundali service request submitted." });
});

app.Run();

static string CreateSlug(string title)
{
    var slug = new string(title
        .Trim()
        .ToLowerInvariant()
        .Select(character => char.IsLetterOrDigit(character) ? character : '-')
        .ToArray());

    while (slug.Contains("--"))
    {
        slug = slug.Replace("--", "-");
    }

    return slug.Trim('-');
}
