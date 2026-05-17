using MayaAstro.DatabaseEntities;

namespace MayaAstro.Models
{
    public class BlogDetailVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string BannerImageUrl { get; set; }
        public string ThumbnailImageUrl {  get; set; }
        public int BlogCategoryId { get; set; }
        public string Tag { get; set; }
        public int SortOrder { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; }
        public int TypeId { get; set; }
        public string ButtonUrl { get; set; }
        public string ButtonText { get; set; }
        public bool IsShowButton { get; set; }
        public string Author { get; set; }
        public DateTime? PublishDate { get; set; }
        public string PageUrl { get; set; }
        public string BannerAltText { get; set; }
        public string BlogMetaTitle { get; set; }
        public string BlogMetaDescription { get; set; }
        public int IsPublished { get; set; }
        public int? WebsiteId { get; set; }
        public bool IsDeleted { get; set; }
        public string SeoTitle { get; set; }
        public string BlogMetaKeyword { get; set; }
        public string OgiImage { get; set; }
        public string BlogMetaContent { get; set; }
        public string BlogCategoryName { get; set; }
        public List<BlogDetailVM> BlogList { get; set; }
        public List<BlogCategoryVM> BlogCategoryList { get; set; }


    }

    public class BlogListImageVM
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public string ThumbnailImageUrl { get; set; }
        public string PageUrl { get; set; }
        public DateTime? PublishDate { get; set; }
        public string CategoryName { get; set; }
        public string Author { get; set; }
        public string AuthorImage { get; set; }
        public string BannerAltText { get; set; }
    }
}
