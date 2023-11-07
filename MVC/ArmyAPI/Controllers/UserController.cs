using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
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
			List<Users> users = _DbUsers.GetAll();

			return this.Content(JsonConvert.SerializeObject(users), "application/json");
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

		#region string Register(string userId, string p, string name, string rank, string specialty, string ip1, string ip2, string email, string phoneMil, string phone)
		[CustomAuthorizationFilter]
		[HttpPost]
		[ActionName("RegisterFull")]
		public string Register(string userId, string name, string rank, string specialty, string ip1, string ip2, string email, string phoneMil, string phone)
		{
			string result = "";
			Users user = new Users();
			try
			{
				user.UserID = userId;
				user.Name = name;
				user.Rank = rank;
				user.Specialty = specialty;
				user.IPAddr1 = ip1;
				user.IPAddr2 = ip2;
				user.Email = email;
				user.PhoneMil = phoneMil;
				user.Phone = phone;

				result = _DbUsers.Add(user).ToString();
			}
			catch (Exception ex)
			{
				Response.StatusCode = 401;
				Response.Write(ex.Message);
			}

			return result;
		}
		#endregion string Register(string userId, string name, string rank, string specialty, string ip1, string ip2, string email, string phoneMil, string phone)

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
			string adminId = TempData["LoginAcc"].ToString();
			int result = 0;
			if (userId == adminId)
				Response.StatusCode = 401;
			else
				result = _DbUsers.Delete(userId, TempData["LoginAcc"].ToString());

			return result;
		}
		#endregion int Delete(string userId)

		#region string CheckUser(string userId, string wp)
		[CustomAuthorizationFilter]
		[HttpPost]
		public string CheckUser(string userId, string wp)
		{
			string md5pw = Md5.Encode(wp);
			string result = _DbUsers.Check(userId, md5pw);

			return result;
		}
		#endregion string CheckUser(string userId, string wp)

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

		#region string UpdateStatus(string userId, int status)
		/// <summary>
		/// 更新
		/// </summary>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public string UpdateStatus(string userId, int status)
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
		#endregion string UpdateStatus(string userId, int status)

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
	}
}