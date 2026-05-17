namespace MayaAstro.DatabaseEntities
{
    public class QuestionDetail
    {
        public int Id { get; set; }
        public string Answer { get; set; }
        public int UserId { get; set; }
        public Boolean IsTrue { get; set; }
        public int QuestionId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
