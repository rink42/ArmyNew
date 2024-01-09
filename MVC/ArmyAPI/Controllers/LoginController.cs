using ArmyAPI.Commons;
using ArmyAPI.Data;
using ArmyAPI.Filters;
using ArmyAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
			string fName = "Login _ChkAccPwd";
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			string errMsg = "";
			string tmp = "";
			string check = "";
			string md5Check = "";
			string name = null;
			StringBuilder limitsSb = new StringBuilder();

			UserDetail user = (new ArmyAPI.Controllers.UserController()).GetDetailByUserId(a);
            user.IsAdmin = (new ArmyAPI.Commons.BaseController())._DbUserGroup.IsAdmin(a);
            WriteLog.Log($"[{fName}] user = {JsonConvert.SerializeObject(user)}");
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
					WriteLog.Log($"[{fName}] Login: {a}");
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
					WriteLog.Log($"[{fName}] AD Checked, isAuthenticated = {isAuthenticated}");

					if (isAD && !isAuthenticated)
						errMsg = "帳號密碼錯誤(AD)";
					else
					{
						try
						{
							// 密碼長度 > 3 碼 且 檢查 ID 是否符合身份証編碼規則
							if (p.Length > 3 && Users.CheckUserId(a))
							{
								// 如果是申請中(剛註冊要先設定這個狀態) 或 未申請(status == null)
								// 讓他能登入到申請人事權限頁面
								//Users.Statuses? status = _DbUsers.GetStatus(a);
								Users.Statuses? status = null;
								if (user.Status != null)
									status = (Users.Statuses)user.Status;

								string loginIP = (new Globals()).GetUserIpAddress();
								WriteLog.Log($"[{fName}] ID={a}, IP={loginIP}, AD={isAD}, Status={status}");

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
									{
										string cacheKey = $"User_{a}";
										Globals._Cache.Remove(cacheKey);
										Globals._Cache.Add(cacheKey, user, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddHours(8) });

										// 把單位表抓出放在 Cache 中
										cacheKey = "ArmyUnits";
										List<ArmyUnits> armyUnits = Globals._Cache.Get(cacheKey) as List<ArmyUnits>;
										if (armyUnits == null)
										{
											Globals._Cache.Remove(cacheKey);
											armyUnits = (new ArmyAPI.Controllers.LimitsController()).GetNewArmyUnit1();
											Globals._Cache.Add(cacheKey, armyUnits, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddHours(8) });
											//WriteLog.Log(JsonConvert.SerializeObject(armyUnits));
										}
									}
									// 有勾全軍
									// 組合 官科 條件
									// 組合 階級 條件
									// 組合 單位 條件
									string permissions = "";
									string groupTmp = "";
									string rankTmp = "";
									string unitTmp = "";
									if (user.Limits2HasUnitType(UserDetailLimits.UnitTypes.全軍))
									{
										// 有勾選全軍
										// 官科
										if (!string.IsNullOrEmpty(user.TGroups))
										{
											groupTmp = $"Army.dbo.v_member_data.group_code IN ('{user.TGroups.Replace(",", "','")}')";
										}

										foreach (UserDetailLimits.UnitTypes value in Enum.GetValues(typeof(UserDetailLimits.UnitTypes)))
										{
											// 階級
											if (value.GetDescription() == "階級")
											{
												var w1 = user.Limits2.Find(_l2 => _l2.HasLimit(value));
												if (w1 != null)
												{
													string tmp1 = w1.GetWhereByType(value);
													if (!string.IsNullOrEmpty(tmp1))
													{
														if (!string.IsNullOrEmpty(rankTmp))
															rankTmp += " OR ";
														rankTmp += $"({tmp1})";
													}
												}
											}

											// 單位
											if (value.GetDescription() == "單位")
											{
												var w2 = user.Limits2.Find(_l2 => _l2.HasLimit(value));
												if (w2 != null)
												{
													string tmp2 = w2.GetWhereByType(value);
													if (!string.IsNullOrEmpty(tmp2))
													{
														if (!string.IsNullOrEmpty(unitTmp))
															unitTmp += " OR ";
														unitTmp += $"({tmp2})";
													}
												}
											}
										}

										if (!string.IsNullOrEmpty(user.Units))
										{
											permissions = $"(Army.dbo.v_member_data.unit_code NOT IN ('{user.Units.Replace(",", "','")}'){(groupTmp.Length > 0 ? " AND " : "")}{groupTmp}{(rankTmp.Length > 0 ? " AND " : "")}{rankTmp}) OR (Army.dbo.v_member_data.unit_code IN ('{user.Units.Replace(",", "','")}'){(rankTmp.Length > 0 ? " AND " : "")}{rankTmp})";
											WriteLog.Log($"permissions = {permissions}");
										}
									}

									//// 使用LINQ過濾空字串
									//List<string> nonEmptyTexts = permissions.Where(text => !string.IsNullOrEmpty(text)).ToList();

									//// 將過濾後的元素用 " AND " 組合成一個字串
									//string nonEmptyPermissions = string.Join(" AND ", nonEmptyTexts);
									//WriteLog.Log($"nonEmptyPermissions = {nonEmptyPermissions}");

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
								else if (status == Users.Statuses.Disable)
								{
									errMsg = "帳號已停用";
								}
								else if (user.IPAddr1 != loginIP && user.IPAddr2 != loginIP)
								{
									errMsg = "登入 IP 不符";
								}
								else
								{
									errMsg = "未知錯誤";
								}
								WriteLog.Log($"[{fName}] {DateTime.Now.ToString("HH:mm:ss")} Login Finish");

								////TableauConfig
								//XML_TableauConfig xmlTableau = new XML_TableauConfig();
								//WriteLog.Log(JsonConvert.SerializeObject(xmlTableau.GetAll()));
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

			sb.Append(JsonConvert.SerializeObject(result));

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