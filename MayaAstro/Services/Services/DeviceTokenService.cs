using Newtonsoft.Json;
using System.Text;

namespace MayaAstro.Services.Services
{
    public class DeviceTokenService
    {
        private const string ApiUrl = "https://mayaitsolution.com/api/saveDeviceToken"; 

        public async Task SaveDeviceTokenAsync(string deviceToken, int userId)
        {
            var tokenModel = new
            {
                DeviceToken = deviceToken,
                UserId = userId
            };

            using var client = new HttpClient();
            var jsonPayload = JsonConvert.SerializeObject(tokenModel);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(ApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                // Token saved successfully
            }
            else
            {
                // Handle failure
            }
        }
    }
}
