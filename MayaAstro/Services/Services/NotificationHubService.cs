using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.NotificationHubs.Messaging;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MayaAstro.Services.Services
{
    public class NotificationHubService
    {
        private readonly NotificationHubClient _hubClient;
        private readonly ILogger<NotificationHubService> _logger;

        public NotificationHubService(string connectionString, string hubName, ILogger<NotificationHubService> logger)
        {
            _hubClient = NotificationHubClient.CreateClientFromConnectionString(connectionString, hubName);
            _logger = logger;
        }

        public async Task<string> RegisterDeviceAsync(string deviceToken, string accountGuid, string platform)
        {
            _logger.LogInformation("📱 RegisterDeviceAsync called. AccountGuid={AccountGuid}, Platform={Platform}, Token={Token}", accountGuid, platform, deviceToken);

            if (string.IsNullOrEmpty(deviceToken) || string.IsNullOrEmpty(accountGuid) || string.IsNullOrEmpty(platform))
            {
                _logger.LogWarning("❌ Invalid device registration request. Missing required fields.");
                throw new ArgumentException("DeviceToken, AccountGuid, and Platform are required.");
            }

            try
            {
                NotificationPlatform notificationPlatform = platform.ToLower() switch
                {
                    "android" => NotificationPlatform.Fcm,
                    "ios" => NotificationPlatform.Apns,
                    _ => throw new ArgumentException("Unsupported platform. Use 'ios' or 'android'.")
                };

                var installation = new Installation
                {
                    InstallationId = accountGuid,
                    Platform = notificationPlatform,
                    PushChannel = deviceToken,
                    Tags = new[] { $"user:{accountGuid}-{platform.ToLower()}" }
                };

                // Log full installation object as JSON
                _logger.LogDebug("📦 Installation object: {InstallationJson}", JsonSerializer.Serialize(installation));

                await _hubClient.CreateOrUpdateInstallationAsync(installation);

                _logger.LogInformation("✅ Device registered successfully. InstallationId={InstallationId}, Tags={Tags}",
                    installation.InstallationId, string.Join(",", installation.Tags));

              

                return installation.InstallationId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔥 Failed to register device.");
                throw;
            }
        }


        public async Task<string> SendNotificationAsync(string accountGuid, string message, string postGuid, string postType, string postTypeIconURL)
        {
            try
            {
                var alert = $@"
{{
  ""aps"": {{
    ""alert"": ""{message}"",
    ""sound"": ""default""
  }},
  ""postGuid"": ""{postGuid}"",
  ""postType"": ""{postType}"",
  ""postTypeIconURL"": ""{postTypeIconURL}""
}}";

                var tag = $"user:{accountGuid}-ios";
                _logger.LogInformation("📤 Sending APNs notification to tag: {Tag}", tag);

                var result = await _hubClient.SendAppleNativeNotificationAsync(alert, tag);

                if (result != null)
                {
                    _logger.LogInformation("✅ Notification sent. State={State}, TrackingId={TrackingId}", result.State, result.TrackingId);
                    return result.TrackingId;
                }

                _logger.LogWarning("⚠️ Notification send result is null.");
                return "";
            }
            catch(Exception ex)
            {
                return null;
            }
        }
       

    }
}
