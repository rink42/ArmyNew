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
			string result = "";

			foreach (DB_Tableau.TableNames tableName in System.Enum.GetValues(typeof(DB_Tableau.TableNames)))
			{
				if (result.Length > 0)
					result += ", ";

				int num = _DbTableau.Gets(tableName, "");

				result += $"{{\"{tableName.ToString().Replace("rmy", "")}\": {num}}}";
			}

			return this.Content(result, "application/json");
		}
		#endregion ContentResult GetAll()
	}
}
