using Microsoft.AspNetCore.Mvc;
using MayaAstro.Services.Services;
using Microsoft.Extensions.Logging;

namespace MayaAstro.Controllers.WebApi
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationHubService _notificationHubService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(NotificationHubService notificationHubService, ILogger<NotificationsController> logger)
        {
            _notificationHubService = notificationHubService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceRequest request)
        {
            _logger.LogInformation("🔔 Register endpoint hit. Request={@Request}", request);

            if (string.IsNullOrEmpty(request.DeviceToken) || string.IsNullOrEmpty(request.AccountGuid) || string.IsNullOrEmpty(request.Platform))
            {
                _logger.LogWarning("❌ Missing required fields. DeviceToken={DeviceToken}, AccountGuid={AccountGuid}, Platform={Platform}",
                    request.DeviceToken, request.AccountGuid, request.Platform);
                return BadRequest("DeviceToken, AccountGuid, and Platform are required.");
            }

            try
            {
                var installationId = await _notificationHubService.RegisterDeviceAsync(
                    request.DeviceToken,
                    request.AccountGuid,
                    request.Platform);

                _logger.LogInformation("📬 RegisterDevice response: InstallationId={InstallationId}", installationId);
                return Ok(new { InstallationId = installationId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔥 Registration failed.");
                return StatusCode(500, "An error occurred while registering the device.");
            }
        }



        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request)
        {
            _logger.LogInformation("📨 SendNotification endpoint called with payload: {@Request}", request);

            if (string.IsNullOrEmpty(request.accountGuid) || string.IsNullOrEmpty(request.Message))
            {
                _logger.LogWarning("⚠️ Validation failed: accountGuid or message is missing. accountGuid={AccountGuid}, Message={Message}",
                    request.accountGuid, request.Message);
                return BadRequest("UserId and Message are required.");
            }

            try
            {
                var trackingId = await _notificationHubService.SendNotificationAsync(
                    request.accountGuid,
                    request.Message,
                    request.PostGuid,
                    request.PostType,
                    request.PostTypeIconURL
                );

                _logger.LogInformation("✅ Notification sent successfully. TrackingId={TrackingId}", trackingId);
                return Ok(new { TrackingId = trackingId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔥 Failed to send notification for accountGuid={AccountGuid}", request.accountGuid);
                return StatusCode(500, "An error occurred while sending the notification.");
            }
        }

    }

    public class RegisterDeviceRequest
    {
        public string DeviceToken { get; set; } = string.Empty;
        public string AccountGuid { get; set; }  // Corrected spelling
        public string Platform { get; set; } = string.Empty; // "ios" or "android"
    }

    public class SendNotificationRequest
    {
        public string accountGuid { get; set; }
        public string Message { get; set; }
        public string PostGuid { get; set; }
        public string PostType { get; set; }
        public string PostTypeIconURL { get; set; }
    }
}
