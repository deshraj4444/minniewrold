using System;
namespace MayaAstro.Models
{
	public class LecturerCategoryVM
    {
        public int Id { get; set; }
        public string LecturerName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public List<LecturerCategoryVM> LecturerList { get; set; }
    }
}

