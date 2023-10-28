using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArmyAPI.Data;
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
		[HttpPost]
		public string GetAll(bool showDisable)
		{
			List<Menus> menus = BuildMenuTree(_DbMenus.GetAll(showDisable), 0);

			return JsonConvert.SerializeObject(menus);
		}
		#endregion string GetAll(bool showDisable)

		#region string GetPrev4(bool showDisable)
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

		#region int Update(int index, string id, string newTitle, bool? isEnable)
		[HttpPost]
		public int Update(int index, string id, string newTitle, bool? isEnable)
		{
			int result = _DbMenus.Update(index, id, newTitle, isEnable, _Id);

			return result;
		}
		#endregion int Update(int index, string id, string newTitle, bool? isEnable)
	}
}