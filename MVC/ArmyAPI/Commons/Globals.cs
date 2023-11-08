using System;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using ArmyAPI.Controllers;
using System.Web.UI.WebControls;

namespace ArmyAPI.Commons
{
	public class Globals : IDisposable
    {
        #region Enum


        #endregion Enum

        #region 變數

        protected bool _Disposed = false;

        #endregion 變數

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

        #region String GetEnumDesc (Enum e)
        public String GetEnumDesc(Enum e)
        {
            FieldInfo EnumInfo = e.GetType().GetField(e.ToString());
            DescriptionAttribute[] EnumAttributes = (DescriptionAttribute[])EnumInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (EnumAttributes.Length > 0)
                return EnumAttributes[0].Description;

            return e.ToString();
        }
        #endregion String GetEnumDesc (Enum e)

        #region String GetEnumDesc<T> (T e, out bool descIsExist)
        public String GetEnumDesc<T>(T e, out bool descIsExist)
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
        #endregion String GetEnumDesc<T> (T e, out bool descIsExist)

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

        #endregion 靜態方法

        #endregion 方法/私有方法/靜態方法
    }
}
