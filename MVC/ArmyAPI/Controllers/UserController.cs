using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
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
		[CustomAuthorizationFilter]
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

		#region string Register(string userId, string p)
		[HttpPost]
		public string Register(string userId, string p)
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
			}
			catch (Exception ex)
			{
				Response.StatusCode = 401;
				Response.Write(ex.Message);
			}

			return result;
		}
		#endregion string Register(string userId, string p)

		#region string Register(string userId, string p, string name, string rank, string title, string skill, string ip1, string ip2, string email, string phoneMil, string phone)
		[CustomAuthorizationFilter]
		[HttpPost]
		[ActionName("RegisterFull")]
		public string Register(string userId, string name, string rank, string title, string skill, string ip1, string ip2, string email, string phoneMil, string phone)
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
			}
			catch (Exception ex)
			{
				//Response.StatusCode = 401;
				Response.Write(ex.Message);
			}

			return result;
		}
		#endregion string Register(string userId, string name, string rank, string title, string skill, string ip1, string ip2, string email, string phoneMil, string phone)

		#region int Delete(string userId)
		/// <summary>
		/// 刪除
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public int Delete(string userId)
		{
			string loginId = HttpContext.Items["LoginId"] as string;
			int result = 0;
			if (userId == loginId)
				Response.StatusCode = 401;
			else
				result = _DbUsers.Delete(userId, loginId);

			return result;
		}
		#endregion int Delete(string userId)

		#region int Deletes(string userIds)
		/// <summary>
		/// 刪除
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		[CustomAuthorizationFilter]
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

		#region //string CheckUser(string userId, string p)
		//[CustomAuthorizationFilter]
		//[HttpPost]
		//public string CheckUser(string userId, string p)
		//{
		//	string md5pw = Md5.Encode(p);
		//	string result = _DbUsers.Check(userId, md5pw);

		//	return result;
		//}
		#endregion //string CheckUser(string userId, string p)

		#region ContentResult CheckUserData(string userId, string name, string birthday, string email, string phone)
		[HttpPost]
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
		[CustomAuthorizationFilter]
		[HttpPost]
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
		[CustomAuthorizationFilter]
		[HttpPost]
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
		[CustomAuthorizationFilter]
		[HttpPost]
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
		[CustomAuthorizationFilter]
		[HttpPost]
		public string UpdateDetail_Limits(string userId, string name, string rank, string title, string skill, string ip1, string ip2, string email, string phoneMil, string phone, string limits1, string limits2, string tgroups, byte? process, string reason, string review, byte? outcome)
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

			dynamic menusUser = new { MenuUser = limits1, UserID = userId };

			dynamic limitCodes = new { LimitCodes = limits2, UserID = userId };
			// 要記申請日期
			DB_UpdateDetail_Limits db = new DB_UpdateDetail_Limits();
			string result = db.Run(user, menusUser, limitCodes, isAdmin).ToString();

			return result;
		}
		#endregion string UpdateDetail_Limits(string userId, string name, string rank, string title, string skill, string ip1, string ip2, string email, string phoneMil, string phone, string limits1, string limits2, string tgroups, byte? process, string reason, string review, byte? outcome)

		#region string UpdateStatus(string userId, short status)
		/// <summary>
		/// 更新
		/// </summary>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
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
		[CustomAuthorizationFilter]
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
			catch (Exception ex)
			{
				//Response.StatusCode = 401;
				Response.Write(ex.Message);
			}

			return result;
		}
		#endregion string UpdateStatus(string userIds, short status)

		#region string UpdateGroupID(string userId, int groupId)
		/// <summary>
		/// 更新
		/// </summary>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
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
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult GetRanks()
		{
			List<Rank> users = _DbArmy.GetRanks();

			return this.Content(JsonConvert.SerializeObject(users), "application/json");
		}
		#endregion ContentResult GetRanks()

		#region ContentResult GetSkills()
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult GetSkills()
		{
			List<Skill> skills = _DbArmy.GetSkills();

			return this.Content(JsonConvert.SerializeObject(skills), "application/json");
		}
		#endregion ContentResult GetSkills()

		#region ContentResult GetTitles(string title)
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult GetTitles(string title)
		{
			List<Title> titles = _DbArmy.GetTitles(title);

			return this.Content(JsonConvert.SerializeObject(titles), "application/json");
		}
		#endregion ContentResult GetTitles(string title)

		#region ContentResult GetDetail(string userId)
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult GetDetail(string userId)
		{
			string loginId = HttpContext.Items["LoginId"] as string;
			bool isAdmin = (HttpContext.Items["IsAdmin"] as bool?) ?? false;

			if (string.IsNullOrEmpty(userId))
				userId = loginId;

			if (isAdmin || loginId == userId)
			{
				UserDetail ud = _DbUsers.GetDetail(userId, isAdmin);
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

				JsonSerializerSettings settings = !isAdmin ? new JsonSerializerSettings { ContractResolver = new CustomContractResolver("Process", "Review", "Outcome") } : null;

				return this.Content(JsonConvert.SerializeObject(ud, settings), "application/json");
			}
			else
				return this.Content("");
		}
		#endregion ContentResult GetDetail(string userId)

		#region ContentResult GetInProgressList()
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult GetInProgressList()
		{
			string loginId = HttpContext.Items["LoginId"] as string;
			bool isAdmin = (HttpContext.Items["IsAdmin"] as bool?) ?? false;

			string result = "";
			if (isAdmin)
			{
				var usersList = _DbUsers.GetByStatus(Users.Statuses.InProgress);
				result = JsonConvert.SerializeObject(usersList);
			}

			return this.Content(result, "application/json");
		}
		#endregion ContentResult GetInProgressList()
	}
}