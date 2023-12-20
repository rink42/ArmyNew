using System.Web.Mvc;
using ArmyAPI.Commons;

namespace ArmyAPI.Filters
{
	public class CheckUserIDFilter : ActionFilterAttribute
	{
		private string _ParameterName;

		public CheckUserIDFilter(string parameterName)
		{
			this._ParameterName = parameterName;
		}
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			object userId = filterContext.ActionParameters[_ParameterName];
			// 檢查是否存在 UserID 參數
			if (filterContext.ActionParameters.ContainsKey(_ParameterName) && userId != null)
			{
				string msg = "";
				// 在這裡添加您的驗證邏輯
				if (!(new Class_TaiwanID()).Check((string)userId, out msg))
				{
					// 驗證失敗時，可以重定向到錯誤頁面或者返回適當的錯誤結果
					filterContext.Result = new HttpStatusCodeResult(401, msg);
				}
			}
			else
			{
				// 如果沒有 UserID 參數，可以根據需要採取適當的措施
				// 例如，重定向到錯誤頁面或者返回適當的錯誤結果
				filterContext.Result = new HttpStatusCodeResult(401, $"缺少 {_ParameterName} 參數");
			}

			base.OnActionExecuting(filterContext);
		}
	}
}