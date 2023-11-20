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
using Microsoft.SqlServer.Server;
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

		#region ContentResult GetAll()
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult GetAll()
		{
			List<Limits> userGroup = _DbLimits.GetAll();

			return this.Content(JsonConvert.SerializeObject(userGroup), "application/json");
		}
		#endregion ContentResult GetAll()

		#region ContentResult GetAll_Min()
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult GetAll_Min()
		{
			List<Limits> userGroup = _DbLimits.GetAll();

			var groupedData = userGroup.GroupBy(l => l.Category)
			.Select(g => new
			{
				Category = g.Key,
				Items = g.Select(item => new
				{
					LimitCode = item.LimitCode.Substring(0, 6),
					Title = item.Title
				})
			});

			string json = JsonConvert.SerializeObject(groupedData);

			return this.Content(json, "application/json");
		}
		#endregion ContentResult GetAll_Min()

		#region string Add(string category, string title, int sort, bool isEnable)
		/// <summary>
		/// 新增
		/// </summary>
		/// <param name="category"></param>
		/// <param name="title"></param>
		/// <param name="sort"></param>
		/// <param name="isEnable"></param>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public int Add(string category, string title, int sort, bool isEnable)
		{
			int result = _DbLimits.Add(category, title, sort, isEnable, TempData["LoginAcc"].ToString());

			return result;
		}
		#endregion int Add(string category, string title, int sort, bool isEnable)

		#region string Adds(string category, string titles, int sort, bool isEnable)
		/// <summary>
		/// 新增
		/// </summary>
		/// <param name="category"></param>
		/// <param name="titles"></param>
		/// <param name="sort"></param>
		/// <param name="isEnable"></param>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public int Adds(string category, string titles, int sort, bool isEnable)
		{
			int result = 0;

			foreach (string t in titles.Split(','))
			{
				result +=  _DbLimits.Add(category, t, sort, isEnable, TempData["LoginAcc"].ToString());
			}
			return result;
		}
		#endregion int Adds(string category, string title, int sort, bool isEnable)

		#region string Update(string code, short category, string title, int sort, bool isEnable)
		/// <summary>
		/// 新增
		/// </summary>
		/// <param name="code"></param>
		/// <param name="category"></param>
		/// <param name="title"></param>
		/// <param name="sort"></param>
		/// <param name="isEnable"></param>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public int Update(string code, short category, string title, int sort, bool isEnable)
		{
			int result = _DbLimits.Update(code, category, title, sort, isEnable, TempData["LoginAcc"].ToString());

			return result;
		}
		#endregion int Update(string code, short category, string title, int sort, bool isEnable)

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

		#region ContentResult GetArmyUnitOriginal()
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult GetArmyUnitOriginal()
		{
			Army_Unit units = _DbArmy.GetOriginal();

			return this.Content(JsonConvert.SerializeObject(units), "application/json");
		}
		#endregion ContentResult GetArmyUnitOriginal()

		#region ContentResult GetNewArmyUnit()
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult GetNewArmyUnit()
		{
			ArmyUnits units = _DbArmyUnits.GetAll();

			return this.Content(JsonConvert.SerializeObject(units), "application/json");
		}
		#endregion ContentResult GetNewArmyUnit()

		#region ContentResult SetArmyUnit()
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult SetArmyUnit(string all)
		{
			Army_Unit units = JsonConvert.DeserializeObject<Army_Unit>(all);

			Army_Unit.ResetLevel(ref units);

			ArmyUnits newUnits = new ArmyUnits();
			units.CopyTo(newUnits);

			int result = _DbArmyUnits.Add(newUnits); 

			return this.Content(result.ToString(), "application/json");
		}
		#endregion ContentResult SetArmyUnit()
	}
}