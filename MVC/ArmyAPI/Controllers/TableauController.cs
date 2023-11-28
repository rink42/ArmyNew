using System.Collections.Generic;
using System.Web.Mvc;
using ArmyAPI.Commons;
using ArmyAPI.Filters;
using Newtonsoft.Json;
using static ArmyAPI.Data.MsSqlDataProvider;

namespace ArmyAPI.Controllers
{
	public class TableauController : BaseController
	{
		#region ContentResult GetAll()
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult GetAll()
		{
			//string result = "";
			//dynamic jsonObject = new System.Dynamic.ExpandoObject();
			List<object> result = new List<object>();

			foreach (DB_Tableau.TableNames tableName in System.Enum.GetValues(typeof(DB_Tableau.TableNames)))
			{
				//if (result.Length > 0)
				//	result += ", ";

				int num = _DbTableau.Gets(tableName, "");
				string[] descs = Globals.GetEnumDesc(tableName).Split(',');

				var jsonObj = new { c = descs[0],  n = descs[1], v = num };

				result.Add(jsonObj);
			}

			return this.Content(JsonConvert.SerializeObject(result), "application/json");
		}
		#endregion ContentResult GetAll()

		#region string Record(string url)
		/// <summary>
		/// 新增
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public int Record(string url)
		{
			int result = _DbLimits.Add(category, title, sort, isEnable, TempData["LoginAcc"].ToString());

			return result;
		}
		#endregion int Record(string url)
	}
}
