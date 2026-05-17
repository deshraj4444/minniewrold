using MayaAstro.Models;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Model;
using MayaAstro.Services.Configuration;
using SendGrid.Helpers.Mail.Model;

namespace MayaAstro.Controllers
{
    public class MinnieWorldController : Controller
    {

        private readonly IOptions<Services.Configuration.BrevoSettings> _brevo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _client;
        private readonly IOptions<Messages> _messages;
        HttpResponseMessage responseMessage = new HttpResponseMessage();
        private readonly IConfiguration _configuration;

        public MinnieWorldController(IConfiguration configuration,IHttpContextAccessor httpContextAccessor, IOptions<Messages> messages, IOptions<BrevoSettings> brevo)
        {
            _brevo = brevo;
            _client = new HttpClient(); 
            var apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl");
            _client.BaseAddress = new Uri(apiBaseUrl);
            _httpContextAccessor = httpContextAccessor;
            _messages = messages;

        }
        //[Route("index")]
        public IActionResult Index()
        {
            return View();
        }

        

        [HttpPost]
        public async Task<IActionResult> ContactUs(ContactsVM model)
        {
            if (ModelState.IsValid)
            {
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                responseMessage = await _client.PostAsync("api/AddContact/", content);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<ApiResponseModel>(responseData);
                    if (response != null && response.Data != null)
                    {
                        var savedContact = JsonConvert.DeserializeObject<ContactsVM>(response.Data.ToString());
                    }
                }



                if (!Configuration.Default.ApiKey.ContainsKey("api-key"))
                {
                    Configuration.Default.ApiKey.Add("api-key", _brevo.Value.ApiKey);
                }

                var apiInstance = new TransactionalEmailsApi();
                var sender = new SendSmtpEmailSender("MinnieWorld", _brevo.Value.SenderEmail);
                var recipient = new SendSmtpEmailTo(model.Email, model.FirstName);
                string temapltename = "Verify your Email Address";
                string templatedes = "Thank you for Visiting.";
                string admintemplatename = "New User Registed";
                string admintemplatedes = $"A new user has registered.<br><br><b>Name:</b> {model.FirstName} {model.LastName}<br><b>Email:</b> {model.Email}";
                string HtmlContent = GetTemplateHmtl(temapltename, templatedes);
                string HtmlContentadmin = GetAdminHtmltemplate(
    "New User Registered",
    "A new user has joined our platform.",
    model.FirstName,
    model.LastName,
    model.Email
);
                // HTML content for the email

                var TextContent = HtmlContent;

                // Replace placeholders in HtmlContent
                HtmlContent = HtmlContent.Replace("[URL]", TextContent)
                                         .Replace("[FirstName]", model.FirstName)
                                         .Replace("[Email]", model.Email);

                var email = new SendSmtpEmail
                {
                    Sender = sender,
                    To = new List<SendSmtpEmailTo> { recipient },
                    HtmlContent = HtmlContent,
                    Subject = "Verify Email Request",
                };
                var result = await apiInstance.SendTransacEmailAsync(email);


                var adminrecipient = new SendSmtpEmailTo(_brevo.Value.SenderEmail.ToString(), _brevo.Value.Name);
                var adminmail = new SendSmtpEmail
                {
                    Sender = sender,
                    To = new List<SendSmtpEmailTo> { adminrecipient }, 
                    HtmlContent = HtmlContentadmin, // Make sure to use HtmlContentadmin here
                    Subject = "New User Registered",
                };
                var adminresult = await apiInstance.SendTransacEmailAsync(adminmail);
                TempData["message"] = "Contact saved successfully.";
                return RedirectToAction("Index", "MinnieWorld");
            }

            return View(model);
        }

