using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace ArmyAPI.Controllers
{
    public class LoginController : Controller
    {
		public string Check(string a, string p)
		{
			return _ChkAccPwd(a, p);
		}

		private string _ChkAccPwd(string a, string p)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			// LDAP 驗証
			if (sb.Length == 0)
			{
				string name = "";
				try
				{
					// 取得名稱
					//var info = new SystemManagementController().SystemUserBasic("", "", "", "true", a, "", "", "");
					//var infoContent = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>((info as System.Web.Http.Results.OkNegotiatedContentResult<string>).Content);

					//if (infoContent.Count > 0)
					//	name = (string)infoContent[0].NAME;
				}
				catch
				{
				}

				// 產生 SessionKey ( 帳號+當前時間yyyyMMddHHmm
				string tmp = $"{a},{name},{DateTime.Now.ToString("yyyyMMddHHmm")}";

				string check = AES.Encrypt(tmp, System.Web.Configuration.WebConfigurationManager.AppSettings["SGTPKey"]);
				var result = new { a = a, n = name, c = check, m = Md5.Encode(check) };

				sb.Append(Newtonsoft.Json.JsonConvert.SerializeObject(result));
			}

			return sb.ToString();
		}
	}

	public class Md5
	{

		public static string Encode(string input)
		{
			return Encode(Encoding.Default.GetBytes(input));
		}

		public static string Encode(byte[] input)
		{
			// Create a new instance of the MD5CryptoServiceProvider object.
			System.Security.Cryptography.MD5 s1 = System.Security.Cryptography.MD5.Create();

			// Create a new Stringbuilder to collect the bytes
			// and create a string.
			StringBuilder sBuilder = new StringBuilder();

			// Convert the input string to a byte array and compute the hash.
			byte[] data = s1.ComputeHash(input);

			// Loop through each byte of the hashed data 
			// and format each one as a hexadecimal string.
			for (int i = 0; i < data.Length; i++)
			{
				sBuilder.Append(data[i].ToString("X2"));
			}

			// Return the hexadecimal string.
			return sBuilder.ToString();
		}
	}

	public static class AES
	{
		/// <summary>
		/// 字串加密(非對稱式)
		/// </summary>
		/// <param name="Source">加密前字串</param>
		/// <param name="CryptoKey">加密金鑰</param>
		/// <returns>加密後字串</returns>
		public static string Encrypt(string SourceStr, string CryptoKey)
		{
			string encrypt = "";
			try
			{
				AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
				MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
				SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
				byte[] key = sha256.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));
				byte[] iv = md5.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));
				aes.Key = key;
				aes.IV = iv;

				byte[] dataByteArray = Encoding.UTF8.GetBytes(SourceStr);
				using (MemoryStream ms = new MemoryStream())
				using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
				{
					cs.Write(dataByteArray, 0, dataByteArray.Length);
					cs.FlushFinalBlock();
					encrypt = Convert.ToBase64String(ms.ToArray());
				}
			}
			catch (Exception e)
			{
				throw e;
			}
			return encrypt;
		}

		/// <summary>
		/// 字串解密(非對稱式)
		/// </summary>
		/// <param name="Source">解密前字串</param>
		/// <param name="CryptoKey">解密金鑰</param>
		/// <returns>解密後字串</returns>
		public static string Decrypt(string SourceStr, string CryptoKey)
		{
			string decrypt = "";
			try
			{
				AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
				MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
				SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
				byte[] key = sha256.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));
				byte[] iv = md5.ComputeHash(Encoding.UTF8.GetBytes(CryptoKey));
				aes.Key = key;
				aes.IV = iv;

				byte[] dataByteArray = Convert.FromBase64String(SourceStr);
				using (MemoryStream ms = new MemoryStream())
				{
					using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
					{
						cs.Write(dataByteArray, 0, dataByteArray.Length);
						cs.FlushFinalBlock();
						decrypt = Encoding.UTF8.GetString(ms.ToArray());
					}
				}
			}
			catch (Exception e)
			{
				throw e;
			}
			return decrypt;
		}

	}
}