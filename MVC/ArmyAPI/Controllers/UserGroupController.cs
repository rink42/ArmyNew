using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;
using ArmyAPI.Commons;
using ArmyAPI.Data;
using ArmyAPI.Filters;
using ArmyAPI.Models;
using Newtonsoft.Json;

namespace ArmyAPI.Controllers
{
	public class UserGroupController : BaseController
	{
		
		// GET: UserGroup
		public ActionResult Index()
        {
            return View();
		}

		#region ContentResult GetAll()
		[ControllerAuthorizationFilter]
		[HttpPost]
		public ContentResult GetAll()
		{
			List<UserGroup> userGroup = _DbUserGroup.GetAll();

			return this.Content(JsonConvert.SerializeObject(userGroup), "application/json");
		}
		#endregion ContentResult GetAll()

		#region int Add(string title, int sort, bool isEnable)
		/// <summary>
		/// 新增
		/// </summary>
		/// <param name="title"></param>
		/// <param name="sort"></param>
		/// <param name="isEnable"></param>
		/// <returns></returns>
		[ControllerAuthorizationFilter]
		[HttpPost]
		public int Add(string title, int sort, bool isEnable)
		{
			string loginId = HttpContext.Items["LoginId"] as string;
			int result = _DbUserGroup.Add(title, sort, isEnable, loginId);

			return result;
		}
		#endregion int Add(string title, int sort, bool isEnable)

		#region int Delete(int index)
		/// <summary>
		/// 刪除
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		[ControllerAuthorizationFilter]
		[HttpPost]
		public int Delete(int index)
		{
			string loginId = HttpContext.Items["LoginId"] as string;
			int result = _DbUserGroup.Delete(index, loginId);

			return result;
		}
		#endregion int Delete(int index)

		#region bool IsAdmin()
		/// <summary>
		/// 是否為管理者
		/// </summary>
		/// <returns></returns>
		[NonAction]
		public bool IsAdmin()
		{
			string loginId = HttpContext.Items["LoginId"] as string;
			bool result = _DbUserGroup.IsAdmin(loginId);

			return result;
		}
		#endregion bool IsAdmin()

		#region bool IsAdmin(string userId)
		/// <summary>
		/// 是否為管理者
		/// </summary>
		/// <returns></returns>
		[NonAction]
		public bool IsAdmin(string userId)
		{
			bool result = _DbUserGroup.IsAdmin(userId);

			return result;
		}
		#endregion bool IsAdmin(string userId)
	}
}