using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ArmyAPI.Controllers;
using ArmyAPI.Models;
using Newtonsoft.Json;

namespace ArmyAPI.Commons
{
	public class Globals : IDisposable
    {
		#region Enum

		#region enum CacheOperators : byte
		public enum CacheOperators : byte
		{
			Add = 1,
			Update,
			Delete,
			Get
		}
        #endregion enum CacheOperators : byte

        #endregion Enum

        #region 變數

        protected bool _Disposed = false;

        private string _LoginId = "";
        private string _LoginAcc = "";

		private bool _IsAdmin1 = false;

		public static ObjectCache _Cache = MemoryCache.Default;

		#endregion 變數

		public string LoginId
        {
            get
            {
                return _LoginId;
            }
        }
        public string LoginAcc
		{
            get
            {
                return _LoginAcc;
            }
        }

        public bool IsAdmin1
        {
            get
            {
                return _IsAdmin1;
            }
        }

        #region 建構子
        public Globals()
        {
        }
        #endregion 建構子

        #region 解構子
        ~Globals()
        {
            Dispose(false);
        }
        #endregion 解構子

        #region 方法/私有方法/靜態方法

        #region Dispose()
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion Dispose()

        #region Dispose(bool isDisposing)
        protected virtual void Dispose(bool isDisposing)
        {
            if (!_Disposed)
            {
                _Disposed = true;
            }
        }
        #endregion Dispose(bool isDisposing)

        #region Close()
        public void Close()
        {
            ((IDisposable)this).Dispose();
        }
        #endregion Close()

        #region static string GetEnumDesc (Enum e)
        public static string GetEnumDesc(Enum e)
        {
            FieldInfo EnumInfo = e.GetType().GetField(e.ToString());
            DescriptionAttribute[] EnumAttributes = (DescriptionAttribute[])EnumInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (EnumAttributes.Length > 0)
                return EnumAttributes[0].Description;

            return e.ToString();
        }
        #endregion static string GetEnumDesc (Enum e)

        #region static string GetEnumDesc<T> (T e, out bool descIsExist)
        public static string GetEnumDesc<T>(T e, out bool descIsExist)
        {
            descIsExist = false;
            FieldInfo EnumInfo = e.GetType().GetField(e.ToString());
            DescriptionAttribute[] EnumAttributes = (DescriptionAttribute[])EnumInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (EnumAttributes.Length > 0)
            {
                descIsExist = true;
                return EnumAttributes[0].Description;
            }

            return e.ToString();
        }
        #endregion static string GetEnumDesc<T> (T e, out bool descIsExist)

        #region string GetFields<T> (T fields)
        public string GetFields<T>(T fields)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            foreach (T f in Enum.GetValues(typeof(T)))
            {
                if ((ulong.Parse(Convert.ChangeType(f, typeof(ulong)).ToString()) & ulong.Parse(Convert.ChangeType(fields, typeof(ulong)).ToString())) > 0)
                {
                    if (sb.Length > 0)
                        sb.Append(", ");

                    sb.Append(String.Format("[{0}]", f));
                }
            }

            return sb.ToString();
        }
        #endregion string GetFields<T> (T fields)

        #region T MyConvert<T>(string s, T defaultValue)
        public T MyConvert<T>(string s, T defaultValue)
        {
            T result = default(T);

            switch (result.GetType().ToString())
            {
                case "System.Byte":
                    byte byteTmp = default(byte);
                    if (byte.TryParse(s, out byteTmp))
                        result = (T)Convert.ChangeType(byteTmp, typeof(T));
                    else
                        result = defaultValue;
                    break;
                case "System.Int16":
                    short shortTmp = default(short);
                    if (short.TryParse(s, out shortTmp))
                        result = (T)Convert.ChangeType(shortTmp, typeof(T));
                    else
                        result = defaultValue;
                    break;
                case "System.Int32":
                    int intTmp = default(int);
                    if (int.TryParse(s, out intTmp))
                        result = (T)Convert.ChangeType(intTmp, typeof(T));
                    else
                        result = defaultValue;
                    break;
                case "System.Int64":
                    long longTmp = default(long);
                    if (long.TryParse(s, out longTmp))
                        result = (T)Convert.ChangeType(longTmp, typeof(T));
                    else
                        result = defaultValue;
                    break;
                case "System.UInt16":
                    ushort ushortTmp = default(ushort);
                    if (ushort.TryParse(s, out ushortTmp))
                        result = (T)Convert.ChangeType(ushortTmp, typeof(T));
                    else
                        result = defaultValue;
                    break;
                case "System.UInt32":
                    uint uintTmp = default(uint);
                    if (uint.TryParse(s, out uintTmp))
                        result = (T)Convert.ChangeType(uintTmp, typeof(T));
                    else
                        result = defaultValue;
                    break;
                case "System.UInt64":
                    ulong ulongTmp = default(ulong);
                    if (ulong.TryParse(s, out ulongTmp))
                        result = (T)Convert.ChangeType(ulongTmp, typeof(T));
                    else
                        result = defaultValue;
                    break;
                case "System.Single":
                    float floatTmp = default(float);
                    if (float.TryParse(s, out floatTmp))
                        result = (T)Convert.ChangeType(floatTmp, typeof(T));
                    else
                        result = defaultValue;
                    break;
                case "System.Double":
                    double doubleTmp = default(double);
                    if (double.TryParse(s, out doubleTmp))
                        result = (T)Convert.ChangeType(doubleTmp, typeof(T));
                    else
                        result = defaultValue;
                    break;
                case "System.DateTime":
                    DateTime dateTimeTmp = default(DateTime);
                    if (DateTime.TryParse(s, out dateTimeTmp))
                        result = (T)Convert.ChangeType(dateTimeTmp, typeof(T));
                    else
                        result = defaultValue;
                    break;
            }

            return result;
        }
        #endregion T MyConvert<T>(string s, T defaultValue)

