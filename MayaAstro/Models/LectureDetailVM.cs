using System;
using System.ComponentModel.DataAnnotations;
namespace MayaAstro.Models
{
	public class LectureDetailVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "German Title is a required field")]
        public string Name { get; set; }
        [Required(ErrorMessage = "English Title is a required field")]
        public string EnglishName { get; set; }
        [Required(ErrorMessage = "Category is a required field")]
        public int LectureId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; } = DateTime.UtcNow;
        public string LectureCategoryName { get; set; }
        public List<LectureDetailVM> LectureList { get; set; }
        public List<LectureCategoryVM> LectureCategoryList { get; set; }
    }
}

