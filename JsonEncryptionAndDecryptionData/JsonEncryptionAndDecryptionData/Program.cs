using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Cryptography;

internal class Program
{
    public static List<Dictionary<string, string>> data;
    public static string fileName = "deneme-data.enc"; // Dosya adı uzantısı .enc
    public static string password = "deneme"; // Şifreleme ve şifre çözme için kullanılacak parola
    public static string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName); //Dosya yolu

    public static void Main(string[] args)
    {
        data = new List<Dictionary<string, string>>();

        data.Add(new Dictionary<string, string>
                {
                    { "Anahtar1", "Değer1" },
                    { "Anahtar2", "Değer2" }
                });

        // Önce veriyi şifrele
        JsonEncryption();

        // Ardından veriyi çöz
        DecryptionData();

        Console.ReadLine(); // Uygulamanın kapanmaması için bekleyin.
    }
    public static void JsonEncryption()
    {
        try
        {
            string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented); //Şifrelenecek data

            byte[] iv = new byte[16];
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.IV = iv;

            }

            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.KeySize = 256;
                aesAlg.BlockSize = 128;
                aesAlg.Mode = CipherMode.CFB;
                aesAlg.Padding = PaddingMode.PKCS7;

                Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(password, iv, 1000);
                aesAlg.Key = keyDerivation.GetBytes(aesAlg.KeySize / 8);

                using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, iv))
                {
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        msEncrypt.Write(iv, 0, iv.Length);

                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(jsonData);
                            }
                        }

                        File.WriteAllBytes(filePath, msEncrypt.ToArray());

                        Console.WriteLine("Data encrypted and saved successfully!");
                    }
                }
            }
        }
        catch (Exception ex) { Console.WriteLine("An error occurred while encrypting and saving data: " + ex.Message); }
    }

    public static void DecryptionData()
    {
        try
        {
            byte[] encryptedData = File.ReadAllBytes(filePath);
            byte[] iv = new byte[16];
            Array.Copy(encryptedData, iv, iv.Length);

            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.KeySize = 256;
                aesAlg.BlockSize = 128;
                aesAlg.Mode = CipherMode.CFB; // CipherMode.CFB kullanılıyor

                aesAlg.Padding = PaddingMode.PKCS7; // Padding modu ayarlanıyor

                Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(password, iv, 1000);
                aesAlg.Key = keyDerivation.GetBytes(aesAlg.KeySize / 8);

                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, iv))
                {
                    using (MemoryStream msDecrypt = new MemoryStream(encryptedData, iv.Length, encryptedData.Length - iv.Length))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                string jsonData = srDecrypt.ReadToEnd();
                                Console.WriteLine("Data decrypted successfully!");
                                Console.WriteLine("Decrypted JSON Data: " + jsonData);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex) { Console.WriteLine("An error occurred while decrypting data: " + ex.Message); }
    }
}