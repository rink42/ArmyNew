using System.Collections.Generic;
using System.Web.Mvc;
using ArmyAPI.Commons;
using ArmyAPI.Filters;
using ArmyAPI.Models;
using Newtonsoft.Json;

namespace ArmyAPI.Controllers
{
	public class MenuUserController : BaseController
    {
		public MenuUserController()
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


		#region string Add(int menuIndex, string userId)
		/// <summary>
		/// 新增
		/// </summary>
		/// <param name="menuIndex"></param>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public string Add(int menuIndex, string userId)
		{
			string loginId = TempData["LoginAcc"].ToString();
			string result = "";

			if (Globals.IsAdmin(loginId))
			{
				result = _DbMenuUser.Add(menuIndex, userId, loginId).ToString();
			}
			else
			{
				result = "非管理者";
			}

			return result;
		}
		#endregion string Add(int menuIndex, string userId)

		#region int Adds(string menuIndexs, string userId)
		/// <summary>
		/// 新增
		/// </summary>
		/// <param name="menuIndexs"></param>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult Adds(string menuIndexs, string userId)
		{
			string loginId = TempData["LoginAcc"].ToString();
			var result = new Class_Response { code = 0, errMsg = "" };
			bool isAdmin = _DbUserGroup.IsAdmin(loginId);
			if (isAdmin)
			{

				if (Globals.IsAdmin(loginId))
				{
					foreach (string m in menuIndexs.Split(','))
					{
						if (int.TryParse(m, out int tmp))
						{
							tmp = _DbMenuUser.Add(tmp, userId, loginId);

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
			}
			return this.Content(JsonConvert.SerializeObject(result), "application/json");
		}
		#endregion ContentResult Adds(string menuIndexs, string userId)
	}
}