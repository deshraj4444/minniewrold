using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MayaAstro.Models
{
    public class HoroscopeVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public string ThumbnailImageUrl { get; set; }
        public string ThumbnailAltText { get; set; }
        public string BannerImageUrl { get; set; }
        public string BannerAltText { get; set; }
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public string PageUrl { get; set; }
        public int RashiId { get; set; }
        public string HoroscopeSeoTitle { get; set; }
        public string HoroscopeMetaTitle { get; set; }
        public string HoroscopeMetaDescription { get; set; }
        public string HoroscopeMetaKeyword { get; set; }
        public string HoroscopeMetaContent { get; set; }

        public List<HoroscopeVM> HoroscopeList { get; set; }

        public List<RashiVM> RashiList { get; set; }

    }

}

