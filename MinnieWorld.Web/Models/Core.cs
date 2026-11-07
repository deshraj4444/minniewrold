namespace MinnieWorld.Web.Models;

/// <summary>A tile on the hub home page. Each one links to a section of this site (e.g. /astro) or an external URL.</summary>
public class Site
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Tagline { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string AccentColor { get; set; } = "#4f46e5";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AdminUser
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>Contact form submissions from every section; <see cref="SiteKey"/> records which one.</summary>
public class ContactMessage
{
    public int Id { get; set; }
    public string SiteKey { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
