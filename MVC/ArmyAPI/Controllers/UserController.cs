using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using ArmyAPI.Commons;
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
			//string loginId = TempData["LoginAcc"].ToString();
			//bool isAdmin = _DbUserGroup.IsAdmin(loginId);

			//List<Users> users = _DbUsers.GetAll();

			//JsonSerializerSettings settings = !isAdmin ? new JsonSerializerSettings { ContractResolver = new CustomContractResolver("Process", "Reason", "Review", "Outcome") } : null;

			//return this.Content(JsonConvert.SerializeObject(users, settings), "application/json");
			string loginId = TempData["LoginAcc"].ToString();
			bool isAdmin = _DbUserGroup.IsAdmin(loginId);

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
				user.UserID = userId;
				string md5pw = Md5.Encode(p);
				user.Password = md5pw;
				user.Name = "";

				result = _DbUsers.Add(user).ToString();
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
			string loginId = TempData["LoginAcc"].ToString();
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
			string loginId = TempData["LoginAcc"].ToString();
			int result = 0;
			if (userIds.Split(',').Contains(loginId))
				Response.StatusCode = 401;
			else
				result = _DbUsers.Deletes(userIds, loginId);

			return result;
		}
		#endregion int Deletes(string userIds)

		#region string CheckUser(string userId, string p)
		[CustomAuthorizationFilter]
		[HttpPost]
		public string CheckUser(string userId, string p)
		{
			string md5pw = Md5.Encode(p);
			string result = _DbUsers.Check(userId, md5pw);

			return result;
		}
		#endregion string CheckUser(string userId, string p)

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

		#region string UpdateDetail_NoLimits(string userId, string name, string ip1, string ip2, string email, string phoneMil, string phone, byte? process, string reason, string review, byte? outcome)
		/// <summary>
		/// 更新
		/// </summary>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public string UpdateDetail_NoLimits(string userId, string name, string ip1, string ip2, string email, string phoneMil, string phone, byte? process, string reason, string review, byte? outcome)
		{

			string result = "";
			UserDetail user = new UserDetail();
			try
			{
				user.UserID = userId;
				user.Name = name;
				user.IPAddr1 = ip1;
				user.IPAddr2 = ip2;
				user.Email = email;
				user.PhoneMil = phoneMil;
				user.Phone = phone;
				user.Process = process;
				user.Reason = reason;
				user.Review = review;
				user.Outcome = outcome;

				result = _DbUsers.UpdateDetail(user).ToString();
			}
			catch (Exception ex)
			{
				Response.StatusCode = 401;
				Response.Write(ex.Message);
			}

			return result;
		}
		#endregion string UpdateDetail_NoLimits(string userId, string name, string ip1, string ip2, string email, string phoneMil, string phone, byte? process, string reason, string review, byte? outcome)

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

		#region string UpdateDetail_Limits(string userId, string name, string ip1, string ip2, string email, string phoneMil, string phone, string limits, byte? process, string reason, string review, byte? outcome)
		/// <summary>
		/// 更新(含權限)
		/// </summary>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public string UpdateDetail_Limits(string userId, string name, string ip1, string ip2, string email, string phoneMil, string phone, string limits, byte? process, string reason, string review, byte? outcome)
		{
			string result = UpdateDetail_NoLimits(userId, name, ip1, ip2, email, phoneMil, phone, process, reason, review, outcome);

			if (result == "1")
			{
				result += (1 + _DbLimitsUser.Update(userId, limits)).ToString();
			}

			return result;
		}
		#endregion string UpdateDetail_Limits(string userId, string name, string ip1, string ip2, string email, string phoneMil, string phone, string limits, byte? process, string reason, string review, byte? outcome)

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

		#region ContentResult GetDetail()
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult GetDetail()
		{
			string loginId = TempData["LoginAcc"].ToString();

			return GetDetail_Admin(loginId);
		}
		#endregion ContentResult GetDetail(string userId)

		#region ContentResult GetDetail_Admin(string userId)
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult GetDetail_Admin(string userId)
		{
			string loginId = TempData["LoginAcc"].ToString();
			bool isAdmin = _DbUserGroup.IsAdmin(loginId);

			if (isAdmin || loginId == userId)
			{
				UserDetail ud = _DbUsers.GetDetail(userId, isAdmin);
				var categorys = _DbLimits.GetCategorys();

				ud.Limits = new List<UserDetailLimits>();

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
					ud.Limits.Add(udLimit);
				}

				JsonSerializerSettings settings = !isAdmin ? new JsonSerializerSettings { ContractResolver = new CustomContractResolver("Process", "Reason", "Review", "Outcome") } : null;

				return this.Content(JsonConvert.SerializeObject(ud, settings), "application/json");
			}
			else
				return this.Content("");
		}
		#endregion ContentResult GetDetail_Admin(string userId)


		#region ContentResult GetInProgressList()
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult GetInProgressList()
		{
			string loginId = TempData["LoginAcc"].ToString();
			bool isAdmin = _DbUserGroup.IsAdmin(loginId);

			List<UserDetail> uds = _DbUsers.GetDetails(isAdmin);

			JsonSerializerSettings settings = !isAdmin ? new JsonSerializerSettings { ContractResolver = new CustomContractResolver("Process", "Reason", "Review", "Outcome") } : null;

			return this.Content(JsonConvert.SerializeObject(uds, settings), "application/json");
		}
		#endregion ContentResult GetInProgressList()
	}
}