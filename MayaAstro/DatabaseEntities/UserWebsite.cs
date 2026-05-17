namespace MayaAstro.DatabaseEntities
{
    public class UserWebsite
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int WebsiteId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
