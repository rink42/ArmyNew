using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ArmyAPI
{
	public class WebApiApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			GlobalConfiguration.Configure(WebApiConfig.Register);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
		}

		protected void Application_BeginRequest()
		{
			if (Request.Headers.AllKeys.Contains("Origin") && Request.HttpMethod == "OPTIONS")
			{
				Response.Flush();
			}
		}

		protected void Application_EndRequest()
		{
			Session.Abandon();
		}

		protected void Application_PreSendRequestHeaders()
		{
			Response.Headers.Remove("Access-Control-Allow-Origin");
			Response.Headers.Add("Access-Control-Allow-Origin", "*");

			if (Request.Headers.AllKeys.Contains("Origin") && Request.HttpMethod == "OPTIONS")
			{
				Response.Headers.Remove("Access-Control-Allow-Methods");
				Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");

				Response.Headers.Remove("Access-Control-Allow-Headers");
				Response.Headers.Add("Access-Control-Allow-Headers", "*");
			}
		}
	}
}
