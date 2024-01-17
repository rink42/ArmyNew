using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using ArmyAPI.Commons;
using ArmyAPI.Data;
using ArmyAPI.Filters;
using ArmyAPI.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;

namespace ArmyAPI.Controllers
{
	public class UserController : BaseController
    {
		#region ContentResult GetAll()
		[ControllerAuthorizationFilter]
		[HttpPost]
		public ContentResult GetAll()
		{
			string loginId = HttpContext.Items["LoginId"] as string;
			bool isAdmin = (HttpContext.Items["IsAdmin"] as bool?) ?? false;

			List<UserDetail> uds = _DbUsers.GetDetails(isAdmin);

			JsonSerializerSettings settings = !isAdmin ? new JsonSerializerSettings { ContractResolver = new CustomContractResolver("Process", "Outcome") } : null;

			return this.Content(JsonConvert.SerializeObject(uds, settings), "application/json");
		}
		#endregion ContentResult GetAll()

		#region //ActionResult Register(string userId, string p)
		//[HttpPost]
		//[CheckUserIDFilter("userId")]

		//public ActionResult Register(string userId, string p)
		//{
		//	return Register(userId, p, null);
		//}
		#endregion //ActionResult Register(string userId, string p)

		#region ActionResult Register(string userId, string p, bool checkAD)
		[HttpPost]
		[CheckUserIDFilter("userId")]

		public ActionResult Register(string userId, string p, bool checkAD)
		{
			Users user = new Users();
			string result = "";
			try
			{
				// 如果 checkAD = true，先把帳密傳進AD檢查驗証
				// isAuthenticated = Globals.ValidateCredentials(ConfigurationManager.AppSettings.Get("AD_Domain"), a, p);
				// 如果 isAuthenticated = false 直接回傳「帳密錯誤」的訊息
				bool isAuthenticated = false;
				if (checkAD && (!string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("CheckAD")) && ConfigurationManager.AppSettings.Get("CheckAD") == "1"))
				{
					isAuthenticated = Globals.ValidateCredentials(ConfigurationManager.AppSettings.Get("AD_Domain"), userId, p);
					if (!isAuthenticated)
						result = "註冊失敗(AD帳密錯誤)";
				}

				if (!checkAD || (checkAD && isAuthenticated))
				{
					user.UserID = userId;
					if (!checkAD)
					{
						string md5pw = Md5.Encode(p);
						user.Password = md5pw;
					}
					ArmyUser armyUser = _DbArmy.GetUser(userId);

					if (armyUser != null)
						user.Name = armyUser.MemberName;
					else
						user.Name = "";

					user.IsAD = checkAD;

					result = _DbUsers.Add(user).ToString();

					if (result == "1")
						result = "註冊成功";
					else if (result == "-1")
						result = "帳號已存在";
					else
						result = "註冊失敗1";
				}
			}
			catch (Exception ex)
			{
				result = "註冊失敗2";
				WriteLog.Log(result, ex.ToString());
			}

			return this.Content(result, "text/plain");
		}
		#endregion ActionResult Register(string userId, string p, bool checkAD)

		#region ActionResult Register(string userId, string p, string name, string rank, string title, string skill, string ip1, string ip2, string email, string phoneMil, string phone)
		[ControllerAuthorizationFilter]
		[HttpPost]
		[ActionName("RegisterFull")]
		[CheckUserIDFilter("userId")]

		public ActionResult Register(string userId, string name, string rank, string title, string skill, string ip1, string ip2, string email, string phoneMil, string phone)
		{
			string result = "";
			Users user = new Users();
			try
			{
				user.UserID = userId;
				user.Name = name;
				user.Rank = rank;
				user.Title = title;
				user.Skill = skill;
				user.IPAddr1 = ip1;
				user.IPAddr2 = ip2;
				user.Email = email;
				user.PhoneMil = phoneMil;
				user.Phone = phone;
				user.Status = (short)Users.Statuses.InProgress;

				result = _DbUsers.UpdateFull(user).ToString();

				if (result == "1")
					result = "註冊成功";
				else if (result == "-1")
					result = "帳號已存在";
				else
					result = "註冊失敗";
			}
			catch (Exception ex)
			{
				result = "註冊失敗";
				WriteLog.Log(result, ex.ToString());
			}

			return this.Content(result, "text/plain");
		}
		#endregion ActionResult Register(string userId, string name, string rank, string title, string skill, string ip1, string ip2, string email, string phoneMil, string phone)

