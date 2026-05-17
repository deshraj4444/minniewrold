namespace MayaAstro.DatabaseEntities
{
    public class BlogCategory
    {
        public int Id { get; set; }
        public string BlogCategoryName { get; set; }
        public bool IsActive { get; set; }
        public string BlogCategoryImage { get; set; }
        public string ImageUrl { get; set; }
        public string ImageAltText { get; set; }
        public int? TypeId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public int? WebsiteId { get; set; }
    }
}
