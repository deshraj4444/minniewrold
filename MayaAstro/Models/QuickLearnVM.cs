using System;
namespace MayaAstro.Models
{
	public class QuickLearnVM
	{
        public int Id { get; set; }
        public string Title { get; set; }
        public int UserId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
        public List<QuickLearnVM> QuickLearnList { get; set; }
    }
}

