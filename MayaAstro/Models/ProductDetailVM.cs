
using System.ComponentModel.DataAnnotations;

namespace MayaAstro.Models
{
    public class ProductDetailVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter Title")]
        public string Title { get; set; }

        public string Description { get; set; }
        public string ProductDescription { get; set; }
        public string BannerImageUrl { get; set; }

        public int? ProductCategoryId { get; set; }

        public string ProductCategoryName { get; set; }
        public int? SortOrder { get; set; }
        public string DownloadLink { get; set; }
        public string Specifications { get; set; }
        public string StandardEquipment { get; set; }
        public string OptionalAccessories { get; set; }
        public string SelectionGuide { get; set; }
        public string SeoTitle { get; set; }
        public string ProductMetaTitle { get; set; }
        public string ProductMetaDescription { get; set; }
        public string ProductMetaKeyword { get; set; }
        public string ProductMetaContent { get; set; }
        public int? IsPublished { get; set; }
        public bool? IsDeleted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsActive { get; set; }


        public List<ProductDetailVM> AllProductList { get; set; }
        public List<ProductVM> ProductList { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 9;

    }
}
