using System;
namespace MayaAstro.Models
{
	public class LecturerDetailVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string EnglishName { get; set; }
        public int LecturerId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public List<LecturerDetailVM> LectureList { get; set; }
        public List<LecturerCategoryVM> LectureCategoryList { get; set; }
    }
}

