using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MayaAstro.DatabaseEntities
{
    public class Rashi
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; } 
       public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
       public List<Horoscope> Horoscopes { get; set; } 

    }
}
