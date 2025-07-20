/// 설  명: AES 128비트 암호화 라이브러리, UTF-8 인코딩
/// 사용예
///     AESEncrypter enc = new AESEncrypter("별도제공된 암호화 키", "별도제공된 암호화 키");
///     string strEncrypted = enc.Encrypt("dotnetsoft");
///     string strDecrypted = enc.Encrypt(strEncrypted);
/// </summary>

using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace LGC.GMES.MES.CMM001.Class
{
    /// <summary>
    /// AES 128비트 암호화 라이브러리, UTF-8 인코딩
    /// </summary>
    public class AESEncrypter
    {
        private System.Text.UTF8Encoding utf8Encoding;
        private RijndaelManaged rijndael;
        private RijndaelManaged rijndaelForDB;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="key">16자리 키값</param>
        /// <param name="initialVector">16자리 벡터값</param>
        public AESEncrypter(string key = GAppSettings.AESKey, string initialVector = GAppSettings.AESVector)
        {
            if (key == null || key == "")
                throw new ArgumentException("The key can not be null or an empty string.", "key");

            if (initialVector == null || initialVector == "")
                throw new ArgumentException("The initial vector can not be null or an empty string.", "initialVector");


            // This is an encoder which converts a string into a UTF-8 byte array.
            this.utf8Encoding = new System.Text.UTF8Encoding();


            // Create a AES algorithm.
            this.rijndael = new RijndaelManaged();

            // Set cipher and padding mode.
            this.rijndael.Mode = CipherMode.ECB;
            //this.rijndael.Padding = PaddingMode.None;

            // Set key and block size.
            const int chunkSize = 128;

            this.rijndael.KeySize = chunkSize;
            this.rijndael.BlockSize = chunkSize;

            // PORTAL에서는 이것을 활성화 하여 사용
            this.rijndael.Key = generateKey(key);
            this.rijndael.IV = generateKey(key);

        }

        public string Encrypt(string value)
        {
            if (value == null)
                value = "";

            // Get an encryptor interface.
            ICryptoTransform transform = this.rijndael.CreateEncryptor();

            // Get a UTF-8 byte array from a unicode string.
            byte[] utf8Value = this.utf8Encoding.GetBytes(value);

            // Encrypt the UTF-8 byte array.
            byte[] encryptedValue = transform.TransformFinalBlock(utf8Value, 0, utf8Value.Length);

            // Return a base64 encoded string of the encrypted byte array.
            return Convert.ToBase64String(encryptedValue);
        }

        public string Decrypt(string value)
        {
            if (value == null || value == "")
                throw new ArgumentException("The cipher string can not be null or an empty string.");

            //인코딩된 값에 '+'가 들어 있으면 ' '으로 넘어오게 되므로 치환한다.
            value = value.Replace(' ', '+');

            // Get an decryptor interface.
            ICryptoTransform transform = rijndael.CreateDecryptor();

            // Get an encrypted byte array from a base64 encoded string.
            byte[] encryptedValue = Convert.FromBase64String(value);

            // Decrypt the byte array.
            byte[] decryptedValue = transform.TransformFinalBlock(encryptedValue, 0, encryptedValue.Length);

            // Return a string converted from the UTF-8 byte array.
            return this.utf8Encoding.GetString(decryptedValue);
        }

        private static byte[] generateKey(String key)
        {
            byte[] desKey = new byte[16];
            byte[] bkey = Encoding.UTF8.GetBytes(key);

            if (bkey.Length < desKey.Length)
            {
                Array.Copy(bkey, 0, desKey, 0, bkey.Length);
                //System.arraycopy(bkey, 0, desKey, 0, bkey.Length);

                for (int i = bkey.Length; i < desKey.Length; i++)
                    desKey[i] = 0;
            }
            else
            {
                Array.Copy(bkey, 0, desKey, 0, desKey.Length);
                //System.arraycopy(bkey, 0, desKey, 0, desKey.length);
            }

            return desKey;
        }

        public String AESEncrypt128ForDB(string value)
        {
            RijndaelManaged rijndaelForDB = new RijndaelManaged();

            // Set cipher and padding mode.
            rijndaelForDB.Mode = CipherMode.CBC;
            rijndaelForDB.Padding = PaddingMode.PKCS7;

            // Set key and block size.
            const int chunkSize = 128;

            rijndaelForDB.KeySize = chunkSize;
            rijndaelForDB.BlockSize = chunkSize;

            // PORTAL에서는 이것을 활성화 하여 사용
            rijndaelForDB.Key = generateKey(GAppSettings.AESKey);
            rijndaelForDB.IV = generateKey(GAppSettings.AESKey);


            // Get an encryptor interface.
            ICryptoTransform transform = rijndaelForDB.CreateEncryptor();

            // Get a UTF-8 byte array from a unicode string.
            byte[] utf8Value = this.utf8Encoding.GetBytes(value);

            // Encrypt the UTF-8 byte array.
            byte[] encryptedValue = transform.TransformFinalBlock(utf8Value, 0, utf8Value.Length);

            // Return a base64 encoded string of the encrypted byte array.
            return Convert.ToBase64String(encryptedValue);
        }

        private static UTF8Encoding encUTF8 = new System.Text.UTF8Encoding();
        public static String AES128Encrypt(string value)
        {
            RijndaelManaged rijndaelForDB = new RijndaelManaged();

            // Set cipher and padding mode.
            rijndaelForDB.Mode = CipherMode.CBC;
            rijndaelForDB.Padding = PaddingMode.PKCS7;

            // Set key and block size.
            const int chunkSize = 128;

            rijndaelForDB.KeySize = chunkSize;
            rijndaelForDB.BlockSize = chunkSize;

            // PORTAL에서는 이것을 활성화 하여 사용
            rijndaelForDB.Key = generateKey(GAppSettings.AESKey);
            rijndaelForDB.IV = generateKey(GAppSettings.AESKey);


            // Get an encryptor interface.
            ICryptoTransform transform = rijndaelForDB.CreateEncryptor();

            // Get a UTF-8 byte array from a unicode string.
            byte[] utf8Value = encUTF8.GetBytes(value);

            // Encrypt the UTF-8 byte array.
            byte[] encryptedValue = transform.TransformFinalBlock(utf8Value, 0, utf8Value.Length);

            // Return a base64 encoded string of the encrypted byte array.
            return Convert.ToBase64String(encryptedValue);
        }

        private static readonly string KEY_128 = GAppSettings.AESKey.Substring(0, 128 / 8);
        public static String AES128Decrypt(string value)
        {
            try
            {
                string encryptString = value;
                //base64
                byte[] encryptBytes = Convert.FromBase64String(encryptString);

                //레인달
                RijndaelManaged rm = new RijndaelManaged();

                // Set cipher and padding mode.
                rm.Mode = CipherMode.CBC;
                rm.Padding = PaddingMode.PKCS7;
                rm.KeySize = 128;

                // 메모리스트림 생성
                MemoryStream memoryStream = new MemoryStream(encryptBytes);

                // key, iv값 정의
                ICryptoTransform decryptor = rm.CreateDecryptor(Encoding.UTF8.GetBytes(KEY_128), Encoding.UTF8.GetBytes(KEY_128));
                CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

                // 복호화된 데이터를 담을 바이트 배열을 선언
                byte[] plainBytes = new byte[encryptBytes.Length];
                int plainCount = cryptoStream.Read(plainBytes, 0, plainBytes.Length);

                //복호화된 바이트배열을 string 변환
                string plainString = Encoding.UTF8.GetString(plainBytes, 0, plainCount);

                cryptoStream.Close();
                memoryStream.Close();

                // Return a base64 encoded string of the encrypted byte array.
                return plainString;
            }
            //catch(Exception decErr)
            catch
            {

                return string.Empty;
            }
        }
        public class GAppSettings
        {
            public const string AESKey = "ThisIsIkepSecurityKey";  //- LG화학 G-POTAL ID 암호화용 
            public const string AESVector = "ThisIsIkepSecurityKey";  //- LG화학 G-POTAL ID 암호화용 
        }
    }
}
