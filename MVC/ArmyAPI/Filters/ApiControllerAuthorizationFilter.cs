using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using ArmyAPI.Commons;


namespace ArmyAPI.Filters
{
	public class ApiControllerAuthorizationFilter : AuthorizationFilterAttribute
	{
		public override void OnAuthorization(HttpActionContext actionContext)
		{
			string controllerName = actionContext.ControllerContext.ControllerDescriptor.ControllerName;
			string actionName = actionContext.ActionDescriptor.ActionName;
			(new Globals()).CustomAuthorizationFilter(System.Web.HttpContext.Current, controllerName, actionName);
		}
	}
}