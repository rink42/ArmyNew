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
		[CustomAuthorizationFilter]
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
		[CustomAuthorizationFilter]
		[HttpPost]
		public int Add(string title, int sort, bool isEnable)
		{
			int result = _DbUserGroup.Add(title, sort, isEnable, TempData["LoginAcc"].ToString());

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
			int result = _DbUserGroup.Delete(index, TempData["LoginAcc"].ToString());

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
			bool result = _DbUserGroup.IsAdmin(TempData["LoginAcc"].ToString());

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