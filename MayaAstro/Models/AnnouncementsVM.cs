using System;
namespace MayaAstro.Models
{
	public class AnnouncementsVM
	{
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<AnnouncementsVM> AnnouncementsList { get; set; }
    }
}

