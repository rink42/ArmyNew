using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using ArmyAPI.Commons;
using ArmyAPI.Filters;
using ArmyAPI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ArmyAPI.Controllers
{
	public class MenusController : BaseController
    {

		private JsonSerializerSettings _JsonSerializerSettings = new JsonSerializerSettings();

		public MenusController()
		{
			_JsonSerializerSettings.ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
		}

		// GET: Menus
		public ActionResult Index()
        {
            return View();
		}

		#region ContentResult GetAll_Admin(bool showDisable)
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult GetAll_Admin(bool showDisable)
		{
			List<Menus> menus = BuildMenuTree(_DbMenus.GetAll(showDisable), 0);

			return this.Content(JsonConvert.SerializeObject(menus), "application/json");
		}
		#endregion ContentResult GetAll_Admin(bool showDisable)

		#region ContentResult GetAll(bool showDisable)
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult GetAll(bool showDisable)
		{
			string loginId = TempData["LoginAcc"].ToString();

			List<Menus> menus = BuildMenuTree(_DbMenus.GetAll(showDisable, loginId), 0);

			return this.Content(JsonConvert.SerializeObject(menus), "application/json");
		}
		#endregion ContentResult GetAll(bool showDisable)

		#region ContentResult GetLeftMenu()
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult GetLeftMenu()
		{
			Users user = Globals.GetCacheUser();

			string loginId = TempData["LoginAcc"].ToString();
			var result = new Class_Response { code = 0, errMsg = "" };

			if (string.IsNullOrEmpty(loginId))
			{
				result.code = -1;
				result.errMsg = "登入帳號不存在";
			}
			else if (user != null && user.UserID != loginId)
			{
                result.code = -1;
                result.errMsg = "登入帳號錯誤";
            }
			else
			{
				List<Menus> menus = BuildMenuTree(_DbMenus.GetLeftMenu(loginId), 0);

				result.errMsg = JsonConvert.SerializeObject(menus);
			}

			return this.Content(JsonConvert.SerializeObject(result), "application/json");
		}
		#endregion ContentResult GetLeftMenu()

		#region ContentResult GetWithoutFix(bool showDisable)
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult GetWithoutFix(bool showDisable)
		{
			string loginId = TempData["LoginAcc"].ToString();

			List<Menus> menus = BuildMenuTree(_DbMenus.GetWithoutFix(showDisable, loginId), 0);

			return this.Content(JsonConvert.SerializeObject(menus, _JsonSerializerSettings), "application/json");
		}
		#endregion ContentResult GetWithoutFix(bool showDisable)

		// 遞迴構建 Menu Tree
		#region private List<Menus> BuildMenuTree(List<Menus> menuData, int parentIndex)
		private List<Menus> BuildMenuTree(List<Menus> menuData, int parentIndex)
		{
			List<Menus> menuTree = new List<Menus>();

			foreach (var item in menuData.Where(x => x.ParentIndex == parentIndex))
			{
				item.Children = BuildMenuTree(menuData, item.Index);
				menuTree.Add(item);
			}

			return menuTree;
		}
		#endregion private List<Menus> BuildMenuTree(List<Menus> menuData, int parentIndex)

		#region int Add(string title, int parentIndex, string route_Tableau, int level, bool isEnable)
		/// <summary>
		/// 新增
		/// </summary>
		/// <param name="title"></param>
		/// <param name="parentIndex"></param>
		/// <param name="level"></param>
		/// <param name="route_Tableau"></param>
		/// <param name="isEnable"></param>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public int Add(string title, int parentIndex, int level, string route_Tableau, bool isEnable)
		{
			int result = _DbMenus.Add(title, parentIndex, level, route_Tableau, isEnable, TempData["LoginAcc"].ToString());

			return result;
		}
		#endregion int Add(string title, int parentIndex, int level, string route_Tableau, bool isEnable)

		#region string Update(int index, string newTitle, bool? isEnable, string changeParent, int level, string route_Tableau)
		/// <summary>
		/// 更新
		/// </summary>
		/// <param name="index"></param>
		/// <param name="newTitle"></param>
		/// <param name="isEnable"></param>
		/// <param name="level"></param>
		/// <param name="changeParent">變更所屬上層。JSON 格式 {'o': '舊的Index', 'n': '新的Index'}</param>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public string Update(int index, string newTitle, bool? isEnable, string changeParent, int level, string route_Tableau)
		{
			string result = "";
			ChangeParent cp = null;
			if (!string.IsNullOrEmpty(changeParent))
			{
				try
				{
					cp = JsonConvert.DeserializeObject<ChangeParent>(changeParent);
				}
				catch (Exception ex)
				{
					result = $"changeParent 格式錯誤！ ({changeParent})\nex = {ex.ToString()}";
					WriteLog.Log(result);
					Response.StatusCode = 401;
				}
			}
			if (string.IsNullOrEmpty(result))
			{
				try
				{
					result = _DbMenus.Update(index, newTitle, isEnable, TempData["LoginAcc"].ToString(), cp, level, route_Tableau).ToString();
				}
				catch (Exception ex)
				{
					result = ex.ToString();
					Response.StatusCode = 401;
				}
			}

			return result;
		}
		#endregion string Update(int index, string newTitle, bool? isEnable, string changeParent, int level, string route_Tableau)

		#region ContentResult UpdateAll(int index, string newTitle, bool? isEnable, string changeParent)
		/// <summary>
		/// 更新全部
		/// </summary>
		/// <param name="menusJson"></param>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public ContentResult UpdateAll(string menusJson)
		{
			Menus[] menus = null;
			string result = "";

			if (!string.IsNullOrEmpty(menusJson))
			{
				try
				{
					menus = JsonConvert.DeserializeObject<Menus[]>(menusJson);
					List<Menus> flattenedMenuList = FlattenMenus(menus);

					var result1 = _DbMenus.AddUpdateMultiData(flattenedMenuList.ToArray(), TempData["LoginAcc"].ToString());

					if (result1.Rows.Count != flattenedMenuList.Count)
						Response.StatusCode = 401;

					result = JsonConvert.SerializeObject(result1);
				}
				catch (Exception ex)
				{
					WriteLog.Log($"轉換失敗！ ({menusJson})\nex = {ex.ToString()}");

					DataTable dt = Globals.CreateResultTable();
					DataRow dr = dt.NewRow();
					dr[0] = "JSON 轉換失敗";

					dt.Rows.Add(dr);

					result = JsonConvert.SerializeObject(dt);
					Response.StatusCode = 401;
				}
			}

			return this.Content(result, "application/json");
		}
		#endregion ContentResult UpdateAll(int index, string newTitle, bool? isEnable, string changeParent)

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
			int result = _DbMenus.Delete(index, TempData["LoginAcc"].ToString());

			return result;
		}
		#endregion int Delete(int index)

		#region int Deletes(string indexes)
		/// <summary>
		/// 刪除
		/// </summary>
		/// <param name="indexes"></param>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public string Deletes(string indexes)
		{
			if (indexes.Split(',').Any(part => !string.IsNullOrWhiteSpace(part) && !part.All(char.IsDigit)))
			{
				Response.StatusCode = 401;
				return "indexes 含有非數字資料";
			}

			int result = _DbMenus.Deletes(indexes, TempData["LoginAcc"].ToString());

			return result.ToString();
		}
		#endregion int Deletes(string indexes)

		#region private static List<Menus> FlattenMenus(Menus[] menus) 把 巢狀Menu 扁平化
		private static List<Menus> FlattenMenus(Menus[] menus)
		{
			List<Menus> flattenedList = new List<Menus>();

			foreach (var menu in menus)
			{
				flattenedList.Add(menu);

				if (menu.Children != null && menu.Children.Count > 0)
				{
					flattenedList.AddRange(FlattenMenus(menu.Children.ToArray()));
					menu.Children = null; // Remove nested children
				}
			}

			return flattenedList;
		}
		#endregion private static List<Menus> FlattenMenus(Menus[] menus)
	}
}