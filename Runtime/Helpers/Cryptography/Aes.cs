using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Device;

namespace CipherDuo.Ethereum.Cryptography
{
    public static class Aes
    {
        /// AES LOGICS ///
        private const int AesKeySize = 16;
        private static readonly string GUIDKEY = CalculateGUID();

        public static async Task<string> AesEncryptAsync(string data)
        {
            return await AesEncryptAsync(data, Encoding.UTF7.GetBytes(GUIDKEY));
        }

        public static async Task<string> AesDecryptAsync(string data)
        {
            return await AesDecryptAsync(data, Encoding.UTF7.GetBytes(GUIDKEY));
        }

        public static async Task<string> AesEncryptAsync(string data, string password)
        {
            return await AesEncryptAsync(data, Encoding.UTF7.GetBytes(password));
        }

        public static async Task<string> AesDecryptAsync(string data, string password)
        {
            return await AesDecryptAsync(data, Encoding.UTF7.GetBytes(password));
        }


        public static async Task<string> AesEncryptAsync(string data, byte[] key)
        {
            return Convert.ToBase64String(await AesEncrypt(Encoding.UTF7.GetBytes(data), key));
        }

        public static async Task<string> AesDecryptAsync(string data, byte[] key)
        {
            return Encoding.UTF7.GetString(await AesDecrypt(Convert.FromBase64String(data), key));
        }

        public static async Task<string> AesEncryptAsync(byte[] data, string password)
        {
            return Encoding.UTF7.GetString(await AesEncrypt(data, Encoding.UTF7.GetBytes(password)));
        }

        public static async Task<byte[]> AesDecryptAsync(byte[] data, string password)
        {
            return await AesDecrypt(data, Encoding.UTF7.GetBytes(password));
        }

        static Task<byte[]> AesEncrypt(byte[] data, byte[] key)
        {
            if (data == null || data.Length <= 0)
            {
                throw new ArgumentNullException($"{nameof(data)} cannot be empty");
            }

            if (key == null || key.Length != AesKeySize)
            {
                throw new ArgumentException($"{nameof(key)} must be length of {AesKeySize}");
            }

            using (var aes = new AesCryptoServiceProvider
            {
                Key = key,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            })
            {
                aes.GenerateIV();
                var iv = aes.IV;
                using (var encrypter = aes.CreateEncryptor(aes.Key, iv))
                using (var cipherStream = new MemoryStream())
                {
                    using (var tCryptoStream = new CryptoStream(cipherStream, encrypter, CryptoStreamMode.Write))
                    using (var tBinaryWriter = new BinaryWriter(tCryptoStream))
                    {
                        // prepend IV to data
                        cipherStream.Write(iv, 0, iv.Length);
                        tBinaryWriter.Write(data);
                        tCryptoStream.FlushFinalBlock();
                    }
                    var cipherBytes = cipherStream.ToArray();

                    return Task.Run(() => { return cipherBytes; });
                }
            }
        }

        private static Task<byte[]> AesDecrypt(byte[] data, byte[] key)
        {
            if (data == null || data.Length <= 0)
            {
                throw new ArgumentNullException($"{nameof(data)} cannot be empty");
            }

            if (key == null || key.Length != AesKeySize)
            {
                throw new ArgumentException($"{nameof(key)} must be length of {AesKeySize}");
            }

            using (var aes = new AesCryptoServiceProvider
            {
                Key = key,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            })
            {
                // get first KeySize bytes of IV and use it to decrypt
                var iv = new byte[AesKeySize];
                Array.Copy(data, 0, iv, 0, iv.Length);

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(aes.Key, iv), CryptoStreamMode.Write))
                    using (var binaryWriter = new BinaryWriter(cs))
                    {
                        // decrypt cipher text from data, starting just past the IV
                        binaryWriter.Write(
                            data,
                            iv.Length,
                            data.Length - iv.Length
                        );
                    }

                    var dataBytes = ms.ToArray();

