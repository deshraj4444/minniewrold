namespace MayaAstro.Services.Services
{
    public interface ICryptoServices
    {
        Task<string> HashPassword(string password);
        Task<bool> ValidatePassword(string password, string salt, string hashPassword);
        Task<string> GetHash(string userProfileId);
        Task<string> Encrypt(string text);
        Task<string> Decrypt(string encrypted);
    }
}
