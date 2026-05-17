using System;
namespace MayaAstro.DatabaseEntities
{
	public class QuickLearn
	{
        public int Id { get; set; }
        public string Title { get; set; }
        public int UserId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}