                    return Task.Run(() => { return dataBytes; });
                }
            }
        }

        //TODO Substitute with SHA2(CSP(SystemInfo.deviceUniqueIdentifier)) digest ~ check if deterministic
        private static string CalculateGUID()
        {
            var guid = SystemInfo.deviceUniqueIdentifier;
            var gLength = guid.Length;
            if (gLength < AesKeySize)
            {
                var diff = AesKeySize - gLength;
                for (int i = gLength; i < diff; i++)
                {
                    guid = guid.Insert(i, guid[diff - i].ToString());
                }
                return guid;
            }
            if (gLength > AesKeySize)
            {
                var diff = gLength - AesKeySize;
                guid = guid.Remove(0, diff);
                return guid;
            }
            return guid;
        }


    }


    
    // Possibly a better alternative

    // public class AesAlternative
    // { 
    //
    //     public struct EncryptedDataPayload
    //     {
    //         byte[] m_contentHash;
    //         byte[] m_peerHash;
    //         byte[] m_accountAddress;
    //         string m_password;
    //
    //         public EncryptedDataPayload(byte[] contentHash, byte[] peerHash, byte[] accountAdr, string password)
    //         {
    //             m_contentHash = contentHash;
    //             m_peerHash = peerHash;
    //             m_accountAddress = accountAdr;
    //             m_password = password;
    //         }
    //     }
    //
    //     private static readonly SecureRandom Random = new SecureRandom();
    //
    //     //Preconfigured Encryption Parameters
    //     public static readonly int NonceBitSize = 128;
    //     public static readonly int MacBitSize = 128;
    //     public static readonly int KeyBitSize = 256;
    //
    //     //Preconfigured Password Key Derivation Parameters
    //     public static readonly int SaltBitSize = 128;
    //     public static readonly int Iterations = 10000;
    //     public static readonly int MinPasswordLength = 12;
    //
    //
    //     /// <summary>
    //     /// Helper that generates a random new key on each call.
    //     /// </summary>
    //     /// <returns></returns>
    //     public static byte[] NewKey()
    //     {
    //         var key = new byte[KeyBitSize / 8];
    //         Random.NextBytes(key);
    //         return key;
    //     }
    //
    //     /// <summary>
    //     /// Simple Encryption And Authentication (AES-GCM) of a UTF8 string.
    //     /// </summary>
    //     /// <param name="secretMessage">The secret message.</param>
    //     /// <param name="key">The key.</param>
    //     /// <param name="nonSecretPayload">Optional non-secret payload.</param>
    //     /// <returns>
    //     /// Encrypted Message
    //     /// </returns>
    //     /// <exception cref="System.ArgumentException">Secret Message Required!;secretMessage</exception>
    //     /// <remarks>
    //     /// Adds overhead of (Optional-Payload + BlockSize(16) + Message +  HMac-Tag(16)) * 1.33 Base64
    //     /// </remarks>
    //     public static string SimpleEncrypt(string secretMessage, byte[] key, byte[] nonSecretPayload = null)
    //     {
    //         if (string.IsNullOrEmpty(secretMessage))
    //             throw new ArgumentException("Secret Message Required!", "secretMessage");
    //
    //         var plainText = Encoding.UTF8.GetBytes(secretMessage);
    //         var cipherText = SimpleEncrypt(plainText, key, nonSecretPayload);
    //         return Convert.ToBase64String(cipherText);
    //     }
    //
    //
    //     /// <summary>
    //     /// Simple Decryption & Authentication (AES-GCM) of a UTF8 Message
    //     /// </summary>
    //     /// <param name="encryptedMessage">The encrypted message.</param>
    //     /// <param name="key">The key.</param>
    //     /// <param name="nonSecretPayloadLength">Length of the optional non-secret payload.</param>
    //     /// <returns>Decrypted Message</returns>
    //     public static string SimpleDecrypt(string encryptedMessage, byte[] key, int nonSecretPayloadLength = 0)
    //     {
    //         if (string.IsNullOrEmpty(encryptedMessage))
    //             throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");
    //
    //         var cipherText = Convert.FromBase64String(encryptedMessage);
    //         var plainText = SimpleDecrypt(cipherText, key, nonSecretPayloadLength);
    //         return plainText == null ? null : Encoding.UTF8.GetString(plainText);
    //     }
    //
    //     /// <summary>
    //     /// Simple Encryption And Authentication (AES-GCM) of a UTF8 String
    //     /// using key derived from a password (PBKDF2).
    //     /// </summary>
    //     /// <param name="secretMessage">The secret message.</param>
    //     /// <param name="password">The password.</param>
    //     /// <param name="nonSecretPayload">The non secret payload.</param>
    //     /// <returns>
    //     /// Encrypted Message
    //     /// </returns>
    //     /// <remarks>
    //     /// Significantly less secure than using random binary keys.
    //     /// Adds additional non secret payload for key generation parameters.
    //     /// </remarks>
    //     public static string SimpleEncryptWithPassword(string secretMessage, string password,
    //                              byte[] nonSecretPayload = null)
    //     {
    //         if (string.IsNullOrEmpty(secretMessage))
    //             throw new ArgumentException("Secret Message Required!", "secretMessage");
    //
    //         var plainText = Encoding.UTF8.GetBytes(secretMessage);
    //         var cipherText = SimpleEncryptWithPassword(plainText, password, nonSecretPayload);
    //         return Convert.ToBase64String(cipherText);
    //     }
    //
    //
    //     /// <summary>
    //     /// Simple Decryption and Authentication (AES-GCM) of a UTF8 message
    //     /// using a key derived from a password (PBKDF2)
    //     /// </summary>
    //     /// <param name="encryptedMessage">The encrypted message.</param>
    //     /// <param name="password">The password.</param>
    //     /// <param name="nonSecretPayloadLength">Length of the non secret payload.</param>
    //     /// <returns>
    //     /// Decrypted Message
    //     /// </returns>
    //     /// <exception cref="System.ArgumentException">Encrypted Message Required!;encryptedMessage</exception>
    //     /// <remarks>
    //     /// Significantly less secure than using random binary keys.
    //     /// </remarks>
    //     public static string SimpleDecryptWithPassword(string encryptedMessage, string password,
    //                              int nonSecretPayloadLength = 0)
    //     {
    //         if (string.IsNullOrWhiteSpace(encryptedMessage))
    //             throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");
    //
    //         var cipherText = Convert.FromBase64String(encryptedMessage);
    //         var plainText = SimpleDecryptWithPassword(cipherText, password, nonSecretPayloadLength);
    //         return plainText == null ? null : Encoding.UTF8.GetString(plainText);
    //     }
    //
    //     public static byte[] SimpleEncrypt(byte[] secretMessage, byte[] key, byte[] nonSecretPayload = null)
    //     {
    //         //User Error Checks
    //         if (key == null || key.Length != KeyBitSize / 8)
    //             throw new ArgumentException(String.Format("Key needs to be {0} bit!", KeyBitSize), "key");
    //
    //         if (secretMessage == null || secretMessage.Length == 0)
    //             throw new ArgumentException("Secret Message Required!", "secretMessage");
    //
    //         //Non-secret Payload Optional
    //         nonSecretPayload = nonSecretPayload ?? new byte[] { };
    //
    //         //Using random nonce large enough not to repeat
    //         var nonce = new byte[NonceBitSize / 8];
    //         Random.NextBytes(nonce, 0, nonce.Length);
    //
    //         var cipher = new GcmBlockCipher(new AesEngine());
    //         var parameters = new AeadParameters(new KeyParameter(key), MacBitSize, nonce, nonSecretPayload);
    //         cipher.Init(true, parameters);
    //
    //         //Generate Cipher Text With Auth Tag
    //         var cipherText = new byte[cipher.GetOutputSize(secretMessage.Length)];
    //         var len = cipher.ProcessBytes(secretMessage, 0, secretMessage.Length, cipherText, 0);
    //         cipher.DoFinal(cipherText, len);
    //
    //         //Assemble Message
    //         using (var combinedStream = new MemoryStream())
    //         {
    //             using (var binaryWriter = new BinaryWriter(combinedStream))
    //             {
    //                 //Prepend Authenticated Payload
    //                 binaryWriter.Write(nonSecretPayload);
    //                 //Prepend Nonce
    //                 binaryWriter.Write(nonce);
    //                 //Write Cipher Text
    //                 binaryWriter.Write(cipherText);
    //             }
    //             return combinedStream.ToArray();
    //         }
    //     }
    //
    //     public static byte[] SimpleDecrypt(byte[] encryptedMessage, byte[] key, int nonSecretPayloadLength = 0)
    //     {
    //         //User Error Checks
    //         if (key == null || key.Length != KeyBitSize / 8)
    //             throw new ArgumentException(String.Format("Key needs to be {0} bit!", KeyBitSize), "key");
    //
    //         if (encryptedMessage == null || encryptedMessage.Length == 0)
    //             throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");
    //
    //         using (var cipherStream = new MemoryStream(encryptedMessage))
    //         using (var cipherReader = new BinaryReader(cipherStream))
    //         {
    //             //Grab Payload
    //             var nonSecretPayload = cipherReader.ReadBytes(nonSecretPayloadLength);
    //
    //             //Grab Nonce
    //             var nonce = cipherReader.ReadBytes(NonceBitSize / 8);
    //
    //             var cipher = new GcmBlockCipher(new AesEngine());
    //             var parameters = new AeadParameters(new KeyParameter(key), MacBitSize, nonce, nonSecretPayload);
    //             cipher.Init(false, parameters);
    //
    //             //Decrypt Cipher Text
    //             var cipherText = cipherReader.ReadBytes(encryptedMessage.Length - nonSecretPayloadLength - nonce.Length);
    //             var plainText = new byte[cipher.GetOutputSize(cipherText.Length)];
    //
    //             try
    //             {
    //                 var len = cipher.ProcessBytes(cipherText, 0, cipherText.Length, plainText, 0);
    //                 cipher.DoFinal(plainText, len);
    //
    //             }
    //             catch (InvalidCipherTextException)
    //             {
    //                 //Return null if it doesn't authenticate
    //                 return null;
    //             }
    //
    //             return plainText;
    //         }
    //
    //     }
    //
    //     public static byte[] SimpleEncryptWithPassword(byte[] secretMessage, string password, byte[] nonSecretPayload = null)
    //     {
    //         nonSecretPayload = nonSecretPayload ?? new byte[] { };
    //
    //         //User Error Checks
    //         if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
    //             throw new ArgumentException(String.Format("Must have a password of at least {0} characters!", MinPasswordLength), "password");
    //
    //         if (secretMessage == null || secretMessage.Length == 0)
    //             throw new ArgumentException("Secret Message Required!", "secretMessage");
    //
    //         var generator = new Pkcs5S2ParametersGenerator();
    //
    //         //Use Random Salt to minimize pre-generated weak password attacks.
    //         var salt = new byte[SaltBitSize / 8];
    //         Random.NextBytes(salt);
    //
    //         generator.Init(
    //           PbeParametersGenerator.Pkcs5PasswordToBytes(password.ToCharArray()),
    //           salt,
    //           Iterations);
    //
    //         //Generate Key
    //         var key = (KeyParameter)generator.GenerateDerivedMacParameters(KeyBitSize);
    //
    //         //Create Full Non Secret Payload
    //         var payload = new byte[salt.Length + nonSecretPayload.Length];
    //         Array.Copy(nonSecretPayload, payload, nonSecretPayload.Length);
    //         Array.Copy(salt, 0, payload, nonSecretPayload.Length, salt.Length);
    //
    //         return SimpleEncrypt(secretMessage, key.GetKey(), payload);
    //     }
    //
    //     public static byte[] SimpleDecryptWithPassword(byte[] encryptedMessage, string password, int nonSecretPayloadLength = 0)
    //     {
    //         //User Error Checks
    //         if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
    //             throw new ArgumentException(String.Format("Must have a password of at least {0} characters!", MinPasswordLength), "password");
    //
    //         if (encryptedMessage == null || encryptedMessage.Length == 0)
    //             throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");
    //
    //         var generator = new Pkcs5S2ParametersGenerator();
    //
    //         //Grab Salt from Payload
    //         var salt = new byte[SaltBitSize / 8];
    //         Array.Copy(encryptedMessage, nonSecretPayloadLength, salt, 0, salt.Length);
    //
    //         generator.Init(
    //           PbeParametersGenerator.Pkcs5PasswordToBytes(password.ToCharArray()),
    //           salt,
    //           Iterations);
    //
    //         //Generate Key
    //         var key = (KeyParameter)generator.GenerateDerivedMacParameters(KeyBitSize);
    //
    //         return SimpleDecrypt(encryptedMessage, key.GetKey(), salt.Length + nonSecretPayloadLength);
    //     }
    // }
}


