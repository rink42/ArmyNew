using System;
using System.Collections.Generic;
using System.Linq;
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

		#region int Register(string userId, string md5pw, string name, string ip1, string ip2, string tpw, string email, string phoneMil, string phone)
		[CustomAuthorizationFilter]
		[HttpPost]
		public int Register(string userId, string md5pw, string name, string ip1, string ip2, string tpw, string email, string phoneMil, string phone)
		{
			Users user = new Users();
			user.UserID = userId;
			user.Password = md5pw;
			user.Name = name;
			user.IPAddr1 = ip1;
			user.IPAddr2 = ip2;
			user.TransPassword = tpw;
			user.Email = email;
			user.PhoneMil = phoneMil;
			user.Phone = phone;

			int result = _DbUsers.Add(user);

			return result;
		}
		#endregion int Register(string userId, string md5pw, string name, string ip1, string ip2, string tpw, string email, string phoneMil, string phone)

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

		#region string CheckUser(string userId, string md5pw)
		[CustomAuthorizationFilter]
		[HttpPost]
		public string CheckUser(string userId, string md5pw)
		{
			string result = _DbUsers.Check(userId, md5pw);

			return result;
		}
		#endregion string CheckUser(string userId, string md5pw)
	}
}