using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArmyAPI.Commons;
using ArmyAPI.Models;
using Newtonsoft.Json;
using static ArmyAPI.Data.MsSqlDataProvider;

namespace ArmyAPI.Filters
{
	public class CustomAuthorizationFilter : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			string controllerName = filterContext.RouteData.Values["controller"].ToString();
			string actionName = filterContext.RouteData.Values["action"].ToString();
			WriteLog.Log($"controllerName = {controllerName}, actionName = {actionName}");
			// 在這裡執行您的驗證邏輯
			//if (!IsAuthorized(filterContext))
			string result = IsAuthorized(filterContext);
			if ("超時|檢查不通過".Split('|').Contains(result))
			{
				//filterContext.Result = new HttpUnauthorizedResult(result);
				filterContext.HttpContext.Response.StatusCode = 401; // 401 表示未经授权
				filterContext.Result = new ContentResult
				{
					Content = result,
					ContentType = "text/plain"
				};

				//WriteLog.Log($"controllerName = {controllerName}, actionName = {actionName}, = {}, = {},");

				return;
			}
			else if (controllerName == "Login" && actionName == "CheckSession")
			{
				filterContext.HttpContext.Response.StatusCode = 200;
				filterContext.Result = new ContentResult
				{
					Content = result,
					ContentType = "text/plain"
				};

				return;
			}
			var jsonObj = JsonConvert.DeserializeObject<dynamic>(result);
			filterContext.Controller.TempData["LoginAcc"] = jsonObj.a;

			filterContext.HttpContext.Items["LoginId"] = (string)jsonObj.a;
			filterContext.HttpContext.Items["IsAdmin"] = (new ArmyAPI.Commons.BaseController())._DbUserGroup.IsAdmin((string)jsonObj.a);


			filterContext.HttpContext.Response.Headers.Remove("Army");
			filterContext.HttpContext.Response.Headers.Remove("ArmyC");
			filterContext.HttpContext.Response.Headers.Remove("Armyc");
			filterContext.HttpContext.Response.Headers.Add("Army", (string)jsonObj.c);
			filterContext.HttpContext.Response.Headers.Add("ArmyC", (string)jsonObj.m);
			filterContext.HttpContext.Response.Headers.Add("Armyc", (string)jsonObj.m);
		}

		private string IsAuthorized(ActionExecutingContext filterContext)
		{
			string headerKey = "Army";
			string s = "";

			if (filterContext.HttpContext.Request.Headers.AllKeys.Contains(headerKey))
				s = filterContext.HttpContext.Request.Headers[headerKey];

			headerKey = "ArmyC";
			string c = "";

			if (filterContext.HttpContext.Request.Headers.AllKeys.Contains(headerKey))
				c = filterContext.HttpContext.Request.Headers[headerKey];

			headerKey = "Armyc";
			c = "";

			if (filterContext.HttpContext.Request.Headers.AllKeys.Contains(headerKey))
				c = filterContext.HttpContext.Request.Headers[headerKey];

			//string result = (new ArmyAPI.Controllers.LoginController()).CheckSession(filterContext.HttpContext.Request.Form["c"], s);
			string result = (new ArmyAPI.Controllers.LoginController()).CheckSession(c, s);

			// 實現自定義的驗證邏輯
			// 返回true表示通過驗證，返回false表示未通過驗證
			//return "超時|檢查不通過".Split('|').Contains(result) ? false : true; // 在此示例中，總是通過驗證
			return result;
		}
	}
}