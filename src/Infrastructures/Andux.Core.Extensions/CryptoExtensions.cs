using System.IO.Compression;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Andux.Core.Extensions
{
    /// <summary>
    /// 加密解密扩展方法
    /// </summary>
    public static class CryptoExtensions
    {
        /// <summary>
        /// 计算字符串的MD5哈希值（32位小写）
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>MD5哈希值</returns>
        public static string ToMD5(this string input)
        {
            using var md5 = MD5.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = md5.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// 计算字符串的SHA256哈希值
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>SHA256哈希值</returns>
        public static string ToSHA256(this string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// 计算字符串的HMAC-SHA256签名
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <param name="secretKey">密钥</param>
        /// <returns>Base64编码的签名</returns>
        public static string ToHMACSHA256(this string input, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var inputBytes = Encoding.UTF8.GetBytes(input);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(inputBytes);
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// AES加密（CBC模式，PKCS7填充）
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="key">密钥（32字节）</param>
        /// <param name="iv">IV向量（16字节）</param>
        /// <returns>Base64编码的密文</returns>
        public static string AESEncrypt(this string plainText, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                using var sw = new StreamWriter(cs);
                sw.Write(plainText);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// AES解密（CBC模式，PKCS7填充）
        /// </summary>
        /// <param name="cipherText">Base64编码的密文</param>
        /// <param name="key">密钥（32字节）</param>
        /// <param name="iv">IV向量（16字节）</param>
        /// <returns>解密后的明文</returns>
        public static string AESDecrypt(this string cipherText, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }

        /// <summary>
        /// 生成随机盐值
        /// </summary>
        /// <param name="length">盐值长度（字节）</param>
        /// <returns>Base64编码的盐值</returns>
        public static string GenerateSalt(int length = 16)
        {
            var salt = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return Convert.ToBase64String(salt);
        }

        /// <summary>
        /// 使用PBKDF2生成密码哈希
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="salt">盐值</param>
        /// <param name="iterations">迭代次数</param>
        /// <param name="hashByteSize">哈希长度（字节）</param>
        /// <returns>Base64编码的哈希值</returns>
        public static string PBKDF2Hash(
            this string password,
            string salt,
            int iterations = 10000,
            int hashByteSize = 32)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                Convert.FromBase64String(salt),
                iterations);
            return Convert.ToBase64String(pbkdf2.GetBytes(hashByteSize));
        }

        /// <summary>
        /// 生成随机密码
        /// </summary>
        /// <param name="length">密码长度</param>
        /// <param name="includeSpecialChars">是否包含特殊字符</param>
        /// <returns>随机密码</returns>
        public static string GenerateRandomPassword(
            int length = 12,
            bool includeSpecialChars = true)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            const string specialChars = "!@#$%^&*()_-+=[{]};:<>|./?";

            var chars = includeSpecialChars
                ? validChars + specialChars
                : validChars;

            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);

            var result = new StringBuilder(length);
            foreach (var b in bytes)
            {
                result.Append(chars[b % chars.Length]);
            }
            return result.ToString();
        }

        #region Base64
        /// <summary>
        /// Base64 URL安全编码（替换+/为-_，去除末尾=）
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>URL安全的Base64字符串</returns>
        public static string ToBase64Url(this string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var base64 = Convert.ToBase64String(bytes);
            return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        /// <summary>
        /// Base64 URL安全解码
        /// </summary>
        /// <param name="base64Url">URL安全的Base64字符串</param>
        /// <returns>解码后的原始字符串</returns>
        public static string FromBase64Url(this string base64Url)
        {
            var base64 = base64Url.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            var bytes = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(bytes);
        }
        #endregion

        #region 非对称加密（RSA）
        /// <summary>
        /// RSA加密（使用公钥）
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="publicKey">PEM格式公钥</param>
        /// <returns>Base64编码的密文</returns>
        public static string RSAEncrypt(this string plainText, string publicKey)
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKey);
            var bytes = Encoding.UTF8.GetBytes(plainText);
            var encrypted = rsa.Encrypt(bytes, RSAEncryptionPadding.OaepSHA256);
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// RSA解密（使用私钥）
        /// </summary>
        /// <param name="cipherText">Base64编码的密文</param>
        /// <param name="privateKey">PEM格式私钥</param>
        /// <returns>解密后的明文</returns>
        public static string RSADecrypt(this string cipherText, string privateKey)
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(privateKey);
            var bytes = Convert.FromBase64String(cipherText);
            var decrypted = rsa.Decrypt(bytes, RSAEncryptionPadding.OaepSHA256);
            return Encoding.UTF8.GetString(decrypted);
        }
        #endregion

        #region 数字签名

        /// <summary>
        /// 生成RSA签名
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="privateKey">PEM格式私钥</param>
        /// <returns>Base64编码的签名</returns>
        public static string RSASignData(this string data, string privateKey)
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(privateKey);
            var bytes = Encoding.UTF8.GetBytes(data);
            var signature = rsa.SignData(bytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signature);
        }

        /// <summary>
        /// 验证RSA签名
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="signature">Base64编码的签名</param>
        /// <param name="publicKey">PEM格式公钥</param>
        /// <returns>验证结果</returns>
        public static bool RSAVerifyData(this string data, string signature, string publicKey)
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKey);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var sigBytes = Convert.FromBase64String(signature);
            return rsa.VerifyData(dataBytes, sigBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        #endregion

        #region 证书操作

        /// <summary>
        /// 从X509证书获取公钥（PEM格式）
        /// </summary>
        /// <param name="certificate">X509证书</param>
        /// <returns>PEM格式公钥</returns>
        public static string GetPublicKeyPem(this X509Certificate2 certificate)
        {
            var publicKey = certificate.GetRSAPublicKey();
            return publicKey.ExportSubjectPublicKeyInfoPem();
        }

        #endregion

        #region 安全随机数

        /// <summary>
        /// 生成加密安全的随机整数
        /// </summary>
        /// <param name="minValue">最小值（包含）</param>
        /// <param name="maxValue">最大值（不包含）</param>
        /// <returns>随机数</returns>
        public static int CryptographicallySecureRandom(int minValue, int maxValue)
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var rand = BitConverter.ToUInt32(bytes, 0);
            return (int)(rand % (maxValue - minValue)) + minValue;
        }

        #endregion

        #region ZIP压缩加密

        /// <summary>
        /// 使用AES加密压缩字节数组
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="password">压缩密码</param>
        /// <returns>加密压缩后的数据</returns>
        public static byte[] ZipWithAes(this byte[] data, string password)
        {
            using var outputMs = new MemoryStream();
            using (var zip = new ZipArchive(outputMs, ZipArchiveMode.Create, true))
            {
                var entry = zip.CreateEntry("data.bin");
                using var entryStream = entry.Open();
                using var aes = Aes.Create();
                aes.Key = DeriveKey(password, aes.IV);

                using var cryptoStream = new CryptoStream(
                    entryStream,
                    aes.CreateEncryptor(),
                    CryptoStreamMode.Write);
                cryptoStream.Write(data, 0, data.Length);
            }
            return outputMs.ToArray();
        }

        private static byte[] DeriveKey(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            return pbkdf2.GetBytes(32); // 256-bit key
        }

        #endregion

    }
}
