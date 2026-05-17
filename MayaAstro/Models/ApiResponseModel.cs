namespace MayaAstro.Models
{
    public class ApiResponseModel
    {
        public string Message { get; set; }
        public bool Status { get; set; }= true;
        public int StatusCode { get; set; }
        public object Data { get; set; }
    }
}
