using MayaAstro.Controllers;

namespace MayaAstro.Models
{
    public class IndexPageVM
    {
        public List<HoroscopeResponse> Horoscopes { get; set; }
        public List<BlogDetailVM> BlogList { get; set; }
        public List<HoroscopeVM> HoroscopeVM { get; set; }
    }


}
