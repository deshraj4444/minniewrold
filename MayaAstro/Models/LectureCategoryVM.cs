using System;
using System.ComponentModel.DataAnnotations;
namespace MayaAstro.Models
{
	public class LectureCategoryVM
    {
        public int Id { get; set; }
        public int ContentId { get; set; }
        [Required(ErrorMessage = "Name is a required field")]
        public string LectureName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; } = DateTime.UtcNow;
        public List<LectureCategoryVM> LectureList { get; set; }
        public List<ContentMasterVM> ContentMasterList { get; set; }
    }
}

