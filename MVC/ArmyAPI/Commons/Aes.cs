using System.IO;
using System.Security.Cryptography;
using System;
using System.Text;

namespace ArmyAPI.Commons
{
	public class Aes
	{
		public static string Encrypt(string plainText, string password)
		{
			byte[] iv = (new ASCIIEncoding()).GetBytes(password);
			using (var md5 = MD5.Create())
			{
				iv = md5.ComputeHash(iv);
			}
			byte[] array;
			password = ValidateKeyLength(password, 32);

			using (System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create())
			{
				aes.Mode = CipherMode.CFB;
				aes.Key = Encoding.UTF8.GetBytes(password);
				aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

				using (MemoryStream memoryStream = new MemoryStream())
				{
					using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
					{
						using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
						{
							streamWriter.Write(plainText);
						}

						array = memoryStream.ToArray();
					}
				}
			}

			return Convert.ToBase64String(array);
		}

		public static string Decrypt(string cipherText, string password)
		{
			byte[] iv = (new ASCIIEncoding()).GetBytes(password);
			using (var md5 = MD5.Create())
			{
				iv = md5.ComputeHash(iv);
			}
			byte[] buffer = Convert.FromBase64String(cipherText);
			password = ValidateKeyLength(password, 32);
			using (System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create())
			{
				aes.Mode = CipherMode.CFB;
				aes.Key = Encoding.UTF8.GetBytes(password);
				aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

				using (MemoryStream memoryStream = new MemoryStream(buffer))
				{
					using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
					{
						using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
						{
							return streamReader.ReadToEnd();
						}
					}
				}
			}
		}

		private static string ValidateKeyLength(string key, int requiredLength)
		{
			// 如果密钥不足指定长度,用0填充到指定长度
			if (key.Length < requiredLength)
			{
				key = key.PadRight(requiredLength, '0');
			}
			// 如果密钥超过指定长度,截取指定长度的字节
			else if (key.Length > requiredLength)
			{
				key = key.Substring(0, requiredLength);
			}

			return key;
		}
	}
}