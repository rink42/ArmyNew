using System.Configuration;
using System.Web.Mvc;
using ArmyAPI.Data;

namespace ArmyAPI.Commons
{
	public class BaseController : Controller
	{
		public static string _ConnectionString = ConfigurationManager.ConnectionStrings["ArmyWebConnectionString"].ConnectionString;
		public MsSqlDataProvider.DB_UserGroup _DbUserGroup = new MsSqlDataProvider.DB_UserGroup(_ConnectionString);
		protected MsSqlDataProvider.DB_Limits _DbLimits = new MsSqlDataProvider.DB_Limits(_ConnectionString);
		protected MsSqlDataProvider.DB_Menus _DbMenus = new MsSqlDataProvider.DB_Menus(_ConnectionString);
		protected MsSqlDataProvider.DB_Users _DbUsers = new MsSqlDataProvider.DB_Users(_ConnectionString);
		protected MsSqlDataProvider.DB_Army _DbArmy = new MsSqlDataProvider.DB_Army(_ConnectionString);
		protected MsSqlDataProvider.DB_MenuUser _DbMenuUser = new MsSqlDataProvider.DB_MenuUser(_ConnectionString);
		protected MsSqlDataProvider.DB_MenuUserGroup _DbMenuUserGroup = new MsSqlDataProvider.DB_MenuUserGroup(_ConnectionString);
		protected MsSqlDataProvider.DB_Tableau _DbTableau = new MsSqlDataProvider.DB_Tableau(_ConnectionString);
		protected MsSqlDataProvider.DB_LimitsUser _DbLimitsUser = new MsSqlDataProvider.DB_LimitsUser(_ConnectionString);
	}
}