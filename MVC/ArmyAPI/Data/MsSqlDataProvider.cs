using System;
using System.Data;
using System.Data.SqlClient;

namespace ArmyAPI.Data
{
    #region class MsSqlDataProvider
    public partial class MsSqlDataProvider : IDisposable
    {
        #region 變數
        private String _ConnectionString = String.Empty;
        private DataTable _ResultDataTable = null;
        private DataSet _ResultDataSet = null;
        private Object _ResultObject = null;

        private bool _Disposed;

        private readonly byte _RetryCount = 5;

        private readonly int _SleepTime = 200;
        #endregion 變數

        #region Enums

        #region ReturnType : byte
        public enum ReturnType : byte
        {
            DateSet,
            DataTable,
            String,
            Long,
            ULong,
            Int,
            Uint,
            Short,
            UShort,
            Byte
        }
        #endregion ReturnType : byte

        #region ReturnTable1_CommonFields : byte
        public enum ReturnTable1_CommonFields : byte
        {
            PageIndex,
            PageSize,
            TotalCount,
            TotalPage
        }
        #endregion ReturnTable1_CommonFields : byte

        #endregion Enums

        #region 屬性

        #region String ConnectionString
        public String ConnectionString
        {
            set
            {
                _ConnectionString = value;
            }
        }
        #endregion String ConnectionString

        #region DataTable ResultDataTable
        public DataTable ResultDataTable
        {
            get
            {
                return _ResultDataTable;
            }
        }
        #endregion DataTable ResultDataTable

        #region DataSet ResultDataSet
        public DataSet ResultDataSet
        {
            get
            {
                return _ResultDataSet;
            }
        }
        #endregion DataSet ResultDataSet

        #region DataTableCollection ResultDataTables
        public DataTableCollection ResultDataTables
        {
            get
            {
                return ((_ResultDataSet != null && _ResultDataSet.Tables.Count > 0) ? _ResultDataSet.Tables : null);
            }
        }
        #endregion DataTableCollection ResultDataTables

        #region Object ResultObject
        public Object ResultObject
        {
            get
            {
                return _ResultObject;
            }
        }
        #endregion Object ResultObject

        #endregion 屬性

        #region 建構子
        public MsSqlDataProvider()
        {
        }
        #endregion 建構子

        #region 解構子
        ~MsSqlDataProvider()
        {
            Dispose(false);
        }
        #endregion 解構子

        #region 方法/私有方法

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
            if (_ResultDataTable != null)
            {
                _ResultDataTable.Clear();
                _ResultDataTable.Dispose();
                _ResultDataTable = null;
            }

            if (_ResultDataSet != null)
            {
                _ResultDataSet.Tables.Clear();
                _ResultDataSet.Clear();
                _ResultDataSet.Dispose();
                _ResultDataSet = null;
            }

            _ResultObject = null;

            ((IDisposable)this).Dispose();
        }
        #endregion Close()

        #region protected void GetDataReturnDataTable (string connectionString, string commandText, SqlParameter[] parameters)
        protected void GetDataReturnDataTable(string connectionString, string commandText, SqlParameter[] parameters)
        {
            using (SqlConnection sqlCn = new SqlConnection(connectionString))
            {
                sqlCn.Open();

                using (SqlCommand sqlCm = new SqlCommand(commandText, sqlCn))
                {
                    sqlCm.CommandTimeout = 600;

                    if (parameters != null)
                        sqlCm.Parameters.AddRange(parameters);

                    SqlDataAdapter da = new SqlDataAdapter(sqlCm);

                    byte retryCount = 0;

                    while (retryCount < _RetryCount)
                    {
                        try
                        {
                            if (_ResultDataTable != null)
                            {
                                _ResultDataTable.Clear();
                                _ResultDataTable.Dispose();
                                _ResultDataTable = null;
                            }
                            _ResultDataTable = new DataTable();

                            da.Fill(_ResultDataTable);

                            da.Dispose();
                            da = null;

                            break;
                        }
                        catch (SqlException sqlEx)
                        {
                            if (retryCount < _RetryCount)
                            {
                                System.Threading.Thread.Sleep(_SleepTime);
                                retryCount++;
                            }
                            else
                            {
                                throw new Exception(String.Format("MsSqlDataProvider GetDataReturnDataTable Error. {0}", sqlEx.ToString()));
                            }
                        }
                    }
                }

                sqlCn.Close();
            }
        }
        #endregion protected void GetDataReturnDataTable (string connectionString, string commandText, SqlParameter[] parameters)

