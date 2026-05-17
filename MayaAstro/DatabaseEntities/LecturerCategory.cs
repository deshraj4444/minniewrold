using System;
namespace MayaAstro.DatabaseEntities
{
	public class LectureCategory
    {
		public int Id { get; set; }
        public int ContentId { get; set; }
        public string LectureName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}

