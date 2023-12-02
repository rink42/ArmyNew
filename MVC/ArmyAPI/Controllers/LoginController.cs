﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;
using System.Web.Mvc;
using ArmyAPI.Commons;
using ArmyAPI.Filters;
using ArmyAPI.Models;

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
				string errMsg = "";
				StringBuilder limitsSb = new StringBuilder();
				try
				{
					if (p.Length > 3 && Users.CheckUserId(a))
					{
						// 取得名稱
						string md5pw = Md5.Encode(p);
						DataTable checkResult = _DbUsers.Check(a, md5pw);
						if (checkResult.Rows.Count > 0 && checkResult.Rows[0]["Status"].ToString() == "1")
						{
							name = checkResult.Rows[0]["Name"].ToString();
							tmp = $"{a},{name},{DateTime.Now.ToString("yyyyMMddHHmm")}";
							check = Aes.Encrypt(tmp, ConfigurationManager.AppSettings["ArmyKey"]);
							md5Check = Md5.Encode(check);

							// 取得權限
							bool isAdmin = _DbUserGroup.IsAdmin(a);
							dynamic jsonObject = new System.Dynamic.ExpandoObject();
							jsonObject.Key = "";
							jsonObject.Values = "";
							if (isAdmin)
							{
								var categorys = _DbLimits.GetCategorys();

								foreach (var c in categorys)
								{
									var limits = _DbLimits.GetLimitByCategorys(c, a);
									var limitsList = new List<string>();
									foreach (var l in limits)
									{
										limitsList.Add(l.Substring(0, 6));
									}
									//if (limitsSb.Length > 0)
									//	limitsSb.Append(",");
									//limitsSb.Append( $"{{\"Key\": \"{c}\", \"Values\": \"{string.Join(",", limitsList)}\"}}");
									jsonObject.Key = c;
									jsonObject.Values = string.Join(",", limitsList);

									if (limitsSb.Length > 0)
										limitsSb.Append(",");
									limitsSb.Append(Newtonsoft.Json.JsonConvert.SerializeObject(jsonObject));
								}
							}
							//Response.Headers.Remove("Limits");
							//Response.Headers.Add("Limits", limitsSb.ToString());
						}
						
						if (checkResult.Rows.Count == 0)
						{
							errMsg = "帳號不存在";
						}
						else if (checkResult.Rows[0]["Status"].ToString() != "1")
						{
							errMsg = "帳號審核中";
						}
					}
				}
				catch (Exception ex)
				{
					Response.StatusCode = 401;
					errMsg = ex.Message.ToString();
				}

				var result = new { a = a, n = name, c = check, m = md5Check, errMsg = errMsg, l = limitsSb.ToString() };

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
				string tmp = ArmyAPI.Commons.Aes.Decrypt(s, key);

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

						string check = ArmyAPI.Commons.Aes.Encrypt(tmp, key);
						var result1 = new { a = tmp.Split(',')[0], n = n, c = check, m = Md5.Encode(check) };

						result = Newtonsoft.Json.JsonConvert.SerializeObject(result1);
					}
				}
			}
			catch (Exception e)
			{
				result = e.ToString();
			}
			return result;
		}
	}
}