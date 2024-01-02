using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using ArmyAPI.Commons;
using ArmyAPI.Models;
using Newtonsoft.Json;


namespace ArmyAPI.Filters
{
	public class ApiControllerAuthorizationFilter : AuthorizationFilterAttribute
	{
		public override void OnAuthorization(HttpActionContext actionContext)
		{
			try
			{
				string controllerName = actionContext.ControllerContext.ControllerDescriptor.ControllerName;
				string actionName = actionContext.ActionDescriptor.ActionName;
				WriteLog.Log($"controllerName = {controllerName}, actionName = {actionName}");

				string result = IsOK(actionContext);
				if ("超時|檢查不通過".Split('|').Contains(result))
				{
					actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
					{
						Content = new StringContent(result),
						ReasonPhrase = "Unauthorized"
					};

					return;
				}
				else if (controllerName == "Login" && actionName == "CheckSession")
				{
					actionContext.Response = new HttpResponseMessage(HttpStatusCode.OK)
					{
						Content = new StringContent(result),
						ReasonPhrase = "OK"
					};

					return;
				}

				var jsonObj = JsonConvert.DeserializeObject<dynamic>(result);

				actionContext.Request.Properties["LoginId"] = (string)jsonObj.a;

				UserDetail user = Globals._Cache.Get($"User_{(string)jsonObj.a}") as UserDetail;
				if (user == null)
				{
					user = (new ArmyAPI.Controllers.UserController()).GetDetailByUserId((string)jsonObj.a);
					Globals._Cache.Add($"User_{(string)jsonObj.a}", user, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(3600) });
				}

				actionContext.Request.Properties[$"User_{(string)jsonObj.a}"] = user;

				actionContext.Request.Properties["IsAdmin"] = (new ArmyAPI.Commons.BaseController())._DbUserGroup.IsAdmin((string)jsonObj.a);

				//actionContext.Response = actionContext.Response ?? new HttpResponseMessage();

				//actionContext.Response.Headers.Remove("Army");
				//actionContext.Response.Headers.Remove("ArmyC");
				//actionContext.Response.Headers.Remove("Armyc");
				//actionContext.Response.Headers.Add("Army", (string)jsonObj.c);
				//actionContext.Response.Headers.Add("ArmyC", (string)jsonObj.m);
				//actionContext.Response.Headers.Add("Armyc", (string)jsonObj.m);
			}
			catch (Exception ex)
			{
				WriteLog.Log("ApiAuthorizationFilter", ex.ToString());

				actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
				{
					Content = new StringContent("驗證失敗"),
					ReasonPhrase = "Unauthorized"
				};
			}
		}

		private string IsOK(HttpActionContext actionContext)
		{
			string headerKey = "Army";
			string s = "";

			if (actionContext.Request.Headers.Contains(headerKey))
			{
				s = actionContext.Request.Headers.GetValues(headerKey).FirstOrDefault();
			}

			if (string.IsNullOrEmpty(s))
			{
				var cookie = actionContext.Request.Headers.GetCookies(headerKey).FirstOrDefault();
				if (cookie != null)
				{
					s = cookie[headerKey].Value;
				}
			}

			string c = "";

			headerKey = "Armyc";

			if (actionContext.Request.Headers.Contains(headerKey))
			{
				c = actionContext.Request.Headers.GetValues(headerKey).FirstOrDefault();
			}

			if (string.IsNullOrEmpty(c))
			{
				var cookie = actionContext.Request.Headers.GetCookies(headerKey).FirstOrDefault();
				if (cookie != null)
				{
					c = cookie[headerKey].Value;
				}
			}

			if (c.Split(',').Length == 2)
			{
				if (c.Split(',')[0].Trim() == c.Split(',')[1].Trim())
					c = c.Split(',')[0].Trim();
			}

			string result = (new ArmyAPI.Controllers.LoginController()).CheckSession(c, s);
			return result;
		}
	}
}