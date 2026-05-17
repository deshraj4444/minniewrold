namespace MayaAstro.Models
{
    public class YouTubeVideo
    {
        public class YouTubeVideoVM
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string ThumbnailImageUrl { get; set; }
            public string Author { get; set; }
            public DateTime? PublishDate { get; set; }
            public string PageUrl { get; set; }
            public bool IsPublished { get; set; }

            // You can add more if needed, such as SEO metadata, but for YouTube videos, these are key.
        }
    }
}
