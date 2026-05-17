using AstroPortal.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AstroPortal.Api.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AstroDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();

        if (!await dbContext.BlogPosts.AnyAsync())
        {
            dbContext.BlogPosts.AddRange(
                new BlogPost
                {
                    Title = "How Daily Horoscopes Help You Plan Mindfully",
                    Slug = "daily-horoscopes-mindful-planning",
                    Excerpt = "Learn how zodiac insights can become a gentle ritual for intention-setting and better choices.",
                    Content = "Daily horoscopes are best used as reflective prompts. Read your sign, note the theme, and choose one practical action for relationships, career, or self-care.",
                    Category = "Horoscope",
                    ImageUrl = "https://images.unsplash.com/photo-1532968961962-8a0cb3a2d4f5?auto=format&fit=crop&w=1200&q=80"
                },
                new BlogPost
                {
                    Title = "Kundali Basics: Birth Time, Place and Planetary Houses",
                    Slug = "kundali-basics-birth-time-place-houses",
                    Excerpt = "A beginner-friendly guide to the key details needed for accurate kundali services.",
                    Content = "A kundali maps the sky at your exact birth moment. Accurate date, time, and place help astrologers interpret planetary houses with more confidence.",
                    Category = "Kundali",
                    ImageUrl = "https://images.unsplash.com/photo-1515942661900-94b3d1972591?auto=format&fit=crop&w=1200&q=80"
                },
                new BlogPost
                {
                    Title = "Choosing Religious Items for a Peaceful Home Altar",
                    Slug = "religious-items-peaceful-home-altar",
                    Excerpt = "Simple tips for selecting idols, malas, incense and diya sets for your sacred corner.",
                    Content = "Choose altar items that are meaningful, easy to maintain, and aligned with your daily practice. Keep the space clean, bright, and intentional.",
                    Category = "Spiritual Living",
                    ImageUrl = "https://images.unsplash.com/photo-1609607847926-da4702f01fef?auto=format&fit=crop&w=1200&q=80"
                });
        }

        if (!await dbContext.Products.AnyAsync())
        {
            dbContext.Products.AddRange(
                new Product { Name = "Rudraksha Mala", Category = "Mala", Description = "A traditional meditation mala for mantra chanting and daily grounding.", Price = 1499, ImageUrl = "https://images.unsplash.com/photo-1602173574767-37ac01994b2a?auto=format&fit=crop&w=900&q=80" },
                new Product { Name = "Brass Diya Set", Category = "Puja Essentials", Description = "Elegant brass diyas for puja rituals, festivals and home altar lighting.", Price = 899, ImageUrl = "https://images.unsplash.com/photo-1605368380945-7472b7fb0697?auto=format&fit=crop&w=900&q=80" },
                new Product { Name = "Sandalwood Incense", Category = "Incense", Description = "Calming sandalwood incense sticks for meditation and peaceful spaces.", Price = 349, ImageUrl = "https://images.unsplash.com/photo-1602928321679-560bb453f190?auto=format&fit=crop&w=900&q=80" },
                new Product { Name = "Crystal Healing Kit", Category = "Crystals", Description = "Curated crystals for focus, clarity and positive energy practices.", Price = 2199, ImageUrl = "https://images.unsplash.com/photo-1515562141207-7a88fb7ce338?auto=format&fit=crop&w=900&q=80" },
                new Product { Name = "Puja Thali", Category = "Puja Essentials", Description = "Complete decorative thali for daily worship and auspicious ceremonies.", Price = 1299, ImageUrl = "https://images.unsplash.com/photo-1583394838336-acd977736f90?auto=format&fit=crop&w=900&q=80" });
        }

        if (!await dbContext.Testimonials.AnyAsync())
        {
            dbContext.Testimonials.AddRange(
                new Testimonial { CustomerName = "Ananya Sharma", Location = "Delhi", Quote = "The kundali consultation was thoughtful, practical and easy to understand.", Rating = 5, AvatarUrl = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=300&q=80" },
                new Testimonial { CustomerName = "Rohit Mehta", Location = "Mumbai", Quote = "Daily guidance and remedies helped me bring structure to important decisions.", Rating = 5, AvatarUrl = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=300&q=80" },
                new Testimonial { CustomerName = "Meera Kapoor", Location = "Jaipur", Quote = "Beautiful products, quick response, and a very professional astrology experience.", Rating = 5, AvatarUrl = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=300&q=80" });
        }

        await dbContext.SaveChangesAsync();
    }
}
