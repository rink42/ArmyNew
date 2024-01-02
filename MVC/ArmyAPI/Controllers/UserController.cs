﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Web.Mvc;
using ArmyAPI.Commons;
using ArmyAPI.Data;
using ArmyAPI.Filters;
using ArmyAPI.Models;
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

			JsonSerializerSettings settings = !isAdmin ? new JsonSerializerSettings { ContractResolver = new CustomContractResolver("Process", "Reason", "Review", "Outcome") } : null;

			return this.Content(JsonConvert.SerializeObject(uds, settings), "application/json");
		}
		#endregion ContentResult GetAll()

		#region ActionResult Register(string userId, string p)
		[HttpPost]
		[CheckUserIDFilter("userId")]

		public ActionResult Register(string userId, string p)
		{
			string result = "";
			Users user = new Users();
			try
			{
				bool isAD = false;
				if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("CheckAD")) && ConfigurationManager.AppSettings.Get("CheckAD") == "1")
				{
					isAD = Globals.CheckUserExistence(userId);
				}
				user.UserID = userId;
				if (!isAD)
				{
					string md5pw = Md5.Encode(p);
					user.Password = md5pw;
				}
				ArmyUser armyUser = _DbArmy.GetUser(userId);

				if (armyUser != null)
					user.Name = armyUser.MemberName;
				else
					user.Name = "";

				result = _DbUsers.Add(user, isAD).ToString();

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
		#endregion ActionResult Register(string userId, string p)

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

		#region string UpdateDetail_Limits(string userId, string name, string rank, string title, string skill, string ip1, string ip2, string email, string phoneMil, string phone, string limits1, string limits2, string tgroups, byte? process, string reason, string review, byte? outcome)
		/// <summary>
		/// 更新(含權限)
		/// </summary>
		/// <returns></returns>
		[ControllerAuthorizationFilter]
		[HttpPost]
		[CheckUserIDFilter("userId")]

		public string UpdateDetail_Limits(string userId, string name, string rank, string title, string skill, string ip1, string ip2, string email, string phoneMil, string phone, string limits1, string limits2, string tgroups, byte? process, string reason, string review, byte? outcome)
		{
			return UpdateDetail_Limits2(userId, name, rank, title, skill, ip1, ip2, email, phoneMil, phone, limits1, limits2, tgroups, process, reason, review, outcome, "");
		}
		#endregion string UpdateDetail_Limits(string userId, string name, string rank, string title, string skill, string ip1, string ip2, string email, string phoneMil, string phone, string limits1, string limits2, string tgroups, byte? process, string reason, string review, byte? outcome)

		#region string UpdateDetail_Limits2(string userId, string name, string rank, string title, string skill, string ip1, string ip2, string email, string phoneMil, string phone, string limits1, string limits2, string tgroups, byte? process, string reason, string review, byte? outcome, string units)
		/// <summary>
		/// 更新(含權限)
		/// </summary>
		/// <returns></returns>
		[ControllerAuthorizationFilter]
		[HttpPost]
		[CheckUserIDFilter("userId")]

		public string UpdateDetail_Limits2(string userId, string name, string rank, string title, string skill, string ip1, string ip2, string email, string phoneMil, string phone, string limits1, string limits2, string tgroups, byte? process, string reason, string review, byte? outcome, string units)
		{
			string loginId = HttpContext.Items["LoginId"] as string;
			bool isAdmin = (HttpContext.Items["IsAdmin"] as bool?) ?? false;

			UserDetail user = new UserDetail();
			user.UserID = userId;
			user.Name = name;
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

			string result = $"{{'r1': {result1}, 'r2': {result2} }}";

			return result;
		}
		#endregion string UpdateDetail_Limits2(string userId, string name, string rank, string title, string skill, string ip1, string ip2, string email, string phoneMil, string phone, string limits1, string limits2, string tgroups, byte? process, string reason, string review, byte? outcome, string units)

		#region string UpdateStatus(string userId, short status)
		/// <summary>
		/// 更新
		/// </summary>
		/// <returns></returns>
		[ControllerAuthorizationFilter]
		[HttpPost]
		[CheckUserIDFilter("userId")]

		public string UpdateStatus(string userId, short status)
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
		#endregion string UpdateStatus(string userId, short status)

		#region string UpdateStatuses(string userIds, short status)
		/// <summary>
		/// 更新
		/// </summary>
		/// <returns></returns>
		[ControllerAuthorizationFilter]
		[HttpPost]
		public string UpdateStatuses(string userIds, short status)
		{
			string result = "";
			Users user = new Users();
			try
			{
				var values = Enum.GetValues(typeof(Users.Statuses)).Cast<Users.Statuses>();

				if ((short)values.Min() <= status && status <= (short)values.Max())
				{
					Users.Statuses euStatus = (Users.Statuses)status;
					result = _DbUsers.UpdateStatuses(userIds, euStatus).ToString();
				}
				else
					throw new Exception("Status 的值不存在");
			}
			catch //(Exception ex)
			{
				//Response.StatusCode = 401;
				//Response.Write(ex.Message);
			}

			return result;
		}
		#endregion string UpdateStatus(string userIds, short status)

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

		#region ContentResult GetTitles(string title)
		[ControllerAuthorizationFilter]
		[HttpPost]
		public ContentResult GetTitles(string title)
		{
			List<Title> titles = _DbArmy.GetTitles(title);

			return this.Content(JsonConvert.SerializeObject(titles), "application/json");
		}
		#endregion ContentResult GetTitles(string title)

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

				JsonSerializerSettings settings = !isAdmin ? new JsonSerializerSettings { ContractResolver = new CustomContractResolver("Process", "Review", "Outcome") } : null;

				return this.Content(JsonConvert.SerializeObject(ud, settings), "application/json");
			}
			else
				return this.Content("");
		}
        #endregion ContentResult GetDetail(string userId)

        #region UserDetail GetDetailByUserId(string userId)
        public UserDetail GetDetailByUserId(string userId)
        {
            UserDetail ud = _DbUsers.GetDetail(userId, true);
            var categorys = _DbLimits.GetCategorys();

            ud.Limits1 = _DbMenuUser.GetByUserId(userId);

            ud.Limits2 = new List<UserDetailLimits>();

            foreach (var c in categorys)
            {
                UserDetailLimits udLimit = new UserDetailLimits();
                udLimit.Key = c;
                var limits = _DbLimits.GetLimitByCategorys(c, userId);
                udLimit.Values = new List<string>();
                foreach (var l in limits)
                {
                    udLimit.Values.Add(l.Substring(0, 6));
                }
                ud.Limits2.Add(udLimit);
            }

            // 業管
            ud.Units = _Db_s_User_Units.GetByUserId(userId);

            return ud;
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
	}
}