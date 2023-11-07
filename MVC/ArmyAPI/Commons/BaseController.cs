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
		protected MsSqlDataProvider.DB_Army_Rank _DbArmy_Ranks = new MsSqlDataProvider.DB_Army_Rank(_ConnectionString);
	}
}