        #region static DataTable CreateResultTable(string tableName)
        public static DataTable CreateResultTable(string tableName = "resultTable")
        {
            DataTable result = new DataTable(tableName);
            DataColumn dc = new DataColumn("result");
            dc.DataType = typeof(string);
            result.Columns.Add(dc);

            return result;
        }
		#endregion static DataTable CreateResultTable(string tableName)

		#region string GetUserIpAddress()
		public string GetUserIpAddress()
		{
			// 使用HttpContext来获取用户IP地址
			string ipAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

			if (string.IsNullOrEmpty(ipAddress))
			{
				ipAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
			}

			return ipAddress.Split(':')[0];
		}
		#endregion string GetUserIpAddress()

		#region public bool CustomAuthorizationFilter(HttpContext context, string controllerName = "", string actionName = "")
		public bool CustomAuthorizationFilter(HttpContext context, string controllerName = "", string actionName = "")
		{
            string fName = "Globals.cs CustomAuthorizationFilter";
			try
			{
				// 在這裡執行您的驗證邏輯
				string result = IsOK(context);
                WriteLog.Log($"[{fName}] result = {result}, controllerName = {controllerName}, actionName = {actionName}");

				if ("超時|檢查不通過".Split('|').Contains(result))
				{
					context.Response.StatusCode = 401; // 401 表示未经授权
					context.Response.ContentType = "text/plain";
					context.Response.Write(result);
					return false; // Change to false to indicate failure
				}
				else if (controllerName == "Login" && actionName == "CheckSession")
				{
					context.Response.StatusCode = 200;
					context.Response.ContentType = "text/plain";
					context.Response.Write(result);
					return true; // Change as needed based on your logic
				}

				var jsonObj = JsonConvert.DeserializeObject<dynamic>(result);

				context.Items["LoginId"] = (string)jsonObj.a;


                // Login 那邊有加入，這邊應該直接取出不用再撈DB
                // 如果撈出來是 NULL 則直接判定檢查不通過
                UserDetail user = Globals._Cache.Get($"User_{(string)jsonObj.a}") as UserDetail;

                if (user == null || string.IsNullOrEmpty(user.UserID))
				{
					context.Response.StatusCode = 401; // 401 表示未经授权
					context.Response.ContentType = "text/plain";
					context.Response.Write("檢查不通過 ");

                    return false; // Change to false to indicate failure
				}
                context.Items["IsAdmin"] = user.IsAdmin;

                context.Response.Headers.Remove("Army");
				context.Response.Headers.Remove("ArmyC");
				context.Response.Headers.Remove("Armyc");
				context.Response.Headers.Add("Army", (string)jsonObj.c);
				context.Response.Headers.Add("ArmyC", (string)jsonObj.m);
				context.Response.Headers.Add("Armyc", (string)jsonObj.m);

				return true;
			}
			catch (Exception ex)
			{
				WriteLog.Log($"{fName} error", ex.ToString());

				context.Response.StatusCode = 401;
				context.Response.ContentType = "text/plain";
				context.Response.Write("驗證失敗");
				return false; // Change to false to indicate failure
			}
		}
		#endregion public bool CustomAuthorizationFilter(HttpContext context, string controllerName = "", string actionName = "")

