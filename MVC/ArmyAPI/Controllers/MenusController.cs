using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArmyAPI.Data;
using ArmyAPI.Filters;
using ArmyAPI.Models;
using Newtonsoft.Json;

namespace ArmyAPI.Controllers
{
    public class MenusController : Controller
    {
		private static string _ConnectionString = ConfigurationManager.ConnectionStrings["Army2ConnectionString"].ConnectionString;
		private MsSqlDataProvider.DB_Menus _DbMenus = new MsSqlDataProvider.DB_Menus(_ConnectionString);

		private string _Id = "Admin";
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

			return JsonConvert.SerializeObject(menus);
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

		// 遞迴構建 Menu Tree
		#region private List<Menus> BuildMenuTree(List<Menus> menuData, int parentIndex)
		private List<Menus> BuildMenuTree(List<Menus> menuData, int parentIndex)
		{
			List<Menus> menuTree = new List<Menus>();

			foreach (var item in menuData.Where(x => x.ParentIndex == parentIndex))
			{
				item.Items = BuildMenuTree(menuData, item.Index);
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
	}
}