namespace MayaAstro.Services.Configuration
{
    public class HostUrl
    {
        public string WebApi { get; set; }
    }
    public class AwsOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string BucketName { get; set; }
        public string PublicBucketName { get; set; }
        public string SubDirectoryName { get; set; }
        public string PublicSubDirectoryName { get; set; }
        public string EndPoint { get; set; }
        public string Url { get; set; }
        public string PublicUrl { get; set; }
    }
    public class StripeOptions
    {
        public string PublicKey { get; set; }
        public string SecretKey { get; set; }
    }
    public class AppSettings
    {
        public string Secret { get; set; }
    }
    public class App
    {
        public string DefaultTimeZone { get; set; }
    }
    public class TwilioVM
    {
        public string AccountSid { get; set; }
        public string AuthToken { get; set; }
        public string Phoneno { get; set; }
    }
    public class SmtpUser
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string Port { get; set; }
        public string Host { get; set; }
        public string From { get; set; }
    }
    public class BrevoSettings
    {
        public string ApiKey { get; set; }
        public string SenderEmail { get; set; }
        public string AdminEmail { get; set; }
        public string SenderName { get; set; }


        public string Name { get; set; }
        public string Websiteurl { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool IsSSLEnabled { get; set; }
    }

 
}