		#region private string IsOK(HttpContext context)
		private string IsOK(HttpContext context)
		{
			string headerKey = "Army";
			string s = "";

			if (context.Request.Headers.AllKeys.Contains(headerKey))
			{
				s = context.Request.Headers[headerKey];
			}

			if (string.IsNullOrEmpty(s))
			{
				s = context.Request.Cookies.Get(headerKey).Value;
			}

			string c = "";

			headerKey = "Armyc";

			if (context.Request.Headers.AllKeys.Contains(headerKey))
			{
				c = context.Request.Headers[headerKey];
			}

			if (string.IsNullOrEmpty(c))
			{
				c = context.Request.Cookies.Get(headerKey).Value;
			}

			if (c.Split(',').Length == 2)
			{
				if (c.Split(',')[0].Trim() == c.Split(',')[1].Trim())
					c = c.Split(',')[0].Trim();
			}

			//string result = (new ArmyAPI.Controllers.LoginController()).CheckSession(context.Request.Form["c"], s);
			string result = "檢查不通過";
			if (!string.IsNullOrEmpty(c) && !string.IsNullOrEmpty(s))
				result = (new ArmyAPI.Controllers.LoginController()).CheckSession(c, s);
			//WriteLog.Log($"c = {c}, s = {s}");
			// 實現自定義的驗證邏輯
			// 返回true表示通過驗證，返回false表示未通過驗證
			return result;
		}
        #endregion private string IsOK(HttpContext context)

		#region 靜態方法

		#region Globals GetInstance()
		public static Globals GetInstance()
        {
            return new Globals();
        }
        #endregion Globals GetInstance()

        #region public static bool IsAdmin(string userId)
        public static bool IsAdmin(string userId)
        {
            var ugController = new UserGroupController();
            bool isAdmin = ugController.IsAdmin(userId);

            return isAdmin;
        }
		#endregion public static bool IsAdmin(string userId)



        #region public static bool ValidateCredentials(string domain, string username, string password)
        /// <summary>
        /// 驗証AD帳密
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool ValidateCredentials(string domain, string username, string password)
		{
            StringBuilder watchSb = new StringBuilder();
            watchSb.AppendLine($"開始 {DateTime.Now.ToString("HH:mm:ss")}");

            using (PrincipalContext context = new PrincipalContext(ContextType.Domain, domain))
			{
                watchSb.AppendLine($"結束 {DateTime.Now.ToString("HH:mm:ss")}");
                WriteLog.Log("Globals.cs ValidateCredentials", watchSb.ToString());
				// Validate the credentials
				return context.ValidateCredentials(username, password);
			}
		}
		#endregion public static bool ValidateCredentials(string domain, string username, string password)

		#region public static bool CheckUserExistence(string username)
		/// <summary>
		/// 檢查 AD 帳號存不存在
		/// </summary>
		/// <param name="username"></param>
		/// <returns></returns>
		public static bool CheckUserExistence(string username)
		{
            StringBuilder watchSb = new StringBuilder();
            watchSb.AppendLine($"開始 {DateTime.Now.ToString("HH:mm:ss")}");

			using (PrincipalContext context = new PrincipalContext(ContextType.Domain))
			{
				Task<bool> task = Task.Run(() =>
				{
					using (UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username))
					{
                        watchSb.AppendLine($"結束 {DateTime.Now.ToString("HH:mm:ss")}");
                        WriteLog.Log("Globals.cs CheckUserExistence", watchSb.ToString());
						return user != null;
					}
				});

				// 設定超時時間為5秒
				bool result = task.Wait(TimeSpan.FromSeconds(5));

				return result && task.Result;
			}
		}
		#endregion public static bool CheckUserExistence(string username)


		#endregion 靜態方法

		#endregion 方法/私有方法/靜態方法
	}

	public static class EnumExtensions
	{
		public static string GetDescription(this Enum value)
		{
			var flags = Enum.GetValues(value.GetType()).Cast<Enum>().Where(v => value.HasFlag(v) && Convert.ToUInt32(v) != 0);
			var descriptions = flags.Select(flag =>
			{
				var field = flag.GetType().GetField(flag.ToString());
				var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
				return attribute == null ? flag.ToString() : attribute.Description;
			});
			return string.Join(", ", descriptions);
		}

		public static bool HasFlagWithDescription<T>(this T value, string description) where T : Enum
		{
			var flags = Enum.GetValues(value.GetType()).Cast<T>().Where(v => value.HasFlag(v) && Convert.ToUInt32(v) != 0);
			return flags.Any(flag => flag.GetDescription() == description);

			// 怎麼得到 Enum 變數 含有 Description 是否有「xxx」]
			// public enum euTest {... }
			// var test = euTest.....;
			// bool containsOtherDescription = test.HasFlagWithDescription("xxx");
		}
	}
}
