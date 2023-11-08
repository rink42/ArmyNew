using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArmyAPI.Data;

namespace ArmyAPI.Commons
{
	public class BaseController : Controller
	{
		private static string _ConnectionString = ConfigurationManager.ConnectionStrings["ArmyWebConnectionString"].ConnectionString;
		protected MsSqlDataProvider.DB_UserGroup _DbUserGroup = new MsSqlDataProvider.DB_UserGroup(_ConnectionString);
		protected MsSqlDataProvider.DB_Limits _DbLimits = new MsSqlDataProvider.DB_Limits(_ConnectionString);
		protected MsSqlDataProvider.DB_Menus _DbMenus = new MsSqlDataProvider.DB_Menus(_ConnectionString);
		protected MsSqlDataProvider.DB_Users _DbUsers = new MsSqlDataProvider.DB_Users(_ConnectionString);
		protected MsSqlDataProvider.DB_Army _DbArmy = new MsSqlDataProvider.DB_Army(_ConnectionString);
		protected MsSqlDataProvider.DB_MenuUser _DbMenuUser = new MsSqlDataProvider.DB_MenuUser(_ConnectionString);
		protected MsSqlDataProvider.DB_MenuUserGroup _DbMenuUserGroup = new MsSqlDataProvider.DB_MenuUserGroup(_ConnectionString);
	}
}