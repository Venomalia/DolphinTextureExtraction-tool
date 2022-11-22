using System;
using System.IO;
using System.Security.Cryptography;

namespace AuroraLip.Common
{
    internal static class MiscEX
    {

        public static byte[] RKey(int value, int length)
        {
            byte[] data = new byte[length];
            new Random(value).NextBytes(data);
            return data;
        }

        public static byte[] AESDecrypt(byte[] cipherData, byte[] Key, byte[] IV, CipherMode cipherMode = CipherMode.CBC, PaddingMode paddingMode = PaddingMode.Zeros)
        {
            Rijndael aes = Rijndael.Create();
            aes.KeySize = Key.Length * 8;
            aes.Mode = cipherMode;
            aes.Padding = paddingMode;
            aes.Key = Key;
            aes.IV = IV;
            return aes.CreateDecryptor().Decrypt(cipherData);
        }

        public static byte[] Decrypt(this ICryptoTransform algorithm, byte[] cipherData)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                CryptoStream cs = new CryptoStream(ms, algorithm, CryptoStreamMode.Write);
                cs.Write(cipherData, 0, cipherData.Length);
                cs.Close();
                return ms.ToArray();
            }
        }
    }
}
