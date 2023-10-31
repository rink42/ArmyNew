using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArmyAPI.Data;
using ArmyAPI.Filters;
using ArmyAPI.Models;
using Newtonsoft.Json;
using static ArmyAPI.Data.MsSqlDataProvider;

namespace ArmyAPI.Controllers
{
    public class UserGroupController : Controller
	{
		private static string _ConnectionString = ConfigurationManager.ConnectionStrings["Army2ConnectionString"].ConnectionString;
		private MsSqlDataProvider.DB_UserGroup _DbUserGroup = new MsSqlDataProvider.DB_UserGroup(_ConnectionString);
		// GET: UserGroup
		public ActionResult Index()
        {
            return View();
		}

		#region string GetAll()
		[CustomAuthorizationFilter]
		[HttpPost]
		public string GetAll()
		{
			List<UserGroup> userGroup = _DbUserGroup.GetAll();

			return JsonConvert.SerializeObject(userGroup);
		}
		#endregion string GetAll()

		#region int Add(string title, bool isEnable)
		/// <summary>
		/// 新增
		/// </summary>
		/// <param name="title"></param>
		/// <param name="isEnable"></param>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public int Add(string title, bool isEnable)
		{
			int result = _DbUserGroup.Add(title, isEnable, "Admin");

			return result;
		}
		#endregion int Add(string title, bool isEnable)

		#region int Delete(int index)
		/// <summary>
		/// 刪除
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public int Delete(int index)
		{
			int result = _DbUserGroup.Delete(index, "Admin");

			return result;
		}
		#endregion int Delete(int index)
	}
}