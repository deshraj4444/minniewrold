namespace MayaAstro.DatabaseEntities
{
    public class Horoscope
    {
        public int Id { get; set; }
        public string PageUrl { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public string ThumbnailImageUrl { get; set; }
        public string ThumbnailAltText { get; set; }
        public string BannerImageUrl { get; set; }
        public string BannerAltText { get; set; }
        public DateOnly Date { get; set; }

        public int RashiId { get; set; }
        public string HoroscopeSeoTitle { get; set; }
        public string HoroscopeMetaTitle { get; set; }
        public string HoroscopeMetaDescription { get; set; }
        public string HoroscopeMetaKeyword { get; set; } 
        public string HoroscopeMetaContent { get; set; }
      
    }
}
