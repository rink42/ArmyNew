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
    public class LimitsController : Controller
	{
		private static string _ConnectionString = ConfigurationManager.ConnectionStrings["ArmyWebConnectionString"].ConnectionString;
		private MsSqlDataProvider.DB_Limits _DbLimits = new MsSqlDataProvider.DB_Limits(_ConnectionString);
		// GET: Limits
		public ActionResult Index()
        {
            return View();
		}

		#region string GetAll()
		[CustomAuthorizationFilter]
		[HttpPost]
		public string GetAll()
		{
			List<Limits> userGroup = _DbLimits.GetAll();

			return JsonConvert.SerializeObject(userGroup);
		}
		#endregion string GetAll()

		#region int Add(string title, int sort, bool isEnable)
		/// <summary>
		/// 新增
		/// </summary>
		/// <param name="title"></param>
		/// <param name="sort"></param>
		/// <param name="isEnable"></param>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public int Add(string title, int sort, bool isEnable)
		{
			int result = _DbLimits.Add(title, sort, isEnable, TempData["LoginAcc"].ToString());

			return result;
		}
		#endregion int Add(string title, int sort, bool isEnable)

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
			int result = _DbLimits.Delete(index, TempData["LoginAcc"].ToString());

			return result;
		}
		#endregion int Delete(int index)
	}
}