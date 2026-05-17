namespace MayaAstro.Models
{
    public class ApiResponse<T>
    {
        public T Data { get; set; }
        public int Status { get; set; }
        public bool Success { get; set; }
    }
}
