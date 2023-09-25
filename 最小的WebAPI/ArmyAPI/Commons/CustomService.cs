namespace ArmyAPI.Commons
{
	public class CustomService
	{
		private readonly IWebHostEnvironment _env;

		public CustomService(IWebHostEnvironment environment)
		{
			_env = environment;
		}

		public string GetHtmlFilePath(string filename)
		{
			var filepath = _env.ContentRootPath + @"\" + filename;

			return filepath;
		}
	}
}
