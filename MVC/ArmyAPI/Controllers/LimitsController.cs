using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArmyAPI.Commons;
using ArmyAPI.Data;
using ArmyAPI.Filters;
using ArmyAPI.Models;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using static ArmyAPI.Data.MsSqlDataProvider;
using Dapper;
using System.Drawing.Design;
using NPOI.Util;
using NPOI.HPSF;

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
		//[CustomAuthorizationFilter]
		//[HttpPost]
		public ContentResult GetArmyUnitOriginal()
		{
			Army_Unit units = _DbArmy.GetOriginal();

			return this.Content(JsonConvert.SerializeObject(units), "application/json");
		}
		#endregion ContentResult GetArmyUnitOriginal()

		#region ContentResult GetNewArmyUnit()
		//[CustomAuthorizationFilter]
		//[HttpPost]
		public ContentResult GetNewArmyUnit()
		{
			ArmyUnits units = _DbArmyUnits.GetAll();
			DataSet notSortedUnits = _DbArmy.GetOriginalNotSorted();

			List<ArmyUnits> root = new List<ArmyUnits>();

			ArmyUnits unSorted = new ArmyUnits();
			unSorted.children = new List<ArmyUnits>();
			if (notSortedUnits != null && notSortedUnits.Tables.Count > 0 && notSortedUnits.Tables[0].Rows.Count > 0)
			{
				foreach (DataRow dr in notSortedUnits.Tables[0].Rows)
				{
					ArmyUnits uns = new ArmyUnits();
					uns.unit_code = dr["unit_code"].ToString().Trim();
					uns.title = dr["unit_title"].ToString().Trim();
					uns.level = dr["ulevel_code"].ToString().Trim();
					if (dr["parent_unit_code"] != null)
						uns.parent_unit_code = dr["parent_unit_code"].ToString().Trim();

					unSorted.children.Add(uns);
				}
			}

			unSorted.title = "未分類";
			root.Add(unSorted);
			root.Add(units);

			return this.Content(JsonConvert.SerializeObject(root), "application/json");
		}
		#endregion ContentResult GetNewArmyUnit()

		#region ContentResult SetArmyUnit()
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult SetArmyUnit(string all)
		{
			List<Army_Unit> units = JsonConvert.DeserializeObject<List<Army_Unit>>(all);

			Army_Unit unit = units[0];

			// 寫入到 ArmyWeb.dbo.s_Unit
			_DbArmyUnits.DeleteAll__s_Unit();
			//WriteLog.Log(all);
			units[0].Resets(null);
			Army_Unit.ModifiedCodes.Length = 0;

			Write_v_Units1(units);

			using (IDbConnection conn = new SqlConnection(_ConnectionString))
			{
				string sqlCmd = @"
INSERT INTO ArmyWeb.dbo.s_Unit
			([UnitCode], [ParentUnitCode], [UnitTitle], [Status], [L_index], [L_title], [R_index], [R_title], [G_index], [G_title], [B_index], [B_title], [C_index], [C_title], [StartDate], [EndDate], [Sort])
	VALUES (@UnitCode, @ParentUnitCode, @UnitTitle, @Status, @L_index, @L_title, @R_index, @R_title, @G_index, @G_title, @B_index, @B_title, @C_index, @C_title, @StartDate, @EndDate, @Sort)
			";

				conn.Execute(sqlCmd, paras);
			}

			ArmyUnits newUnits = new ArmyUnits();
			unit.CopyTo(newUnits);

			int result = _DbArmyUnits.Add(newUnits); 

			return this.Content(result.ToString(), "application/json");
		}
		#endregion ContentResult SetArmyUnit()

		private List<s_Unit> paras = new List<s_Unit>();

		#region void Write_v_Units1(List<Army_Unit> nodes)
		private void Write_v_Units1(List<Army_Unit> nodes)
		{
			var units = nodes.Where(n => n.title != "").Select(n => n.code).ToList();

			foreach (Army_Unit node in nodes)
			{
				if (node.title != "")
				{
					DataTable dt = _DbArmy.GetUnitData(node.code);

					string status = "";
					string startDate = "";
					string endDate = "";

					if (dt != null && dt.Rows.Count > 0)
					{
						status = dt.Rows[0]["unit_status"].ToString();
						startDate = dt.Rows[0]["start_date"].ToString();
						endDate = dt.Rows[0]["end_date"].ToString();
					}

					s_Unit unit = new s_Unit();
					unit.UnitCode = node.code;
					unit.ParentUnitCode = node.parent_code ?? "";
					unit.UnitTitle = node.title;
					unit.Sort = node.sort;
					unit.L_index = node.L_index;
					unit.L_title = node.L_title;
					unit.R_index = node.R_index;
					unit.R_title = node.R_title;
					unit.G_index = node.G_index;
					unit.G_title = node.G_title;
					unit.B_index = node.B_index;
					unit.B_title = node.B_title;
					unit.C_index = node.C_index;
					unit.C_title = node.C_title;
					if (status != "")
						unit.Status = int.Parse(status);
					if (DateTime.TryParse(startDate, out DateTime tmpDTs))
						unit.StartDate = tmpDTs;
					if (DateTime.TryParse(endDate, out DateTime tmpDTe))
						unit.EndDate = tmpDTe;

					paras.Add(unit);

					// 再寫入子節點
					if (node.children != null && node.children.Count > 0)
						Write_v_Units1(node.children);
				}
			}
		}
		#endregion void Write_v_Units1(List<Army_Unit> nodes)
	}
}