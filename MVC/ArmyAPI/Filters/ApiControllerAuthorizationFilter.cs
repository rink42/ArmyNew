using System.Linq;
using System.Net.Http;
using System.Net;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Mvc;
using ArmyAPI.Commons;


namespace ArmyAPI.Filters
{
	public class ApiControllerAuthorizationFilter : AuthorizationFilterAttribute
	{
		public override void OnAuthorization(HttpActionContext actionContext)
		{
			string controllerName = actionContext.ControllerContext.ControllerDescriptor.ControllerName;
			string actionName = actionContext.ActionDescriptor.ActionName;
			string outMsg = string.Empty;
			(new Globals()).CustomAuthorizationFilter(System.Web.HttpContext.Current, out outMsg, controllerName, actionName);


			if (!string.IsNullOrEmpty(outMsg))
			{
				if ("超時|檢查不通過".Split('|').Contains(outMsg))
				{
					actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
					{
						Content = new StringContent(outMsg),
						ReasonPhrase = "Unauthorized"
					};

					return;
				}
			}
		}
	}
}