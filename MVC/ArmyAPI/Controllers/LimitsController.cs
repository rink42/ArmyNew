using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArmyAPI.Commons;
using ArmyAPI.Data;
using ArmyAPI.Filters;
using ArmyAPI.Models;
using Newtonsoft.Json;
using static ArmyAPI.Data.MsSqlDataProvider;

namespace ArmyAPI.Controllers
{
    public class LimitsController : BaseController
	{
		// GET: Limits
		public ActionResult Index()
        {
            return View();
		}

		#region string GetAll()
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult GetAll()
		{
			List<Limits> userGroup = _DbLimits.GetAll();

			return this.Content(JsonConvert.SerializeObject(userGroup), "application/json");
		}
		#endregion string GetAll()

		#region string Add(short category, string title, int sort, bool isEnable, string parentCode)
		/// <summary>
		/// 新增
		/// </summary>
		/// <param name="category"></param>
		/// <param name="title"></param>
		/// <param name="sort"></param>
		/// <param name="isEnable"></param>
		/// <param name="parentCode"></param>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public int Add(short category, string title, int sort, bool isEnable, string parentCode)
		{
			int result = _DbLimits.Add(category, title, sort, isEnable, parentCode, TempData["LoginAcc"].ToString());

			return result;
		}
		#endregion int Add(short category, string title, int sort, bool isEnable, string parentCode)

		#region string Update(string code, short category, string title, int sort, bool isEnable, string parentCode)
		/// <summary>
		/// 新增
		/// </summary>
		/// <param name="code"></param>
		/// <param name="category"></param>
		/// <param name="title"></param>
		/// <param name="sort"></param>
		/// <param name="isEnable"></param>
		/// <param name="parentCode"></param>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public int Update(string code, short category, string title, int sort, bool isEnable, string parentCode)
		{
			int result = _DbLimits.Update(code, category, title, sort, isEnable, parentCode, TempData["LoginAcc"].ToString());

			return result;
		}
		#endregion int Update(string code, short category, string title, int sort, bool isEnable, string parentCode)

		#region int Delete(string code)
		/// <summary>
		/// 刪除
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public int Delete(string code)
		{
			int result = _DbLimits.Delete(code, TempData["LoginAcc"].ToString());

			return result;
		}
		#endregion int Delete(string code)
	}
}