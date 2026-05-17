namespace MayaAstro.Models
{
    public class DeviceTokenVM
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public int UserId { get; set; }
        public string Idiom { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Name { get; set; }
        public string Platform { get; set; }
        public string Version { get; set; }
    }
}