        public string GetTemplateHmtl(string templateName, string templateDes)
        {
            string HtmlContent = @"
                <html>
                <head>
                    <title></title>
                    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
                    <meta name='viewport' content='width=device-width, initial-scale=1' />
                    <meta http-equiv='X-UA-Compatible' content='IE=edge' />
                </head>
                <body style='background-color: #f4f4f4; margin: 0 !important; padding: 0 !important;'>
                    <table border='0' cellpadding='0' cellspacing='0' width='100%'>
                        <tr>
                            <td bgcolor='#f4cc68' align='center'>
                                <table border='0' cellpadding='0' cellspacing='0' width='600'>
                                    <tr>
                                        <td align='center' valign='top' style='padding: 20px 10px 40px 10px;'>
                                            <a href='' target='_blank'>
                                                <img alt='Logo' src='/imgs/avatar.png' width='180' style='display: block;  font-family: ''Lato'', Helvetica, Arial, sans-serif; color: #ffffff; font-size: 18px;' border='0' />
                                            </a>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td bgcolor='#f4cc68' align='center' style='padding: 0px 10px 0px 10px;'>
                                <table border='0' cellpadding='0' cellspacing='0' width='600'>
                                    <tr>
                                        <td bgcolor='#ffffff' align='center' valign='top' style='padding: 30px 20px 0px 0px;border-radius: 4px 4px 0px 0px;color: #111111;font-family: ''Lato'', Helvetica, Arial, sans-serif;font-size: 48px;font-weight: 400;letter-spacing: 4px;line-height: 36px;'>
                                            <h1 style='font-size: 36px;font-weight: 400;margin: 0;font-family: arial;font-weight: 600;letter-spacing: 0px;color: #fbb917;'>{@temapltename}</h1>
                                            <p style='margin-bottom: 0px; padding: 0 32px;font-size: 17px;letter-spacing: 0px;text-align: left;font-family: arial;'>Hi :- <b style='color:#03ADEC;'> [FirstName]</b></p>
                                            <p style='line-height: 23px; margin-bottom: 0px; margin-top: 5px;padding: 0 32px;font-size: 17px;letter-spacing: 0px;text-align: left;font-family: arial; color:666666;'>{@templatedes} </p>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td bgcolor='#fbb917' align='center' style='padding: 0px 10px 0px 10px;'>
                                <table border='0' cellpadding='0' cellspacing='0' width='600'>
                                    <tr>
                                        <td bgcolor='#ffffff' align='left'>
                                            <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                                <tr>
                                                    <td bgcolor='#ffffff' align='center' style='padding: 20px 30px 0px 30px;'>
                                                        <table border='0' cellspacing='0' cellpadding='0'>
                                                            <tr>
                                                                <td align='center' style='border-radius: 3px;' bgcolor='#fbb917'><a href='[URL]' target='_blank' style='font-size: 20px; font-weight: 600; font-family: Helvetica, Arial, sans-serif; color: #ffffff; text-decoration: none; padding: 15px 25px; border-radius: 2px; border: 1px solid #fbb917; display: inline-block;'>Click To Verify </a></td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td bgcolor='#ffffff' align='left' style='padding: 20px 30px 30px 30px; color: #666666; font-family: ''Lato'', Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;'>
                                                        <p style='margin: 0; text-align: center;'>Your username, in case you've forgotten is: <br /><b style='color:#03ADEC;'>[Email]</b> </p>
                                                        <p style='text-align: center; font-weight: 600; font-size: 26px; color: #fbb917; font-family: arial;'>Thanks for using our site!</p>
                                                        
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";
            HtmlContent = HtmlContent.Replace("{@temapltename}", templateName).Replace("{@templatedes}", templateDes);
            return HtmlContent;

        }

        public string GetAdminHtmltemplate(string admintemplatename, string admintemplateDes, string firstName, string lastName, string email)
        {
            return $@"
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{admintemplatename}</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 20px auto;
            background: #ffffff;
            padding: 30px;
            border-radius: 8px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
        }}
        h2 {{
            color: #fbb917;
            text-align: center;
        }}
        .info {{
            padding: 15px;
            border: 1px solid #ddd;
            border-radius: 5px;
            background: #f9f9f9;
            margin-top: 20px;
        }}
        p {{
            font-size: 16px;
            color: #333;
            margin: 10px 0;
        }}
        .footer {{
            text-align: center;
            margin-top: 30px;
            font-size: 14px;
            color: #888;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>{admintemplatename}</h2>
        <p>{admintemplateDes}</p>
        <div class='info'>
            <p><strong>Full Name:</strong> {firstName} {lastName}</p>
            <p><strong>Email Address:</strong> {email}</p>
            <p><strong>Submitted At (UTC):</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}</p>
        </div>
        <p class='footer'>This is an automated message from Maya IT Solution.</p>
    </div>
</body>
</html>";
        }


    }
}
