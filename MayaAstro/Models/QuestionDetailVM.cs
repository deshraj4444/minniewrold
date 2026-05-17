namespace MayaAstro.Models
{
    public class QuestionDetailVM
    {
        public int Id { get; set; }
        public string Answer { get; set; }
        public string QuestionName { get; set; }
        public int UserId { get; set; }
        public Boolean IsTrue { get; set; }
        public int QuestionId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public List<QuestionDetailVM> QuestionDetailList { get; set; }
        public List<QuestionVM> QuestionList { get; set; }
    }
}
