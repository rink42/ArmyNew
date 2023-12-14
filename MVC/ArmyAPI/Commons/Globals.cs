using System;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using ArmyAPI.Controllers;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using System.Linq;
using System.Web.Mvc;
using System.Web;
using System.DirectoryServices.AccountManagement;
using ArmyAPI.Models;
using System.Web.Caching;
using System.Configuration;
using Org.BouncyCastle.Asn1.Ocsp;

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

        public bool CustomAuthorizationFilter(string controllerName, string actionName)
        {
			HttpContext context = HttpContext.Current;
			// 在這裡執行您的驗證邏輯
			//if (!IsAuthorized(filterContext))
			string result = IsOK(context);
            if ("超時|檢查不通過".Split('|').Contains(result))
            {
				//filterContext.Result = new HttpUnauthorizedResult(result);
				context.Response.StatusCode = 401; // 401 表示未经授权
                context.Response.ContentType = "text/plain";
                context.Response.Write(result);

                //WriteLog.Log($"controllerName = {controllerName}, actionName = {actionName}, = {}, = {},");

                return false;
            }
            else if (controllerName == "Login" && actionName == "CheckSession")
            {
				context.Response.StatusCode = 200;
                context.Response.ContentType = "text/plain";
                context.Response.Write(result);

                return true;
            }

            var jsonObj = JsonConvert.DeserializeObject<dynamic>(result);
            _LoginAcc = jsonObj.a;

            _LoginId = (string)jsonObj.a;
            _IsAdmin1 = (new ArmyAPI.Commons.BaseController())._DbUserGroup.IsAdmin((string)jsonObj.a);


            context.Response.Headers.Remove("Army");
            context.Response.Headers.Remove("ArmyC");
            context.Response.Headers.Remove("Armyc");
            context.Response.Headers.Add("Army", (string)jsonObj.c);
            context.Response.Headers.Add("ArmyC", (string)jsonObj.m);
            context.Response.Headers.Add("Armyc", (string)jsonObj.m);

            return true;
        }

        private string IsOK(HttpContext context)
        {
            string headerKey = "Army";
            string s = "";

            if (context.Request.Headers.AllKeys.Contains(headerKey))
                s = context.Request.Headers[headerKey];

            headerKey = "ArmyC";
            string c = "";

            if (context.Request.Headers.AllKeys.Contains(headerKey))
                c = context.Request.Headers[headerKey];

            headerKey = "Armyc";
            c = "";

            if (context.Request.Headers.AllKeys.Contains(headerKey))
                c = context.Request.Headers[headerKey];

            //string result = (new ArmyAPI.Controllers.LoginController()).CheckSession(filterContext.HttpContext.Request.Form["c"], s);
            string result = (new ArmyAPI.Controllers.LoginController()).CheckSession(c, s);

            // 實現自定義的驗證邏輯
            // 返回true表示通過驗證，返回false表示未通過驗證
            //return "超時|檢查不通過".Split('|').Contains(result) ? false : true; // 在此示例中，總是通過驗證
            return result;
        }



		#region public static bool ValidateCredentials(string domain, string username, string password)
		public static bool ValidateCredentials(string domain, string username, string password)
		{
			using (PrincipalContext context = new PrincipalContext(ContextType.Domain, domain))
			{
				// Validate the credentials
				return context.ValidateCredentials(username, password);
			}
		}
		#endregion public static bool ValidateCredentials(string domain, string username, string password)

		#region public static bool CheckUserExistence(string username)
		/// <summary>
		/// 檢查 AD 帳號存不存在(存在則回傳 Name)
		/// </summary>
		/// <param name="username"></param>
		/// <returns></returns>
		public static bool CheckUserExistence(string username)
		{
			using (PrincipalContext context = new PrincipalContext(ContextType.Domain))
			{
				using (UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username))
				{
                    return user != null;
				}
			}
		}
		#endregion public static bool CheckUserExistence(string username)

		#region public static object UseCache(string key, object value, CacheOperators op)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="op">1: add, 2: update, 3: delete, 4: Get </param>
		/// <returns></returns>
		public static object UseCache(string key, object value, CacheOperators op)
		{
			Cache cache = new Cache();
			object result = null;
            int cacheTime = int.Parse(ConfigurationManager.AppSettings.Get("CacheTime"));
			switch (op)
			{
				case CacheOperators.Add:
					// add
					cache.Insert(key, value, null, DateTime.Now.AddHours(cacheTime), TimeSpan.Zero);
					break;
				case CacheOperators.Update:
					// update
					cache.Remove(key);
					cache.Insert(key, value, null, DateTime.Now.AddHours(cacheTime), TimeSpan.Zero);
					break;
				case CacheOperators.Delete:
					// delete
					cache.Remove(key);
					break;
				case CacheOperators.Get:
					result = cache.Get(key);
					break;
			}

			return result;
		}
		#endregion public static object UseCache(string key, object value, CacheOperators op)

		#endregion 靜態方法

		#endregion 方法/私有方法/靜態方法
	}
}
