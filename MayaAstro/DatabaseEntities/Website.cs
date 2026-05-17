namespace MayaAstro.DatabaseEntities
{
    public class Website
    {
        public int Id { get; set; }
        public string WebsiteName { get; set; }
        public string WebsiteLogoUrl { get; set; }
        public string WebsiteTitle { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
