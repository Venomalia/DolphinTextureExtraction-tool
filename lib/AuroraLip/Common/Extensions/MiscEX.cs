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

        public static void AESDecrypt(byte[] cipherData, byte[] key, byte[] IV, CipherMode cipherMode = CipherMode.CBC, PaddingMode paddingMode = PaddingMode.Zeros)
        {
            Aes aes = Aes.Create();
            aes.KeySize = key.Length * 8;
            aes.Mode = cipherMode;
            aes.Padding = paddingMode;
            aes.Key = key;
            aes.IV = IV;
            aes.CreateDecryptor().TransformBlock(cipherData, 0, cipherData.Length, cipherData,0);
        }

        public static void AESEncrypt(byte[] cipherData, byte[] key, byte[] IV, CipherMode cipherMode = CipherMode.CBC, PaddingMode paddingMode = PaddingMode.Zeros)
        {
            Aes aes = Aes.Create();
            aes.KeySize = key.Length * 8;
            aes.Mode = cipherMode;
            aes.Padding = paddingMode;
            aes.Key = key;
            aes.IV = IV;
            aes.CreateEncryptor().TransformBlock(cipherData, 0, cipherData.Length, cipherData, 0);
        }
    }
}
