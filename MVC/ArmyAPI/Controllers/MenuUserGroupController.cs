using System.Collections.Generic;
using System.Web.Mvc;
using ArmyAPI.Commons;
using ArmyAPI.Filters;
using ArmyAPI.Models;
using Newtonsoft.Json;

namespace ArmyAPI.Controllers
{
	public class MenuUserGroupController : BaseController
    {
		public MenuUserGroupController()
		{
		}

		#region ContentResult GetAll(bool showDisable)
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult GetAll(bool showDisable)
		{
			List<Menus> menus = null;

			return this.Content(JsonConvert.SerializeObject(menus), "application/json");
		}
		#endregion ContentResult GetAll(bool showDisable)


		#region string Add(int menuIndex, int groupIndex)
		/// <summary>
		/// 新增
		/// </summary>
		/// <param name="menuIndex"></param>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public string Add(int menuIndex, int groupIndex)
		{
			string loginId = HttpContext.Items["LoginId"] as string;
			string result = "";

			if (Globals.IsAdmin(loginId))
			{
				result = _DbMenuUserGroup.Add(menuIndex, groupIndex, loginId).ToString();
			}
			else
			{
				result = "非管理者";
			}
			return result;
		}
		#endregion string Add(int menuIndex, int groupIndex)

		#region int Adds(string menuIndexs, int groupIndex)
		/// <summary>
		/// 新增
		/// </summary>
		/// <param name="menuIndexs"></param>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult Adds(string menuIndexs, int groupIndex)
		{
			string loginId = HttpContext.Items["LoginId"] as string;
			var result = new Class_Response { code = 0, errMsg = "" };

			if (Globals.IsAdmin(loginId))
			{
				foreach (string m in menuIndexs.Split(','))
				{
					if (int.TryParse(m, out int tmp))
					{
						tmp = _DbMenuUserGroup.Add(tmp, groupIndex, loginId);

						if (result.errMsg != "")
							result.errMsg += ",";

						if (tmp < 0)
						{
							result.code = -1;
						}
						result.errMsg += $"{{ \"menuIndex\":{m}, \"res\":{tmp} }}";
					}
				}
			}
			else
			{
				result.code = -1;
				result.errMsg = "非管理者";
			}

			return this.Content(JsonConvert.SerializeObject(result), "application/json");
		}
		#endregion ContentResult Adds(string menuIndexs, int groupIndex)
	}
}