namespace MayaAstro.Models
{
    public class ProductVM
    {
        public int Id { get; set; }
        public string ProductCategoryName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsActive { get; set; }
        public List<ProductVM> ProductList { get; set; }
        public List<ProductDetailVM> AllProductList { get; set; }
    }
}
