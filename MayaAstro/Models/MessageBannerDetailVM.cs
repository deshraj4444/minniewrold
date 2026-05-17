using System;
namespace MayaAstro.Models
{
    public class MessageBannerDetailVM
    {
        public int Id { get; set; }
        public string WelcomeMessage { get; set; }
        public string ContactMessage { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string Address { get; set; }

        public string Email { get; set; }

        public string BookingNumber { get; set; }
    }
}