        #region protected void GetDataReturnDataTable<T> (string connectionString, string commandText, SqlParameter[] parameters, ref T output)
        protected void GetDataReturnDataTable<T>(string connectionString, string commandText, SqlParameter[] parameters, ref T output)
        {
            using (SqlConnection sqlCn = new SqlConnection(connectionString))
            {
                sqlCn.Open();

                using (SqlCommand sqlCm = new SqlCommand(commandText, sqlCn))
                {
                    if (parameters != null)
                        sqlCm.Parameters.AddRange(parameters);

                    byte retryCount = 0;

                    SqlDataReader dr = null;

                    while (retryCount < _RetryCount)
                    {
                        try
                        {
                            if (_ResultDataTable != null)
                            {
                                _ResultDataTable.Clear();
                                _ResultDataTable.Dispose();
                                _ResultDataTable = null;
                            }
                            _ResultDataTable = new DataTable();

                            dr = sqlCm.ExecuteReader();

                            if (dr.Read())
                            {
                                output = (T)dr[0];
                            }

                            dr.NextResult();
                            _ResultDataTable.Load(dr);

                            dr.Close();
                            dr = null;

                            break;
                        }
                        catch (SqlException sqlEx)
                        {
                            if (retryCount == 0)
                                WriteError("MsSqlDataProvider GetDataReturnDataTable Error", ref sqlEx, ref commandText);

                            if (retryCount < _RetryCount)
                            {
                                System.Threading.Thread.Sleep(_SleepTime);
                                retryCount++;
                            }
                            else
                            {
                                throw new Exception(String.Format("MsSqlDataProvider GetDataReturnDataTable Error. {0}", sqlEx.ToString()));
                            }
                        }
                    }
                }

                sqlCn.Close();
            }
        }
        #endregion protected void GetDataReturnDataTable<T> (string connectionString, string commandText, SqlParameter[] parameters, ref T output)