		#region ActionResult Delete(string userId)
		/// <summary>
		/// 刪除
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		[ControllerAuthorizationFilter]
		[HttpPost]
		[CheckUserIDFilter("userId")]

		public ActionResult Delete(string userId)
		{
			string loginId = HttpContext.Items["LoginId"] as string;
			string result = "";
			if (userId == loginId)
				result = "不能刪除自己";
			else
				result = _DbUsers.Delete(userId, loginId).ToString();

			return this.Content(result, "text/plain"); ;
		}
		#endregion ActionResult Delete(string userId)

		#region int Deletes(string userIds)
		/// <summary>
		/// 刪除
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		[ControllerAuthorizationFilter]
		[HttpPost]
		public int Deletes(string userIds)
		{
			string loginId = HttpContext.Items["LoginId"] as string;
			int result = 0;
			if (userIds.Split(',').Contains(loginId))
				Response.StatusCode = 401;
			else
				result = _DbUsers.Deletes(userIds, loginId);

			return result;
		}
		#endregion int Deletes(string userIds)

		#region ContentResult CheckUserData(string userId, string name, string birthday, string email, string phone)
		[HttpPost]
		[CheckUserIDFilter("userId")]

		public ContentResult CheckUserData(string userId, string name, string birthday, string email, string phone)
		{
			string result = _DbArmy.CheckUserData(userId, name, birthday, email, phone);
			
			int resultInt = 0;
			string errMsg = "";
			if (!int.TryParse(result, out resultInt))
			{
				resultInt = -1;
				errMsg = "查詢失敗";
			}
			else if (resultInt == -1)
				errMsg = "你是 AD 帳號，請洽單位資訊人員";
			else if (resultInt == 1)
			{
				errMsg = ArmyAPI.Commons.Aes.Encrypt($"{{\"u\":\"{userId}\", \"n\":\"{name}\", \"b\":\"{birthday}\", \"e\":\"{email}\", \"p\":\"{phone}\", \"t\":\"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}\"}}", ConfigurationManager.AppSettings.Get("ArmyKey"));
			}

			var result1 = new Class_Response { code = resultInt, errMsg = errMsg };

			string respon = Newtonsoft.Json.JsonConvert.SerializeObject(result1);

			return this.Content(respon, "application/json");
	}
		#endregion ContentResult CheckUserData(string userId, string name, string birthday, string email, string phone)

		#region string Update(string userId, string name, string ip1, string ip2, string email, string phoneMil, string phone)
		/// <summary>
		/// 更新
		/// </summary>
		/// <returns></returns>
		[ControllerAuthorizationFilter]
		[HttpPost]
		[CheckUserIDFilter("userId")]

		public string Update(string userId, string name, string ip1, string ip2, string email, string phoneMil, string phone)
		{
			string result = "";
			Users user = new Users();
			try
			{
				user.UserID = userId;
				user.Name = name;
				user.IPAddr1 = ip1;
				user.IPAddr2 = ip2;
				user.Email = email;
				user.PhoneMil = phoneMil;
				user.Phone = phone;

				result = _DbUsers.Update(user).ToString();
			}
			catch (Exception ex)
			{
				Response.StatusCode = 401;
				Response.Write(ex.Message);
			}

			return result;
		}
		#endregion string Update(string userId, string name, string ip1, string ip2, string email, string phoneMil, string phone)

		#region string UpdateDetail_NoLimits(string userId, string name, string rank, string title, string skill, string ip1, string ip2, string email, string phoneMil, string phone, byte? process, string reason, string review, byte? outcome)
		/// <summary>
		/// 更新
		/// </summary>
		/// <returns></returns>
		[ControllerAuthorizationFilter]
		[HttpPost]
		[CheckUserIDFilter("userId")]

		public string UpdateDetail_NoLimits(string userId, string name, string rank, string title, string skill, string ip1, string ip2, string email, string phoneMil, string phone, byte? process, string reason, string review, byte? outcome)
		{
			string loginId = HttpContext.Items["LoginId"] as string;
			bool isAdmin = (HttpContext.Items["IsAdmin"] as bool?) ?? false;

			string result = "";
			UserDetail user = new UserDetail();
			try
			{
				user.UserID = userId;
				user.Name = name;
				user.RankCode = rank;
				user.TitleCode = title;
				user.SkillCode = skill;
				user.IPAddr1 = ip1;

				if (isAdmin)
				{
					user.IPAddr2 = ip2;
					user.Process = process;
					user.Review = review;
					user.Outcome = outcome;
				}
				user.Email = email;
				user.PhoneMil = phoneMil;
				user.Phone = phone;
				user.Reason = reason;

				result = _DbUsers.UpdateDetail(user, isAdmin).ToString();
			}
			catch (Exception ex)
			{
				Response.StatusCode = 401;
				Response.Write(ex.Message);
			}

			return result;
		}
		#endregion string UpdateDetail_NoLimits(string userId, string name, string rank, string title, string skill, string ip1, string ip2, string email, string phoneMil, string phone, byte? process, string reason, string review, byte? outcome)

