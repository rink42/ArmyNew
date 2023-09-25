namespace ArmyAPI.Commons
{
	public static class CoreHttpContext
	{
		private static Microsoft.AspNetCore.Hosting.IWebHostEnvironment? HostEnviroment;

		public static string RootPath => HostEnviroment!.WebRootPath != null ? HostEnviroment!.WebRootPath : HostEnviroment!.ContentRootPath;

		public static string MapPath(string path)
		{
			return Path.Combine(RootPath, path);
		}

		internal static void Configure(Microsoft.AspNetCore.Hosting.IWebHostEnvironment hostEnviroment)
		{
			HostEnviroment = hostEnviroment;
		}
	}

	public static class StaticHostEnviromentExtensions
	{
		public static IApplicationBuilder UseStaticHostEnviroment(this IApplicationBuilder app)
		{
			var webHostEnviroment = app.ApplicationServices.GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
			CoreHttpContext.Configure(webHostEnviroment);
			return app;
		}
	}
}