        #region protected void GetDataReturnDataSet (string connectionString, string commandText, SqlParameter[] parameters)
        protected void GetDataReturnDataSet(string connectionString, string commandText, SqlParameter[] parameters)
        {
            using (SqlConnection sqlCn = new SqlConnection(connectionString))
            {
                sqlCn.Open();

                using (SqlCommand sqlCm = new SqlCommand(commandText, sqlCn))
                {
                    if (parameters != null)
                        sqlCm.Parameters.AddRange(parameters);

                    SqlDataAdapter da = new SqlDataAdapter(sqlCm);

                    byte retryCount = 0;

                    while (retryCount < _RetryCount)
                    {
                        try
                        {
                            if (_ResultDataSet != null)
                            {
                                _ResultDataSet.Tables.Clear();
                                _ResultDataSet.Clear();
                                _ResultDataSet.Dispose();
                                _ResultDataSet = null;
                            }
                            _ResultDataSet = new DataSet();

                            da.Fill(_ResultDataSet);

                            da.Dispose();
                            da = null;

                            break;
                        }
                        catch (SqlException sqlEx)
                        {
                            if (retryCount < _RetryCount)
                            {
                                System.Threading.Thread.Sleep(_SleepTime);
                                retryCount++;
                            }
                            else
                            {
                                WriteLog.Log(String.Format("MsSqlDataProvider GetDataReturnDataSet Error. {0}", sqlEx.ToString()));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (retryCount < _RetryCount)
                            {
                                System.Threading.Thread.Sleep(_SleepTime);
                                retryCount++;
                            }
                            else
                            {
                                WriteLog.Log(String.Format("MsSqlDataProvider GetDataReturnDataSet Error. {0}", ex.ToString()));
                            }
                        }
                    }
                }

                sqlCn.Close();
            }
        }
        #endregion protected void GetDataReturnDataSet (string connectionString, string commandText, SqlParameter[] parameters)

        #region protected int InsertUpdateDeleteData (string connectionString, string commandText, SqlParameter[] parameters)
        protected int InsertUpdateDeleteData(string connectionString, string commandText, SqlParameter[] parameters)
        {
            int result = -1;

            using (SqlConnection sqlCn = new SqlConnection(connectionString))
            {
                sqlCn.Open();

                using (SqlCommand sqlCm = new SqlCommand(commandText, sqlCn))
                {
                    if (parameters != null)
                        sqlCm.Parameters.AddRange(parameters);

                    SqlDataAdapter da = new SqlDataAdapter(sqlCm);

                    byte retryCount = 0;

                    while (retryCount < _RetryCount)
                    {
                        try
                        {
                            result = sqlCm.ExecuteNonQuery();

                            break;
                        }
                        catch (Exception ex)
                        {
                            if (retryCount < _RetryCount)
                            {
                                System.Threading.Thread.Sleep(_SleepTime);
                                retryCount++;
                            }
                            else
                            {
                                WriteLog.Log<string>(String.Format("MsSqlDataProvider InsertUpdateDeleteData Error. {0}", ex.ToString()));
                                throw new Exception(String.Format("MsSqlDataProvider InsertUpdateDeleteData Error. {0}", ex.ToString()));
                            }
                        }
                    }
                }

                sqlCn.Close();
            }

            return result;
        }
        #endregion protected int InsertUpdateDeleteData (string connectionString, string commandText, SqlParameter[] parameters)

        #region protected void InsertUpdateDeleteDataThenSelectData (string connectionString, string commandText, SqlParameter[] parameters, ReturnType returnType)
        protected void InsertUpdateDeleteDataThenSelectData(string connectionString, string commandText, SqlParameter[] parameters, ReturnType returnType)
        {
            using (SqlConnection sqlCn = new SqlConnection(connectionString))
            {
                sqlCn.Open();

                using (SqlCommand sqlCm = new SqlCommand(commandText, sqlCn))
                {
                    if (parameters != null)
                        sqlCm.Parameters.AddRange(parameters);

                    SqlDataAdapter da = new SqlDataAdapter(sqlCm);

                    byte retryCount = 0;
                    while (retryCount < _RetryCount)
                    {
                        try
                        {
                            if (returnType == ReturnType.DateSet)
                            {
                                if (_ResultDataSet != null)
                                {
                                    _ResultDataSet.Tables.Clear();
                                    _ResultDataSet.Clear();
                                    _ResultDataSet.Dispose();
                                    _ResultDataSet = null;
                                }
                                _ResultDataSet = new DataSet();

                                da.Fill(_ResultDataSet);
                            }
                            else
                            {
                                if (_ResultDataTable != null)
                                {
                                    _ResultDataTable.Clear();
                                    _ResultDataTable.Dispose();
                                    _ResultDataTable = null;
                                }
                                _ResultDataTable = new DataTable();

                                da.Fill(_ResultDataTable);

                                if (_ResultDataTable.Rows.Count == 1 && returnType != ReturnType.DataTable)
                                {
                                    _ResultObject = _ResultDataTable.Rows[0][0].ToString();
                                    _ResultDataTable.Clear();
                                    _ResultDataTable = null;
                                }
                            }

                            da.Dispose();
                            da = null;

                            break;
                        }
                        catch (Exception ex)
                        {
                            if (retryCount < _RetryCount)
                            {
                                System.Threading.Thread.Sleep(_SleepTime);
                                retryCount++;
                            }
                            else
                            {
                                throw new Exception(String.Format("MsSqlDataProvider InsertUpdateDeleteDataThenSelectData Error. {0}", ex.ToString()));
                            }
                        }
                    }
                }

                sqlCn.Close();
            }
        }
        #endregion protected void InsertUpdateDeleteDataThenSelectData (string connectionString, string commandText, SqlParameter[] parameters, ReturnType returnType)

        #region protected void GetDataReturnObject (string connectionString, string commandText, SqlParameter[] parameters)
        protected void GetDataReturnObject(string connectionString, string commandText, SqlParameter[] parameters)
        {
            using (SqlConnection sqlCn = new SqlConnection(connectionString))
            {
                sqlCn.Open();

                using (SqlCommand sqlCm = new SqlCommand(commandText, sqlCn))
                {
                    sqlCm.CommandTimeout = 600;

                    if (parameters != null)
                        sqlCm.Parameters.AddRange(parameters);

                    byte retryCount = 0;

                    while (retryCount < _RetryCount)
                    {
                        try
                        {
                            SqlDataReader dr = sqlCm.ExecuteReader();
                            dr.Read();

                            if (dr.HasRows)
                                _ResultObject = dr[0];

                            dr.Close();
                            dr = null;

                            break;
                        }
                        catch (SqlException sqlEx)
                        {
                            if (retryCount < _RetryCount)
                            {
                                System.Threading.Thread.Sleep(_SleepTime);
                                retryCount++;
                            }
                            else
                            {
                                throw new Exception(String.Format("MsSqlDataProvider GetDataReturnObject Error. {0}", sqlEx.ToString()));
                            }
                        }
                    }
                }

                sqlCn.Close();
            }
        }
        #endregion protected void GetDataReturnObject (string connectionString, string commandText, SqlParameter[] parameters)

        #region protected void GetDataReturnDataTable (string connectionString, CommandType commandType, string commandText, SqlParameter[] parameters)
        protected void GetDataReturnDataTable(string connectionString, CommandType commandType, string commandText, SqlParameter[] parameters)
        {
            using (SqlConnection sqlCn = new SqlConnection(connectionString))
            {
                sqlCn.Open();

                using (SqlCommand sqlCm = new SqlCommand(commandText, sqlCn))
                {
                    sqlCm.CommandType = commandType;

                    if (parameters != null)
                        sqlCm.Parameters.AddRange(parameters);

                    SqlDataAdapter da = new SqlDataAdapter(sqlCm);

                    byte retryCount = 0;

                    while (retryCount < _RetryCount)
                    {
                        try
                        {
                            if (_ResultDataTable != null)
                            {
                                _ResultDataTable.Clear();
                                _ResultDataTable.Dispose();
                                _ResultDataTable = null;
                            }
                            _ResultDataTable = new DataTable();

                            da.Fill(_ResultDataTable);

                            da.Dispose();
                            da = null;

                            break;
                        }
                        catch (SqlException sqlEx)
                        {
                            if (retryCount < _RetryCount)
                            {
                                System.Threading.Thread.Sleep(_SleepTime);
                                retryCount++;
                            }
                            else
                            {
                                WriteError("MsSqlDataProvider GetDataReturnDataTable Error.", ref sqlEx, ref commandText);
                                throw new Exception(String.Format("MsSqlDataProvider GetDataReturnDataTable Error. {0}", sqlEx.ToString()));
                            }
                        }
                    }
                }

                sqlCn.Close();
            }
        }
        #endregion protected void GetDataReturnDataTable (string connectionString, CommandType commandType string commandText, SqlParameter[] parameters)

        #region protected void GetDataReturnDataTable<T> (string connectionString, CommandType commandType, string commandText, SqlParameter[] parameters, ref T output)
        protected void GetDataReturnDataTable<T>(string connectionString, CommandType commandType, string commandText, SqlParameter[] parameters, ref T output)
        {
            using (SqlConnection sqlCn = new SqlConnection(connectionString))
            {
                sqlCn.Open();

                using (SqlCommand sqlCm = new SqlCommand(commandText, sqlCn))
                {
                    sqlCm.CommandType = commandType;

                    if (parameters != null)
                        sqlCm.Parameters.AddRange(parameters);

                    byte retryCount = 0;

                    SqlDataReader dr = null;

                    while (retryCount < _RetryCount)
                    {
                        try
                        {
                            if (_ResultDataTable != null)
                            {
                                _ResultDataTable.Clear();
                                _ResultDataTable.Dispose();
                                _ResultDataTable = null;
                            }
                            _ResultDataTable = new DataTable();

                            dr = sqlCm.ExecuteReader();

                            if (dr.Read())
                            {
                                output = (T)dr[0];
                            }

                            dr.NextResult();
                            _ResultDataTable.Load(dr);

                            dr.Close();
                            dr = null;

                            break;
                        }
                        catch (SqlException sqlEx)
                        {
                            if (retryCount == 0)
                                WriteError("MsSqlDataProvider GetDataReturnDataTable Error", ref sqlEx, ref commandText);

                            if (retryCount < _RetryCount)
                            {
                                System.Threading.Thread.Sleep(_SleepTime);
                                retryCount++;
                            }
                            else
                            {
                                throw new Exception(String.Format("MsSqlDataProvider GetDataReturnDataTable Error. {0}", sqlEx.ToString()));
                            }
                        }
                    }
                }

                sqlCn.Close();
            }
        }
        #endregion protected void GetDataReturnDataTable<T> (string connectionString, CommandType commandType, string commandText, SqlParameter[] parameters, ref T output)

        #region protected void GetDataReturnDataSet (string connectionString, CommandType commandType, string commandText, SqlParameter[] parameters)
        protected void GetDataReturnDataSet(string connectionString, CommandType commandType, string commandText, SqlParameter[] parameters)
        {
            using (SqlConnection sqlCn = new SqlConnection(connectionString))
            {
                sqlCn.Open();

                using (SqlCommand sqlCm = new SqlCommand(commandText, sqlCn))
                {
                    sqlCm.CommandType = commandType;

                    if (parameters != null)
                        sqlCm.Parameters.AddRange(parameters);

                    SqlDataAdapter da = new SqlDataAdapter(sqlCm);

                    byte retryCount = 0;

                    while (retryCount < _RetryCount)
                    {
                        try
                        {
                            if (_ResultDataSet != null)
                            {
                                _ResultDataSet.Tables.Clear();
                                _ResultDataSet.Clear();
                                _ResultDataSet.Dispose();
                                _ResultDataSet = null;
                            }
                            _ResultDataSet = new DataSet();

                            da.Fill(_ResultDataSet);

                            da.Dispose();
                            da = null;

                            break;
                        }
                        catch (SqlException sqlEx)
                        {
                            if (retryCount < _RetryCount)
                            {
                                System.Threading.Thread.Sleep(_SleepTime);
                                retryCount++;
                            }
                            else
                            {
                                WriteError("MsSqlDataProvider GetDataReturnDataSet Error.", ref sqlEx, ref commandText);
                                throw new Exception(String.Format("MsSqlDataProvider GetDataReturnDataSet Error. {0}", sqlEx.ToString()));
                            }
                        }
                    }
                }

                sqlCn.Close();
            }
        }
        #endregion protected void GetDataReturnDataSet (string connectionString, CommandType commandType, string commandText, SqlParameter[] parameters)

        #region protected int InsertUpdateDeleteData (string connectionString, CommandType commandType, string commandText, SqlParameter[] parameters)
        protected int InsertUpdateDeleteData(string connectionString, CommandType commandType, string commandText, SqlParameter[] parameters)
        {
            int result = -1;

            using (SqlConnection sqlCn = new SqlConnection(connectionString))
            {
                sqlCn.Open();

                using (SqlCommand sqlCm = new SqlCommand(commandText, sqlCn))
                {
                    sqlCm.CommandType = commandType;

                    if (parameters != null)
                        sqlCm.Parameters.AddRange(parameters);

                    byte retryCount = 0;

                    while (retryCount < _RetryCount)
                    {
                        try
                        {
                            result = sqlCm.ExecuteNonQuery();

                            break;
                        }
                        catch (SqlException sqlEx)
                        {
                            if (retryCount < _RetryCount)
                            {
                                System.Threading.Thread.Sleep(_SleepTime);
                                retryCount++;
                            }
                            else
                            {
                                WriteError("MsSqlDataProvider InsertUpdateDeleteData Error.", ref sqlEx, ref commandText);
                                throw new Exception(String.Format("MsSqlDataProvider InsertUpdateDeleteData Error. {0}", sqlEx.ToString()));
                            }
                        }
                    }
                }

                sqlCn.Close();
            }

            return result;
        }
        #endregion protected int InsertUpdateDeleteData (string connectionString, CommandType commandType, string commandText, SqlParameter[] parameters)

        #region protected void InsertUpdateDeleteDataThenSelectData (string connectionString, CommandType commandType, string commandText, SqlParameter[] parameters, ReturnType returnType)
        protected void InsertUpdateDeleteDataThenSelectData(string connectionString, CommandType commandType, string commandText, SqlParameter[] parameters, ReturnType returnType)
        {
            using (SqlConnection sqlCn = new SqlConnection(connectionString))
            {
                sqlCn.Open();

                using (SqlCommand sqlCm = new SqlCommand(commandText, sqlCn))
                {
                    sqlCm.CommandType = commandType;

                    if (parameters != null)
                        sqlCm.Parameters.AddRange(parameters);

                    SqlDataAdapter da = new SqlDataAdapter(sqlCm);

                    byte retryCount = 0;
                    while (retryCount < _RetryCount)
                    {
                        try
                        {
                            if (returnType == ReturnType.DateSet)
                            {
                                if (_ResultDataSet == null)
                                    _ResultDataSet = new DataSet();
                                da.Fill(_ResultDataSet);
                            }
                            else
                            {
                                if (_ResultDataTable != null)
                                {
                                    _ResultDataTable.Clear();
                                    _ResultDataTable.Dispose();
                                    _ResultDataTable = null;
                                }
                                _ResultDataTable = new DataTable();

                                da.Fill(_ResultDataTable);

                                if (_ResultDataTable.Rows.Count == 1 && returnType != ReturnType.DataTable)
                                {
                                    _ResultObject = _ResultDataTable.Rows[0][0].ToString();
                                    _ResultDataTable.Clear();
                                    _ResultDataTable = null;
                                }
                            }

                            da.Dispose();
                            da = null;

                            break;
                        }
                        catch (SqlException sqlEx)
                        {
                            if (retryCount < _RetryCount)
                            {
                                System.Threading.Thread.Sleep(_SleepTime);
                                retryCount++;
                            }
                            else
                            {
                                WriteError("MsSqlDataProvider InsertUpdateDeleteDataThenSelectData Error.", ref sqlEx, ref commandText);
                                throw new Exception(String.Format("MsSqlDataProvider InsertUpdateDeleteDataThenSelectData Error. {0}", sqlEx.ToString()));
                            }
                        }
                    }
                }
            }
        }
        #endregion protected void InsertUpdateDeleteDataThenSelectData (string connectionString, CommandType commandType, string commandText, SqlParameter[] parameters, ReturnType returnType)

        #region protected void GetDataReturnObject (string connectionString, CommandType commandType, string commandText, SqlParameter[] parameters)
        protected void GetDataReturnObject(string connectionString, CommandType commandType, string commandText, SqlParameter[] parameters)
        {
            using (SqlConnection sqlCn = new SqlConnection(connectionString))
            {
                sqlCn.Open();

                using (SqlCommand sqlCm = new SqlCommand(commandText, sqlCn))
                {
                    sqlCm.CommandType = commandType;

                    if (parameters != null)
                        sqlCm.Parameters.AddRange(parameters);

                    byte retryCount = 0;

                    while (retryCount < _RetryCount)
                    {
                        try
                        {
                            SqlDataReader dr = sqlCm.ExecuteReader();
                            dr.Read();

                            if (dr.HasRows)
                                _ResultObject = dr[0];

                            dr.Close();
                            dr = null;

                            break;
                        }
                        catch (SqlException sqlEx)
                        {
                            if (retryCount < _RetryCount)
                            {
                                System.Threading.Thread.Sleep(_SleepTime);
                                retryCount++;
                            }
                            else
                            {
                                WriteError("MsSqlDataProvider GetDataReturnObject Error.", ref sqlEx, ref commandText);
                                throw new Exception(String.Format("MsSqlDataProvider GetDataReturnObject Error. {0}", sqlEx.ToString()));
                            }
                        }
                    }
                }

                sqlCn.Close();
            }
        }
        #endregion protected void GetDataReturnObject (string connectionString, CommandType commandType, string commandText, SqlParameter[] parameters)

        #region void WriteError (string errorKey, ref SqlException sqlEx, ref String commandText)
        private void WriteError(string errorKey, ref SqlException sqlEx, ref String commandText)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            //sb.AppendLine(String.Format("ip = {0}, path = {1}", System.Web.HttpContext.Current.Request.UserHostAddress, System.Web.HttpContext.Current.Request.Path));
            //sb.AppendLine(String.Format("httpContext.Request.ApplicationPath ={0}", System.Web.HttpContext.Current.Request.ApplicationPath));
            //sb.AppendLine(String.Format("httpContext.Request.Url.AbsoluteUri = {0}", System.Web.HttpContext.Current.Request.Url.AbsoluteUri));
            //sb.AppendLine(String.Format("httpContext.Request.Url.AbsolutePath = {0}", System.Web.HttpContext.Current.Request.Url.AbsolutePath));
            sb.AppendLine(String.Format("{0}. {1} = ", errorKey, sqlEx.ToString()));
            sb.AppendLine(String.Format("commandText = {0}", commandText));
            WriteLog.Log<String>(sb.ToString());
        }
        #endregion void WriteError (string errorKey, ref SqlException sqlEx, ref String commandText)

