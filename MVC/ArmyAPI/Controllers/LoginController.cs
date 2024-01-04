using ArmyAPI.Commons;
using ArmyAPI.Filters;
using ArmyAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Caching;
using System.Text;
using System.Web.Mvc;
using Aes = ArmyAPI.Commons.Aes;

namespace ArmyAPI.Controllers
{
    public class LoginController : BaseController
    {
		#region ContentResult Check(string a, string p)
		[CheckUserIDFilter("a")]
		public ContentResult Check(string a, string p)
		{
			return _ChkAccPwd(a, p);
		}
		#endregion ContentResult Check(string a, string p)

		#region private ContentResult _ChkAccPwd(string a, string p)
		private ContentResult _ChkAccPwd(string a, string p)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			string check1 = Md5.Encode(a + p);
			string errMsg = "";
			string tmp = "";
			string check = "";
			string md5Check = "";
			string name = null;
			StringBuilder limitsSb = new StringBuilder();

			UserDetail user = (new ArmyAPI.Controllers.UserController()).GetDetailByUserId(a);
			WriteLog.Log($"{check1} user = {JsonConvert.SerializeObject(user)}");
			if (string.IsNullOrEmpty(user.UserID))
				errMsg = "無此帳號";
			else
			{
				// 檢查 Army.dbo.v_member_dara.non_es_cdoe 有值，代表「編外」
				// 檢查 Army.dbo.v_member_data.unit_cdoe != ArmyWeb.dbo.Users.UnitCode

				// 檢查最後登入時間，超過2個月直接鎖定帳號
				bool isOK = _DbUsers.CheckLastLoginDate(a);

				// 產生 SessionKey ( 帳號+當前時間yyyyMMddHHmm
				// 再檢查 DB
				if (isOK)
				{
					bool isAuthenticated = false;
					bool isAD = false;
					WriteLog.Log($"{check1} Login: {a}");
					bool isExistInAD = false;
					// LDAP 驗証
					if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("CheckAD")) && ConfigurationManager.AppSettings.Get("CheckAD") == "1")
					{
						// 如果存在帳號再往下驗証
						isExistInAD = Globals.CheckUserExistence(a);
						if (isExistInAD)
						{
							isAuthenticated = Globals.ValidateCredentials(ConfigurationManager.AppSettings.Get("AD_Domain"), a, p);
							isAD = true;
						}
					}
					WriteLog.Log($"{check1} AD Checked, isAuthenticated = {isAuthenticated}");

					if (isAD && !isAuthenticated)
						errMsg = "帳號密碼錯誤(AD)";
					else
					{
						try
						{
							if (p.Length > 3 && Users.CheckUserId(a))
							{
								// 如果是申請中(剛註冊要先設定這個狀態) 或 未申請(status == null)
								// 讓他能登入到申請人事權限頁面
								//Users.Statuses? status = _DbUsers.GetStatus(a);
								Users.Statuses? status = null;
								if (user.Status != null)
									status = (Users.Statuses)user.Status;

								string loginIP = (new Globals()).GetUserIpAddress();
								WriteLog.Log($"{check1} ID={a}, IP={loginIP}, AD={isAD}");

								if ((ConfigurationManager.AppSettings.Get("CheckIpPassA").IndexOf(Md5.Encode(a)) >= 0 || (user.IPAddr1 == loginIP || user.IPAddr2 == loginIP)) || status == null || status == Users.Statuses.InProgress || status == Users.Statuses.InReview)
								{
									// 取得名稱
									string md5pw = "";
									if (!isAD)
										md5pw = Md5.Encode(p);

									if (ConfigurationManager.AppSettings.Get("CheckIpPassA").IndexOf(Md5.Encode(a)) >= 0)
										isAD = true;

									if (isAD == false && user.PP.Equals(md5pw) == false)
										user = null;
									else
										Globals._Cache.Add($"User_{a}", user, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(3600) });

									if (user != null && (user.Status == null || user.Status == 0 || user.Status == 1 || user.Status == -1))
									{
										_DbUsers.UpdateLastLoginDate(user);

										name = user.Name;
										tmp = $"{a},{name},{DateTime.Now.ToString("yyyyMMddHHmm")}";
										check = Aes.Encrypt(tmp, ConfigurationManager.AppSettings["ArmyKey"]);
										md5Check = Md5.Encode(check);

										if (user.Limits2 != null)
										{
											// 提取 Limits2 并转换为 List<Limit2Item>
											dynamic l = new System.Dynamic.ExpandoObject();
											l.Key = "";
											l.Values = "";
											foreach (var item in user.Limits2)
											{
												l.Key = item.Key;
												l.Values = string.Join(",", item.Values);
												if (limitsSb.Length > 0)
													limitsSb.Append(",");
												limitsSb.Append(Newtonsoft.Json.JsonConvert.SerializeObject(l));
											}
										}
									}
									else if (user != null && user.Status != null && user.Status == -3)
										errMsg = user.Review;

									if (user == null)
									{
										errMsg = "帳號不存在";
									}
									//else if (user.Status == 0)
									//{
									//	errMsg = "帳號審核中";
									//}
								}
								else
								{
									errMsg = "登入 IP 不符";
								}
								WriteLog.Log($"{DateTime.Now.ToString("HH:mm:ss")} Login Finish");
							}
						}
						catch (Exception ex)
						{
							//Response.StatusCode = 401;
							errMsg = "檢查帳密發生錯誤";

							WriteLog.Log(errMsg, ex.ToString());
						}
					}
				}
				else
					errMsg = "登入時間間隔超過 2 個月";
			}

			var result = new { a = a, n = name, c = check, m = md5Check, errMsg = errMsg, l = limitsSb.ToString(), s = (user != null) ? user.Status.ToString() : "" };

			sb.Append(Newtonsoft.Json.JsonConvert.SerializeObject(result));

			return this.Content(sb.ToString(), "application/json");
		}
		#endregion private ContentResult _ChkAccPwd(string a, string p)

		#region string CheckSession(string c)
		[ControllerAuthorizationFilter]
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

		#region bool CheckSession1(string ReturnUrl)
		[ControllerAuthorizationFilter]
		[ActionName("CheckSession")]
		public bool CheckSession1(string ReturnUrl)
		{
			WriteLog.Log($"ReturnUrl = {ReturnUrl}");
			return true;
		}
		#endregion bool CheckSession1(string ReturnUrl)

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
				//WriteLog.Log($"a = {a}");
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
				WriteLog.Log("LoginController, CheckSession", e.ToString());
			}
			return result;
		}
	}
}