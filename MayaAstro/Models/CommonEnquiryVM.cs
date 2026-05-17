using System;
namespace MayaAstro.Models
{
    public class CommonEnquiryVM
    {
        public int Id { get; set; }

        public string Type { get; set; }   // REGISTER / CONTACT

        public string Name { get; set; }
        public string FirmName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string City { get; set; }

        public string Subject { get; set; }
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public List<CommonEnquiryVM> list { get; set; }
    }
}