        #region 私有方法

        #region static string GetFields<T> (T fields)
        private static string GetFields<T>(T fields)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            foreach (T f in Enum.GetValues(typeof(T)))
            {
                bool descIsExist = false;
                if ((ulong.Parse(Convert.ChangeType(f, typeof(ulong)).ToString()) & ulong.Parse(Convert.ChangeType(fields, typeof(ulong)).ToString())) > 0)
                {
                    if (sb.Length > 0)
                        sb.Append(", ");

                    string desc = ArmyAPI.Commons.Globals.GetInstance().GetEnumDesc<T>(f, out descIsExist);

                    sb.Append(String.Format(((descIsExist) ? "{0}" : "[{0}]"), desc));
                }
            }

            return sb.ToString();
        }
        #endregion static string GetFields<T> (T fields)

        #region static string CheckSQL (string sqlcmd)
        private static string CheckSQL(string sqlcmd)
        {
            sqlcmd = sqlcmd.Replace(Environment.NewLine, String.Empty);
            sqlcmd = sqlcmd.Replace("\0", String.Empty);
            sqlcmd = sqlcmd.Replace("\t", String.Empty);
            sqlcmd = sqlcmd.Replace("\v", String.Empty);

            string[] chk = { /*0*/"xp_cmdshell", /*1*/"script", /*2*/"iframe", /*3*/"%", /*4*/"cast ",
                            /*5*/"exec ", /*6*/"--", /*7*/"/*", /*8*/"@@", /*9*/"select ",
                            /*10*/"insert ", /*11*/"update ", /*12*/"delete ", /*13*/"create ", /*14*/"truncate ",
                            /*15*/"declare ", /*16*/"drop ", /*17*/"grant ", /*18*/"case ", /*19*/"'sa'",
                            /*20*/" or ", /*21*/" sa ", /*22*/"char(", /*23*/"<", /*24*/">",
                            /*25*/"expression"};

            for (int i = 0; i <= chk.Length - 1; i++)
            {
                try
                {
                    if (System.Web.HttpUtility.UrlDecode(sqlcmd).IndexOf(chk[i], StringComparison.InvariantCultureIgnoreCase) > 0 || System.Web.HttpUtility.HtmlDecode(sqlcmd).IndexOf(chk[i], StringComparison.InvariantCultureIgnoreCase) > 0)
                    {
                        return String.Empty;
                    }
                }
                catch //(Exception ex)
                {
                    return String.Empty;
                }
            }

            return sqlcmd;
        }
        #endregion static string CheckSQL (string sqlcmd)

        #endregion 私有方法

        #endregion 方法/私有方法
    }
    #endregion class MsSqlDataProvider
}
