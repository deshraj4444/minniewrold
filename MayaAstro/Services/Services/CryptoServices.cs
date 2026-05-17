using MayaAstro.Services.Configuration;
using MayaAstro.Services.Services;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using System.Security.Cryptography;
using System.Text;

namespace MayaAstro.Services.Services
{
  
        public class CryptoService : ICryptoServices
        {
            private readonly IOptions<Messages> _messages;

            public CryptoService(IOptions<Messages> messages)
            {
                _messages = messages;
            }

            public async Task<string> HashPassword(string password)
            {
                var cryptoProvider = new RNGCryptoServiceProvider();
                var salt = new byte[_messages.Value.SaltByteSize];
                cryptoProvider.GetBytes(salt);

                var hash = await GetPbkdf2Bytes(password, salt, _messages.Value.Pbkdf2Iterations, _messages.Value.HashByteSize);
                return await Task.FromResult(Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash));
            }

            public async Task<string> GetHash(string userProfileId)
            {
                var cryptoProvider = new RNGCryptoServiceProvider();
                var salt = new byte[12];
                cryptoProvider.GetBytes(salt);
                return await Task.FromResult(Convert.ToBase64String(salt));
            }

            public async Task<bool> ValidatePassword(string password, string salt, string hashPassword)
            {
                var saltByte = Convert.FromBase64String(salt);
                var hash = Convert.FromBase64String(hashPassword);
                var testHash = await GetPbkdf2Bytes(password, saltByte, Constants.Constants.Pbkdf2Iterations, hash.Length);
                return await SlowEquals(hash, testHash);
            }

            private static async Task<bool> SlowEquals(byte[] a, byte[] b)
            {
                var diff = (uint)a.Length ^ (uint)b.Length;
                for (var i = 0; i < a.Length && i < b.Length; i++) diff |= (uint)(a[i] ^ b[i]);
                return await Task.FromResult(diff == 0);
            }

            private static async Task<byte[]> GetPbkdf2Bytes(string password, byte[] salt, int iterations, int outputBytes)
            {
                var pbkdf2 = new Rfc2898DeriveBytes(password, salt) { IterationCount = iterations };
                return await Task.FromResult(pbkdf2.GetBytes(outputBytes));
            }

            public async Task<string> Encrypt(string text)
            {
                var key = Encoding.UTF8.GetBytes(_messages.Value.EncryptDecryptKey);

                using (var aesAlg = Aes.Create())
                {
                    using (var encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV))
                    {
                        using (var msEncrypt = new MemoryStream())
                        {
                            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                            using (var swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(text);
                            }

                            var iv = aesAlg.IV;

                            var decryptedContent = msEncrypt.ToArray();

                            var result = new byte[iv.Length + decryptedContent.Length];

                            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                            Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);

                            return await Task.FromResult(Convert.ToBase64String(result));
                        }
                    }
                }
            }

            public async Task<string> Decrypt(string encrypted)
            {
                var fullCipher = Convert.FromBase64String(encrypted);

                var iv = new byte[16];
                var cipher = new byte[16];

                Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, iv.Length);
                var key = Encoding.UTF8.GetBytes(_messages.Value.EncryptDecryptKey);

                using (var aesAlg = Aes.Create())
                {
                    using (var decryptor = aesAlg.CreateDecryptor(key, iv))
                    {
                        string result;
                        using (var msDecrypt = new MemoryStream(cipher))
                        {
                            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                            {
                                using (var srDecrypt = new StreamReader(csDecrypt))
                                {
                                    result = srDecrypt.ReadToEnd();
                                }
                            }
                        }

                        return await Task.FromResult(result);
                    }
                }
            }



    }
}
