namespace MayaAstro.Models
{


    public class RashiVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<RashiVM> RashiList { get; set; }
    }



}
