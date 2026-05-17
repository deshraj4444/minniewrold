using System.Text.Json;

namespace MayaAstro.Models
{
    public class FirebaseConfig
    {
        public string ApiKey { get; set; }
        public string AuthDomain { get; set; }
        public string ProjectId { get; set; }
        public string StorageBucket { get; set; }
        public string MessagingSenderId { get; set; }
        public string AppId { get; set; }
        public string MeasurementId { get; set; }

        // Convert the configuration to JSON
        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