		#region string UpdateDetail(string userId, string name, string ip1, string ip2, string email, string phoneMil, string phone, string limits)
		/// <summary>
		/// 更新(含權限)
		/// </summary>
		/// <returns></returns>
		[ControllerAuthorizationFilter]
		[HttpPost]
		[CheckUserIDFilter("userId")]

		public string UpdateDetail(string userId, string name, string ip1, string ip2, string email, string phoneMil, string phone, string limits)
		{
			string result = Update(userId, name, ip1, ip2, email, phoneMil, phone);

			if (result == "1")
			{
				result = (1 + _DbLimitsUser.Update(userId, limits)).ToString();
			}

			return result;
		}
		#endregion string UpdateDetail(string userId, string name, string ip1, string ip2, string email, string phoneMil, string phone, string limits)

		#region string UpdateDetail_Limits(string userId, string name, string unitCode, string rank, string title, string skill, string ip1, string ip2, string email, string phoneMil, string phone, string limits1, string limits2, string tgroups, byte? process, string reason, string review, byte? outcome)
		/// <summary>
		/// 更新(含權限)
		/// </summary>
		/// <returns></returns>
		[ControllerAuthorizationFilter]
		[HttpPost]
		[CheckUserIDFilter("userId")]

		public string UpdateDetail_Limits(string userId, string name, string unitCode, string rank, string title, string skill, string ip1, string ip2, string email, string phoneMil, string phone, string limits1, string limits2, string tgroups, byte? process, string reason, string review, byte? outcome)
		{
			return UpdateDetail_Limits2(userId, name, unitCode, rank, title, skill, ip1, ip2, email, phoneMil, phone, limits1, limits2, tgroups, process, reason, review, outcome, "", "");
		}
		#endregion string UpdateDetail_Limits(string userId, string name, string unitCode, string rank, string title, string skill, string ip1, string ip2, string email, string phoneMil, string phone, string limits1, string limits2, string tgroups, byte? process, string reason, string review, byte? outcome)

		#region string UpdateDetail_Limits2(string userId, string name, string unitCode, string rank, string title, string skill, string ip1, string ip2, string email, string phoneMil, string phone, string limits1, string limits2, string tgroups, byte? process, string reason, string review, byte? outcome, string units, string pp1)
		/// <summary>
		/// 更新(含權限)
		/// </summary>
		/// <returns></returns>
		[ControllerAuthorizationFilter]
		[HttpPost]
		[CheckUserIDFilter("userId")]

		public string UpdateDetail_Limits2(string userId, string name, string unitCode, string rank, string title, string skill, string ip1, string ip2, string email, string phoneMil, string phone, string limits1, string limits2, string tgroups, byte? process, string reason, string review, byte? outcome, string units, string pp1)
		{
			string loginId = HttpContext.Items["LoginId"] as string;
			bool isAdmin = (HttpContext.Items["IsAdmin"] as bool?) ?? false;

			UserDetail user = new UserDetail();
			user.UserID = userId;
			user.Name = name;

			if (!string.IsNullOrEmpty(pp1) && pp1.Length == 32)
			{
				user.PP = pp1;
			}

			user.UnitCode = unitCode;
			user.RankCode = rank;
			user.TitleCode = title;
			user.SkillCode = skill;
			user.IPAddr1 = ip1;
			user.TGroups = tgroups;

			if (isAdmin)
			{
				user.IPAddr2 = ip2;
				user.Process = process;
				user.Review = review;
				user.Outcome = outcome;
			}
			user.Email = email;
			user.PhoneMil = phoneMil;
			user.Phone = phone;
			user.Reason = reason;

			dynamic menusUser = new { MenuIndexs = limits1, UserID = userId };

			dynamic limitCodes = new { LimitCodes = limits2, UserID = userId };
			// 要記申請日期
			DB_UpdateDetail_Limits db = new DB_UpdateDetail_Limits();
			int result1 = db.Run(user, menusUser, limitCodes, isAdmin);

			int result2 = _Db_s_User_Units.Inserts(units, userId);

			List<string> permissions = new List<string>();
			// 有勾全軍 + 有設官科: 設定的業管UnitCode以外的 AND v_member_data.group_code IN (user.TGroups) 聯集 設定的業管(UnitCode) 全部
			// 有勾全軍 + 有選階級: unit_code IN (Limits.Where)
			// 有勾全軍 + 業管: = 全軍
			if (user.Limits2HasUnitType(UserDetailLimits.UnitTypes.全軍))
			{
				if (!string.IsNullOrEmpty(user.TGroups))
				{
					permissions.Add($"Army.dbo.v_member_data.group_code IN ('{user.TGroups.Replace(",", "','")}')");
				}

				foreach (UserDetailLimits.UnitTypes value in Enum.GetValues(typeof(UserDetailLimits.UnitTypes)))
				{
					if (value.GetDescription() != "網站2")
					{
						var w1 = user.Limits2.Find(_l2 => _l2.HasLimit(value));
						if (w1 != null)
						{
							permissions.Add(w1.GetWhereByType(value));
						}
					}
				}
			}

			string result = $"{{'r1': {result1}, 'r2': {result2} }}";

			return result;
		}
		#endregion string UpdateDetail_Limits2(string userId, string name, string unitCode, string rank, string title, string skill, string ip1, string ip2, string email, string phoneMil, string phone, string limits1, string limits2, string tgroups, byte? process, string reason, string review, byte? outcome, string units, string pp1)

