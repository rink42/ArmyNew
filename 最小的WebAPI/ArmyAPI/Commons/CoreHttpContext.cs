namespace ArmyAPI.Commons
{
	public static class CoreHttpContext
	{
		[Obsolete]
		private static Microsoft.AspNetCore.Hosting.IHostingEnvironment? _HostEnviroment;

		[Obsolete]
		public static string WebPath => _HostEnviroment!.WebRootPath;

		[Obsolete]
		public static string MapPath(string path)
		{
			return Path.Combine(_HostEnviroment!.WebRootPath, path);
		}

		[Obsolete]
		internal static void Configure(Microsoft.AspNetCore.Hosting.IHostingEnvironment hostEnviroment)
		{
			_HostEnviroment = hostEnviroment;
		}
	}

	public static class StaticHostEnviromentExtensions
	{
		[Obsolete]
		public static IApplicationBuilder UseStaticHostEnviroment(this IApplicationBuilder app)
		{
			var webHostEnviroment = app.ApplicationServices.GetRequiredService<Microsoft.AspNetCore.Hosting.IHostingEnvironment>();
			CoreHttpContext.Configure(webHostEnviroment);
			return app;
		}
	}
}
