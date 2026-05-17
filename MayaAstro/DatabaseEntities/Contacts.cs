using MayaAstro.Models;

namespace MayaAstro.DatabaseEntities
{
    public class Contacts
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string Remarks { get; set; }
        public int SiteType { get; set; } = 1;
        public string Semester { get; set; }
        public string Technology { get; set; }

        public DateTime? CreatedDate { get; set; }

        
    }
}