		#region string AddUpdateDetail_Limits2(string data)
		/// <summary>
		/// 更新(含權限)
		/// </summary>
		/// <returns></returns>
		[ControllerAuthorizationFilter]
		[HttpPost]
		[CheckUserIDFilter("userId")]

		public string AddUpdateDetail_Limits2(string data)
		{
			string loginId = HttpContext.Items["LoginId"] as string;
			bool isAdmin = (HttpContext.Items["IsAdmin"] as bool?) ?? false;

			dynamic dataObj = JsonConvert.DeserializeObject<ExpandoObject>(data);

			UserDetail user = new UserDetail();
			int result1 = 0;
			int result2 = 0;
			try
			{
				user.UserID = dataObj.userId;
				user.Name = dataObj.name;

				if (!string.IsNullOrEmpty(dataObj.pp1) && dataObj.pp1.Length == 32)
				{
					user.PP = dataObj.pp1;
				}

				user.UnitCode = dataObj.unitCode;
				user.RankCode = dataObj.rank;
				user.TitleCode = dataObj.title;
				user.SkillCode = dataObj.skill;
				user.IPAddr1 = dataObj.ip1;
				user.TGroups = dataObj.tgroups;

				if (isAdmin)
				{
					user.IPAddr2 = dataObj.ip2;
					user.Process = dataObj.process;
					user.Review = dataObj.review;
					user.Outcome = dataObj.outcome;
				}
				user.Email = dataObj.email;
				user.PhoneMil = dataObj.phoneMil;
				user.Phone = dataObj.phone;
				user.Reason = dataObj.reason;

				dynamic menusUser = new { MenuIndexs = dataObj.limits1, UserID = dataObj.userId };

				dynamic limitCodes = new { LimitCodes = dataObj.limits2, UserID = dataObj.userId };

				user.IsAD = dataObj.isAD;

				// 席位帳號
				user.IsSeat = dataObj.isSeat;

				user.StartDate = dataObj.startDate;
				user.EndDate = dataObj.endDate;

				// 要記申請日期
				DB_AddUpdateDetail_Limits db = new DB_AddUpdateDetail_Limits();
				result1 = db.Run(user, menusUser, limitCodes, isAdmin);

				result2 = _Db_s_User_Units.Inserts(dataObj.units, dataObj.userId);

				List<string> permissions = new List<string>();
				// 有勾全軍 + 有設官科: 設定的業管UnitCode以外的 AND v_member_data.group_code IN (user.TGroups) 聯集 設定的業管(UnitCode) 全部
				// 有勾全軍 + 有選階級: unit_code IN (Limits.Where)
				// 有勾全軍 + 業管: = 全軍
				if (user.Limits2HasUnitType(UserDetailLimits.UnitTypes.全軍))
				{
					if (!string.IsNullOrEmpty(user.TGroups))
					{
						permissions.Add($"Army.dbo.v_member_data.group_code IN ('{user.TGroups.Replace(",", "','")}')");
					}

					foreach (UserDetailLimits.UnitTypes value in Enum.GetValues(typeof(UserDetailLimits.UnitTypes)))
					{
						if (value.GetDescription() != "網站2")
						{
							var w1 = user.Limits2.Find(_l2 => _l2.HasLimit(value));
							if (w1 != null)
							{
								permissions.Add(w1.GetWhereByType(value));
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				WriteLog.Log("AddUpdateDetail_Limits2 error", ex.ToString());
			}
			string result = $"{{'r1': {result1}, 'r2': {result2} }}";

			return result;
		}
		#endregion string AddUpdateDetail_Limits2(string data)

		#region string UpdateStatus(string userId, short? status)
		/// <summary>
		/// 更新
		/// </summary>
		/// <returns></returns>
		[ControllerAuthorizationFilter]
		[HttpPost]
		[CheckUserIDFilter("userId")]

		public string UpdateStatus(string userId, short? status)
		{
			string result = "";
			Users user = new Users();
			try
			{
				user.UserID = userId;
				user.Status = status;

				result = _DbUsers.UpdateStatus(user).ToString();
			}
			catch (Exception ex)
			{
				Response.StatusCode = 401;
				Response.Write(ex.Message);
			}

			return result;
		}
		#endregion string UpdateStatus(string userId, short? status)

		#region string UpdateStatuses(string userIds, short? status)
		/// <summary>
		/// 更新
		/// </summary>
		/// <returns></returns>
		[ControllerAuthorizationFilter]
		[HttpPost]
		public string UpdateStatuses(string userIds, short? status)
		{
			string result = "";
			try
			{
				Users.Statuses? euStatus = null;

				if (status != null)
				{
					var values = Enum.GetValues(typeof(Users.Statuses)).Cast<Users.Statuses>();

					if ((short)values.Min() <= status && status <= (short)values.Max())
						euStatus = (Users.Statuses)status;
					else
						throw new Exception("Status 的值不存在");
				}

				result = _DbUsers.UpdateStatuses(userIds, euStatus).ToString();
			}
			catch (Exception ex)
			{
				//Response.StatusCode = 401;
				Response.Write(ex.Message);
			}

			return result;
		}
		#endregion string UpdateStatus(string userIds, short? status)

		#region string UpdateGroupID(string userId, int groupId)
		/// <summary>
		/// 更新
		/// </summary>
		/// <returns></returns>
		[ControllerAuthorizationFilter]
		[HttpPost]
		[CheckUserIDFilter("userId")]

		public string UpdateGroupID(string userId, int groupId)
		{
			string result = "";
			Users user = new Users();
			try
			{
				user.UserID = userId;
				user.GroupID = groupId;

				result = _DbUsers.UpdateGroupID(user).ToString();
			}
			catch (Exception ex)
			{
				Response.StatusCode = 401;
				Response.Write(ex.Message);
			}

			return result;
		}
		#endregion string UpdateGroupID(string userId, int groupId)

		#region ContentResult UpdatePW(string userId, string p)
		/// <summary>
		/// 更新
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public ContentResult UpdatePW(string userId, string p)
		{
			var result = new Class_Response { code = 0, errMsg = "" };
			Users user = new Users();
			try
			{
				string checkData = "cd";
				if (Request.Headers.AllKeys.Contains(checkData))
				{
					checkData = Request.Headers[checkData];

					dynamic checkDataObj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ArmyAPI.Commons.Aes.Decrypt(checkData, ConfigurationManager.AppSettings.Get("ArmyKey")));

					string format = "yyyy-MM-dd HH:mm:ss";
					if (DateTime.TryParseExact((string)checkDataObj.t, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
					{
						int seconds = (int)((TimeSpan)(DateTime.Now - dateTime)).TotalSeconds;
						if (seconds < int.Parse(ConfigurationManager.AppSettings.Get("UpdatePwWaitSeconds")))
						{
							if (userId == (string)checkDataObj.u)
							{
								user.UserID = userId;
								string md5pw = Md5.Encode(p);
								user.Password = md5pw;

								result.code = _DbUsers.UpdatePW(user);
							}
						else
							throw new ArgumentException("帳號錯誤");
						}
						else
							throw new ArgumentException($"超過等待時間, {seconds}");
					}
					else
						throw new ArgumentException($"無法解析日期時間, {checkDataObj.t}");
				}
				else
					throw new ArgumentNullException($"認證失敗, {checkData}");
			}
			catch (Exception ex)
			{
				result.code = 401;
				result.errMsg = ex.Message;	
			}

			return this.Content(Newtonsoft.Json.JsonConvert.SerializeObject(result), "application/json");
		}
		#endregion ContentResult UpdateStatus(string userId, string p)

		#region ContentResult GetRanks()
		[ControllerAuthorizationFilter]
		[HttpPost]
		public ContentResult GetRanks()
		{
			string cacheKey = "Ranks";
            List<Rank> ranks = Globals._Cache.Get(cacheKey) as List<Rank>;

			if (ranks == null)
			{
				ranks = _DbArmy.GetRanks();
				Globals._Cache.Add(cacheKey, ranks, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddHours(24) });
			}
			return this.Content(JsonConvert.SerializeObject(ranks), "application/json");
		}
		#endregion ContentResult GetRanks()

		#region ContentResult GetSkills()
		[ControllerAuthorizationFilter]
		[HttpPost]
		public ContentResult GetSkills()
		{
			string cacheKey = "Skills";
            List<Skill> skills = Globals._Cache.Get(cacheKey) as List<Skill>;

			if (skills == null)
			{
				skills = _DbArmy.GetSkills();
                Globals._Cache.Add(cacheKey, skills, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddHours(24) });
            }
            return this.Content(JsonConvert.SerializeObject(skills), "application/json");
		}
		#endregion ContentResult GetSkills()

		#region ContentResult GetSkillsByDesc(string desc)
		[ControllerAuthorizationFilter]
		[HttpPost]
		public ContentResult GetSkillsByDesc(string desc)
		{
			List<Skill> skills = _DbArmy.GetSkills(desc);
            return this.Content(JsonConvert.SerializeObject(skills), "application/json");
		}
		#endregion ContentResult GetSkillsByDesc(string desc)

		#region ContentResult GetTitles(string title)
		[ControllerAuthorizationFilter]
		[HttpPost]
		public ContentResult GetTitles(string title)
		{
			List<Title> titles = _DbArmy.GetTitles(title);

			return this.Content(JsonConvert.SerializeObject(titles), "application/json");
		}
		#endregion ContentResult GetTitles(string title)

		#region ContentResult GetUnitsByTitle(string title)
		[ControllerAuthorizationFilter]
		[HttpPost]
		public ContentResult GetUnitsByTitle(string title)
		{
			List<Army_Unit> units = _DbArmy.GetUnitsByTitle(title);

			JsonSerializerSettings settings = new JsonSerializerSettings { ContractResolver = new CustomContractResolver("sort", "level", "parent_unit_code", "children", "index", "L_index", "L_title", "R_index", "R_title", "G_index", "G_title", "B_index", "B_title", "C_index", "C_title") };

			return this.Content(JsonConvert.SerializeObject(units, settings), "application/json");
		}
		#endregion ContentResult GetUnitsByTitle(string title)

		#region ContentResult GetDetail(string userId)
		[ControllerAuthorizationFilter]
		[HttpPost]
		public ContentResult GetDetail(string userId)
		{
			string loginId = HttpContext.Items["LoginId"] as string;
			bool isAdmin = (HttpContext.Items["IsAdmin"] as bool?) ?? false;

			if (string.IsNullOrEmpty(userId))
				userId = loginId;

			if (isAdmin || loginId == userId)
			{
				UserDetail ud = GetDetailByUserId(userId);

				JsonSerializerSettings settings = !isAdmin ? new JsonSerializerSettings { ContractResolver = new CustomContractResolver("Process", "Outcome") } : null;

				return this.Content(JsonConvert.SerializeObject(ud, settings), "application/json");
			}
			else
				return this.Content("");
		}
        #endregion ContentResult GetDetail(string userId)

        #region UserDetail GetDetailByUserId(string userId)
        public UserDetail GetDetailByUserId(string userId)
        {
            UserDetail user = _DbUsers.GetDetail(userId, true);
			if (user != null)
			{
				var categorys = _DbLimits.GetCategorys();

				user.Limits1 = _DbMenuUser.GetByUserId(userId);

				user.Limits2 = new List<UserDetailLimits>();

				foreach (var c in categorys)
				{
					var limits = _DbLimits.GetLimitByCategorys(c, userId);
					UserDetailLimits udLimit = new UserDetailLimits(c,
																	limits.Select(l => l.Split('|')[0].Substring(0, 6)).ToList(),
																	limits.Select(l => l.Split('|')[1]).ToList(),
																	limits.Select(l => l.Split('|')[2]).ToList());
					
					user.Limits2.Add(udLimit);
				}

				// 業管
				user.Units = _Db_s_User_Units.GetByUserId(userId);
			}
			else
				user = new UserDetail();

            return user;
        }
        #endregion UserDetail GetDetailByUserId(string userId)

        #region ContentResult GetInProgressList()
        [ControllerAuthorizationFilter]
		[HttpPost]
		public ContentResult GetInProgressList()
		{
			string loginId = HttpContext.Items["LoginId"] as string;
			bool isAdmin = (HttpContext.Items["IsAdmin"] as bool?) ?? false;

			string result = "";
			if (isAdmin)
			{
				var usersList = _DbUsers.GetByStatus(Users.Statuses.InReview);
				result = JsonConvert.SerializeObject(usersList);
			}

			return this.Content(result, "application/json");
		}
		#endregion ContentResult GetInProgressList()

		// Update IP1

		#region ContentResult GetAllApplyNewIP1()
		[ControllerAuthorizationFilter]
		[HttpPost]
		public ContentResult GetAllApplyNewIP1()
		{
			List<s_User_ChangeIP1> users = _Db_s_User_ChangeIP1.GetAll();

			return this.Content(JsonConvert.SerializeObject(users), "application/json");
		}
		#endregion ContentResult GetAllApplyNewIP1()


		#region string UpdateIP1(string userId, string ip1)
		/// <summary>
		/// 更新
		/// </summary>
		/// <returns></returns>
		[ControllerAuthorizationFilter]
		[HttpPost]
		public string UpdateIP1(string userId, string ip1)
		{
			string result = "";
			Users user = new Users();
			try
			{
				user.UserID = userId;
				user.IPAddr1 = ip1;

				result = _DbUsers.UpdateIP1(user).ToString();
			}
			catch (Exception ex)
			{
				Response.StatusCode = 401;
				Response.Write(ex.Message);
			}

			return result;
		}
		#endregion string UpdateIP1(string userId, string ip1)

		#region void Export()
		/// <summary>
		/// 匯出
		/// </summary>
		/// <returns></returns>
		[ControllerAuthorizationFilter]
		[HttpPost]
		public void Export()
		{
			string loginId = HttpContext.Items["LoginId"] as string;
			UserDetail user = Globals._Cache.Get($"User_{loginId}") as UserDetail;

			string filePath = HttpContext.Server.MapPath("../file/權限申請表.docx");

			using (var ms = new MemoryStream())
			{
				byte[] templateFile = System.IO.File.ReadAllBytes(filePath);
				ms.Write(templateFile, 0, templateFile.Length);
				using (WordprocessingDocument docx = WordprocessingDocument.Open(ms, true))
				{
					string applyDate = "";
					if (user.ApplyDate != null)
					{
						DateTime dt = (DateTime)user.ApplyDate;

						applyDate = $"{dt.Year - 1911}年{dt.Month}月{dt.Day}日";
					}

					string phone = user.PhoneMil;
					if (!string.IsNullOrEmpty(user.Phone))
					{
						if (!string.IsNullOrEmpty(phone))
							phone = phone + " / ";

						phone = phone + user.Phone;
					}

					string checkedBox = "■";
					string uncheckedBox = "□";

					string units = "";
					string unitDefines = "業管,陸軍,後備,資電軍,陸階,全軍";
					int index = 1;
					foreach (string d in unitDefines.Split(','))
					{
						units += $"({index})";
						if (user.Limits2.FindAll(l2 => l2.Key == "單位").Find(l2t => l2t.Texts.Contains(d)) != null)
							units += checkedBox;
						else
							units += uncheckedBox;
						units += (d + " ");

						index++;
					}

					string ranks = "";
					string rankDefines = "軍官(含將官),軍官(不含將官),士官,士兵,聘僱";
					index = 1;
					foreach (string d in rankDefines.Split(','))
					{
						ranks += $"({index})";
						if (user.Limits2.FindAll(l2 => l2.Key == "階級").Find(l2t => l2t.Texts.Contains(d)) != null)
							ranks += checkedBox;
						else
							ranks += uncheckedBox;
						ranks += (d + " ");

						index++;
					}

					WordRender.GenerateDocx(
						new Dictionary<string, string>()
						{
							["Unit"] = $"{user.UnitCode??""}-{user.Unit??""}",
						    ["RankTitle_MemberName"] = $"{user.RankTitle??""}{user.Name??""}",
							["TitleSkill"] = $"{user.TitleName ?? ""}({user.TitleCode ?? ""}) / {user.SkillDesc ?? ""}({user.SkillCode ?? ""})",
							["MemberId"] = user.UserID,
							["Email"] = user.Email != null ? user.Email.Split('@')[0] : "",
							["Phone"] = $"{phone}",
							["ApplyDate"] = applyDate,
							["Checkbox_Unit"] = units,
							["Checkbox_Rank"] = ranks,
							["Checkbox_Other官科"] = user.Limits2.FindAll(l2 => l2.Key == "其他").Find(l2t => l2t.Texts.Contains("官科")) != null ? checkedBox : uncheckedBox,
							["ApplyReason"] = user.Reason ?? "",
							["UseConsent"] = System.IO.File.ReadAllText(Server.MapPath("../file/陸軍司令部人事系統帳號使用同意書.txt"))
						}, docx);

					var docxBytes = ms.ToArray();
					// 將修改後的文件發送給客戶端
					Response.Clear();
					Response.Buffer = true;
					Response.ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
					Response.AddHeader("Content-Disposition", $"attachment; filename={user.UserID}-權限申請表.docx");
					Response.BinaryWrite(docxBytes);
					Response.Flush();
				}
			}
		}
		#endregion void Export()

		#region ContentResult CheckMissPhoto()
		public ContentResult CheckMissPhoto()
		{
			int result = 0;

			List<string> files = System.IO.Directory.GetFiles(Server.MapPath("../../Photo"))
							.Where(x => System.Text.RegularExpressions.Regex.IsMatch(System.IO.Path.GetFileNameWithoutExtension(x), @"[a-zA-Z]\d{9}"))
							.Select(x => System.IO.Path.GetFileNameWithoutExtension(x))
							.ToList();
			//List<string> files = JsonConvert.DeserializeObject<List<string>>(System.IO.File.ReadAllText(Server.MapPath("../file/Photos.txt")));
			List<string> memberids = _DbArmy.GetAllMemberId();
            List<string> missPhotoMemberids = memberids.Except(files).ToList();

			result = missPhotoMemberids.Count;
            _DbUsers.CheckMissPhoto(missPhotoMemberids);

			return this.Content(result.ToString(), "application/json");
		}
		#endregion ContentResult CheckMissPhoto()
	}

	public static class WordRender
	{
		static void ReplaceParserTag(this OpenXmlElement elem, Dictionary<string, string> data)
		{
			var pool = new List<Run>();
			var matchText = string.Empty;
			var hiliteRuns = elem.Descendants<Run>();
			List<Run> runs = new List<Run>();

			foreach (Run r in hiliteRuns)
			{
				if (r.RunProperties != null && r.RunProperties.Elements<Highlight>().Any())
				{
					runs.Add(r);
				}
			}

			foreach (var run in hiliteRuns)
			{
				var t = run.InnerText;
				if (t.StartsWith("["))
				{
					pool = new List<Run> { run };
					matchText = t;
				}
				else
				{
					matchText += t;
					pool.Add(run);
				}
				if (t.EndsWith("]"))
				{
					var m = Regex.Match(matchText, @"\[\$(?<n>\w+)\$\]");
					if (m.Success && data.ContainsKey(m.Groups["n"].Value))
					{
						var firstRun = pool.First();
						firstRun.RemoveAllChildren<Text>();
						firstRun.RunProperties.RemoveAllChildren<Highlight>();
						var newText = data[m.Groups["n"].Value];
						var firstLine = true;
						foreach (var line in Regex.Split(newText, @"\\n"))
						{
							if (firstLine) firstLine = false;
							else firstRun.Append(new Break());
							firstRun.Append(new Text(line));
						}
						pool.Skip(1).ToList().ForEach(o => o.Remove());
					}
				}
			}
		}

		public static byte[] GenerateDocx(byte[] template, Dictionary<string, string> data)
		{
			using (var ms = new MemoryStream())
			{
				ms.Write(template, 0, template.Length);
				using (var docx = WordprocessingDocument.Open(ms, true))
				{
					docx.MainDocumentPart.HeaderParts.ToList().ForEach(hdr =>
					{
						hdr.Header.ReplaceParserTag(data);
					});
					docx.MainDocumentPart.FooterParts.ToList().ForEach(ftr =>
					{
						ftr.Footer.ReplaceParserTag(data);
					});
					docx.MainDocumentPart.Document.Body.ReplaceParserTag(data);
					docx.Save();
				}
				return ms.ToArray();
			}
		}

		public static void GenerateDocx(Dictionary<string, string> data, WordprocessingDocument docx)
		{
			docx.MainDocumentPart.HeaderParts.ToList().ForEach(hdr =>
			{
				hdr.Header.ReplaceParserTag(data);
			});
			docx.MainDocumentPart.FooterParts.ToList().ForEach(ftr =>
			{
				ftr.Footer.ReplaceParserTag(data);
			});
			docx.MainDocumentPart.Document.Body.ReplaceParserTag(data);
			docx.Save();
		}
	}
}