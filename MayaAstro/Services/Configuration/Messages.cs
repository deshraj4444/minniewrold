namespace MayaAstro.Services.Configuration
{
    public class Messages
    {
        public string InternalServerError { get; set; }
        public string InValidLogin { get; set; }
        public string UsersNotExists { get; set; }
        public string SavedSuccessfully { get; set; }
        public string UpdatedSuccessfully { get; set; }
        public string DeleteSuccessfully { get; set; }
        public string ForgotPasswordLinkSentSuccessfully { get; set; }
        public int SaltByteSize { get; set; }
        public int HashByteSize { get; set; }
        public int Pbkdf2Iterations { get; set; }
        public string EncryptDecryptKey { get; set; }
        public string PasswordHasBeenChangedSucessfully { get; set; }
    }
}
