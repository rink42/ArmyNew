using ArmyAPI.Commons;
using ArmyAPI.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Runtime.Caching;
using System.Web.Mvc;

namespace ArmyAPI.Filters
{
    public class ControllerAuthorizationFilter : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			try
			{
				string controllerName = filterContext.RouteData.Values["controller"].ToString();
				string actionName = filterContext.RouteData.Values["action"].ToString();
				WriteLog.Log($"controllerName = {controllerName}, actionName = {actionName}");
				// 在這裡執行您的驗證邏輯
				//if (!IsAuthorized(filterContext))

				string result = IsOK(filterContext);
				if ("超時|檢查不通過".Split('|').Contains(result))
				{
					//filterContext.Result = new HttpUnauthorizedResult(result);
					filterContext.HttpContext.Response.StatusCode = 401; // 401 表示未经授权
					filterContext.Result = new ContentResult
					{
						Content = result,
						ContentType = "text/plain"
					};

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

				filterContext.HttpContext.Items["LoginId"] = (string)jsonObj.a;

				Globals._Cache.Remove($"User_{(string)jsonObj.a}");
				UserDetail user = (new ArmyAPI.Controllers.UserController()).GetDetailByUserId((string)jsonObj.a);

                Globals._Cache.Add($"User_{(string)jsonObj.a}", user, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddHours(8) });

				filterContext.HttpContext.Items[$"User_{(string)jsonObj.a}"] = user;

				filterContext.HttpContext.Items["IsAdmin"] = (new ArmyAPI.Commons.BaseController())._DbUserGroup.IsAdmin((string)jsonObj.a);


				filterContext.HttpContext.Response.Headers.Remove("Army");
				filterContext.HttpContext.Response.Headers.Remove("ArmyC");
				filterContext.HttpContext.Response.Headers.Remove("Armyc");
				filterContext.HttpContext.Response.Headers.Add("Army", (string)jsonObj.c);
				filterContext.HttpContext.Response.Headers.Add("ArmyC", (string)jsonObj.m);
				filterContext.HttpContext.Response.Headers.Add("Armyc", (string)jsonObj.m);
			}
			catch (Exception ex)
			{
				WriteLog.Log("CustomAuthorizationFilter", ex.ToString());

				filterContext.HttpContext.Response.StatusCode = 401;
				filterContext.Result = new ContentResult
				{
					// 直接輸出會觸發 源掃 風險
					//Content = ex.ToString(),
					Content = "驗證失敗",
					ContentType = "text/plain"
				};
			}
		}

		private string IsOK(ActionExecutingContext filterContext)
		{
			string headerKey = "Army";
			string s = "";

			if (filterContext.HttpContext.Request.Headers.AllKeys.Contains(headerKey))
			{
				s = filterContext.HttpContext.Request.Headers[headerKey];
			}

			if (string.IsNullOrEmpty(s))
			{
				s = filterContext.HttpContext.Request.Cookies.Get(headerKey).Value;
			}

			string c = "";

			headerKey = "Armyc";

			if (filterContext.HttpContext.Request.Headers.AllKeys.Contains(headerKey))
			{
				c = filterContext.HttpContext.Request.Headers[headerKey];
			}

			if (string.IsNullOrEmpty(c))
			{
				c = filterContext.HttpContext.Request.Cookies.Get(headerKey).Value;
			}

			if (c.Split(',').Length == 2)
			{
				if (c.Split(',')[0].Trim() == c.Split(',')[1].Trim())
					c = c.Split(',')[0].Trim();
			}

			//string result = (new ArmyAPI.Controllers.LoginController()).CheckSession(filterContext.HttpContext.Request.Form["c"], s);
			string result = (new ArmyAPI.Controllers.LoginController()).CheckSession(c, s);
			//WriteLog.Log($"c = {c}, s = {s}");
			// 實現自定義的驗證邏輯
			// 返回true表示通過驗證，返回false表示未通過驗證
			return result;
		}
	}
}