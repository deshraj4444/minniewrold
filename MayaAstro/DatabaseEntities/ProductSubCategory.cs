namespace MayaAstro.DatabaseEntities
{
    public class ProductSubCategory
    {
        internal string ProductCategoryName;

        public int Id { get; set; }
        public int ProductCategoryId { get; set; }
        public string ProductSubCategoryName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsActive { get; set; }
    }
}
