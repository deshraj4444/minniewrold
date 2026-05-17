    namespace MayaAstro.Models
{
    public class ProductSubCategoryVM
    {
        public int Id { get; set; }
        public int? ProductCategoryId { get; set; }
        public string ProductCategoryName { get; set; }
        public string ProductSubCategoryName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsActive { get; set; }
        public List<ProductSubCategoryVM> SubCategoriesList { get; set; }
        public List<ProductVM> ProductList { get; set; }
    }
}
