using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace LGC.GMES.MES.MainFrame.Security
{
    class AESCoder
    {
        private byte[] key = Convert.FromBase64String("MHdiMH+ph+BruVhJIVl0L2QDsp3QcalGpw2ZLFxJGJg=");
        private byte[] iv = Convert.FromBase64String("6FvWNAAB/Mhtcfu5wg2Rvw==");

        public string Encode(string val)
        {
            string returnValue = null;
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(val);
                        }
                        returnValue = Convert.ToBase64String(ms.ToArray());
                    }
                }
            }

            return returnValue;
        }

        public string Decode(string val)
        {
            string returnValue = null;

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(val)))
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(aes.Key, aes.IV), CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            returnValue = sr.ReadToEnd();
                        }
                    }
                }
            }

            return returnValue;
        }
    }
}
