using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using ArmyAPI.Commons;
using ArmyAPI.Filters;

namespace ArmyAPI.Controllers
{
	public class LoginController : BaseController
    {
		#region ContentResult Check(string a, string p)
		public ContentResult Check(string a, string p)
		{
			return _ChkAccPwd(a, p);
		}
		#endregion ContentResult Check(string a, string p)

		#region private ContentResult _ChkAccPwd(string a, string p)
		private ContentResult _ChkAccPwd(string a, string p)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			// LDAP 驗証


			// 帳密登入
			if (sb.Length == 0)
			{
				string name = "";
				// 產生 SessionKey ( 帳號+當前時間yyyyMMddHHmm
				string tmp = "";
				string check = "";
				string md5Check = "";
				try
				{
					if (p.Length == 32)
					{
						// 取得名稱
						name = _DbUsers.Check(a, p);
						tmp = $"{a},{name},{DateTime.Now.ToString("yyyyMMddHHmm")}";
						check = AES.Encrypt(tmp, ConfigurationManager.AppSettings["ArmyKey"]);
						md5Check = Md5.Encode(check);
						//var info = new SystemManagementController().SystemUserBasic("", "", "", "true", a, "", "", "");
						//var infoContent = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>((info as System.Web.Http.Results.OkNegotiatedContentResult<string>).Content);

						//if (infoContent.Count > 0)
						//	name = (string)infoContent[0].NAME;


						// 取得權限
						string limits = "123";

						Response.Headers.Add("Limits", limits);
					}
				}
				catch
				{
				}

				var result = new { a = a, n = name, c = check, m = md5Check };

				sb.Append(Newtonsoft.Json.JsonConvert.SerializeObject(result));
			}

			return this.Content(sb.ToString(), "application/json");
		}
		#endregion private ContentResult _ChkAccPwd(string a, string p)

		#region string CheckSession(string c)
		[CustomAuthorizationFilter]
		[HttpPost]
		public string CheckSession(string c)
		{
			//string s = "";
			//string headerKey = "Army";

			//if (Request.Headers.AllKeys.Contains(headerKey))
			//	s = Request.Headers[headerKey];

			//string result = CheckSession(c, s);

			//return result;
			return "";
		}
		#endregion string CheckSession(string c)

		[NonAction]
		public string CheckSession(string c, string s)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			// Session_LegalMinute

			string result = "檢查不通過";

			try
			{
				string key = ConfigurationManager.AppSettings["ArmyKey"];
				string tmp = AES.Decrypt(s, key);

				int commonFirst = tmp.IndexOf(',');
				int commonLast = tmp.LastIndexOf(',');

				string a = tmp.Substring(0, commonFirst);
				string n = tmp.Substring(commonFirst + 1, commonLast - commonFirst - 1);
				if (c.Equals(Md5.Encode(s)))
				{
					string tmp1 = tmp.Substring(commonLast + 1);
					DateTime t = DateTime.Parse($"{tmp1.Substring(0, 4)}-{tmp1.Substring(4, 2)}-{tmp1.Substring(6, 2)} {tmp1.Substring(8, 2)}:{tmp1.Substring(10, 2)}");

					if (((TimeSpan)(DateTime.Now - t)).TotalMinutes > int.Parse(ConfigurationManager.AppSettings["Session_LegalMinute"]))
						result = "超時";
					else
					{
						tmp = $"{a},{n},{DateTime.Now.ToString("yyyyMMddHHmm")}";

						string check = AES.Encrypt(tmp, key);
						var result1 = new { a = tmp.Split(',')[0], n = n, c = check, m = Md5.Encode(check) };

						result = Newtonsoft.Json.JsonConvert.SerializeObject(result1);
					}
				}
			}
			catch
			{
			}
			return result;
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