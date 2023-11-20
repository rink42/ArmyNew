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

				//result += $"{{\"{tableName.ToString().Replace("rmy", "")}\": {num}}}";
				//jsonObject.key = tableName.ToString().Replace("rmy", "");
				//jsonObject.value = num;
				var jsonObj = new { n = tableName.ToString().Replace("rmy", ""), v = num };

				result.Add(jsonObj);
			}

			return this.Content(JsonConvert.SerializeObject(result), "application/json");
		}
		#endregion ContentResult GetAll()
	}
}
