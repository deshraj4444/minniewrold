namespace AstroPortal.Api.Models;

public class Testimonial
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Quote { get; set; } = string.Empty;
    public int Rating { get; set; } = 5;
    public string AvatarUrl { get; set; } = string.Empty;
}
