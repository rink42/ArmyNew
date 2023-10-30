using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArmyAPI.Commons;
using ArmyAPI.Data;
using ArmyAPI.Filters;
using ArmyAPI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace ArmyAPI.Controllers
{
    public class MenusController : Controller
    {
		private static string _ConnectionString = ConfigurationManager.ConnectionStrings["Army2ConnectionString"].ConnectionString;
		private MsSqlDataProvider.DB_Menus _DbMenus = new MsSqlDataProvider.DB_Menus(_ConnectionString);

		private JsonSerializerSettings _JsonSerializerSettings = new JsonSerializerSettings();

		private string _Id = "Admin";

		public MenusController()
		{
			_JsonSerializerSettings.ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() };
		}

		// GET: Menus
		public ActionResult Index()
        {
            return View();
		}

		#region string GetAll(bool showDisable)
		[CustomAuthorizationFilter]
		[HttpPost]
		public string GetAll(bool showDisable)
		{
			List<Menus> menus = BuildMenuTree(_DbMenus.GetAll(showDisable), 0);

			return JsonConvert.SerializeObject(menus, _JsonSerializerSettings);
		}
		#endregion string GetAll(bool showDisable)

		#region string GetPrev4(bool showDisable)
		[CustomAuthorizationFilter]
		[HttpPost]
		public string GetPrev4(bool showDisable)
		{
			List<Menus> menus = BuildMenuTree(_DbMenus.GetPrev4(showDisable), 0);

			return JsonConvert.SerializeObject(menus);
		}
		#endregion string GetPrev4(bool showDisable)

		#region string GetWithoutFix(bool showDisable)
		[CustomAuthorizationFilter]
		[HttpPost]
		public string GetWithoutFix(bool showDisable)
		{
			List<Menus> menus = BuildMenuTree(_DbMenus.GetWithoutFix(showDisable), 0);

			return JsonConvert.SerializeObject(menus);
		}
		#endregion string GetWithoutFix(bool showDisable)

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

		#region int Add(string title, int parentIndex, string route_Tableau, bool isEnable)
		/// <summary>
		/// 新增
		/// </summary>
		/// <param name="title"></param>
		/// <param name="parentIndex"></param>
		/// <param name="route_Tableau"></param>
		/// <param name="isEnable"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public int Add(string title, int parentIndex, string route_Tableau, bool isEnable)
		{
			int result = _DbMenus.Add(title, parentIndex, route_Tableau, isEnable, "Admin");

			return result;
		}
		#endregion int Add(string title, int parentIndex, string route_Tableau, bool isEnable)

		#region int Update(int index, string newTitle, bool? isEnable, string changeParent)
		/// <summary>
		/// 更新
		/// </summary>
		/// <param name="index"></param>
		/// <param name="newTitle"></param>
		/// <param name="isEnable"></param>
		/// <param name="changeParent">變更所屬上層。JSON 格式 {'o': '舊的Index', 'n': '新的Index'}</param>
		/// <returns></returns>
		[HttpPost]
		public int Update(int index, string newTitle, bool? isEnable, string changeParent)
		{
			ChangeParent cp = null;
			if (!string.IsNullOrEmpty(changeParent))
			{
				try
				{
					cp = JsonConvert.DeserializeObject<ChangeParent>(changeParent);
				}
				catch (Exception ex)
				{
					WriteLog.Log($"changeParent 格式錯誤！ ({changeParent})\nex = {ex.ToString()}");
				}
			}
			int result = _DbMenus.Update(index, newTitle, isEnable, _Id, cp);

			return result;
		}
		#endregion int Update(int index, string newTitle, bool? isEnable, string changeParent)

		#region string UpdateAll(int index, string newTitle, bool? isEnable, string changeParent)
		/// <summary>
		/// 更新全部
		/// </summary>
		/// <param name="menusJson"></param>
		/// <returns></returns>
		[CustomAuthorizationFilter]
		[HttpPost]
		public string UpdateAll(string menusJson)
		{

			Menus[] menus = null;
			if (!string.IsNullOrEmpty(menusJson))
			{
				try
				{
					menus = JsonConvert.DeserializeObject<Menus[]>(menusJson);
					List<Menus> flattenedMenuList = FlattenMenus(menus);

					var result = _DbMenus.UpdateMultiData(flattenedMenuList.ToArray(), "Admin");

					return JsonConvert.SerializeObject(result);
				}
				catch (Exception ex)
				{
					WriteLog.Log($"轉換失敗！ ({menusJson})\nex = {ex.ToString()}");
				}
			}

			return "";
		}
		#endregion string UpdateAll(int index, string newTitle, bool? isEnable, string changeParent)

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
			int result = _DbMenus.Delete(index, "Admin");

			return result;
		}
		#endregion int Delete(int index)

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
	}
}