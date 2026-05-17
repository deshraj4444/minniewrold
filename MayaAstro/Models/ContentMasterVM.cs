namespace MayaAstro.Models
{
    public class ContentMasterVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public string ModifiedBy { get; set; }
        public DateTime?  ModifiedDate { get; set; } = DateTime.UtcNow;
        public List<ContentMasterVM> ContentMasterList { get; set; }
    }
}
