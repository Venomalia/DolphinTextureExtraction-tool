using System.Security.Cryptography;

namespace AuroraLib.Common
{
    public static class MiscEX
    {
        public static byte[] RKey(int value, int length)
        {
            byte[] data = new byte[length];
            new Random(value).NextBytes(data);
            return data;
        }

        public static byte[] AESDecrypt(byte[] cipherData, byte[] Key, byte[] IV, CipherMode cipherMode = CipherMode.CBC, PaddingMode paddingMode = PaddingMode.Zeros)
        {
            Aes aes = Aes.Create();
            aes.KeySize = Key.Length * 8;
            aes.Mode = cipherMode;
            aes.Padding = paddingMode;
            aes.Key = Key;
            aes.IV = IV;
            return aes.CreateDecryptor().Decrypt(cipherData);
        }

        public static byte[] AESEncrypt(byte[] cipherData, byte[] Key, byte[] IV, CipherMode cipherMode = CipherMode.CBC, PaddingMode paddingMode = PaddingMode.Zeros)
        {
            Aes aes = Aes.Create();
            aes.KeySize = Key.Length * 8;
            aes.Mode = cipherMode;
            aes.Padding = paddingMode;
            aes.Key = Key;
            aes.IV = IV;
            return aes.CreateEncryptor().Decrypt(cipherData);
        }

        public static byte[] Decrypt(this ICryptoTransform algorithm, byte[] cipherData)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                CryptoStream cs = new(ms, algorithm, CryptoStreamMode.Write);
                cs.Write(cipherData, 0, cipherData.Length);
                cs.Close();
                return ms.ToArray();
            }
        }
    }
}
