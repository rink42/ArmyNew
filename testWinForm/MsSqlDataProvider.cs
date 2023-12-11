using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Dapper;

namespace ArmyAPI.Data
{
    #region class MsSqlDataProvider
    public partial class MsSqlDataProvider : IDisposable
    {
        #region 變數
        private String _ConnectionString = String.Empty;
        private String _TableName = String.Empty;
		private DataTable _ResultDataTable = null;
        private DataSet _ResultDataSet = null;
        private Object _ResultObject = null;

        private static SqlTransaction _Transaction = null;

        private bool _Disposed;

        public static Dictionary<int, Object> ManageCollection = new Dictionary<int, Object>();

        private Type _DerivedType;
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

        #region TransactionOperateType : byte
        public enum TransactionOperateType : byte
        {
            Start,
            Commit,
            Rollback
        }
        #endregion TransactionOperateType : byte

        #endregion Enums

        #region 屬性

        #region string ConnectionString
        public string ConnectionString
        {
            set
            {
                _ConnectionString = value;
            }
            get
            {
                return _ConnectionString;
            }
        }
        #endregion string ConnectionString

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

        public MsSqlDataProvider(string connectionString, Type derivedType)
        {
            ConnectionString = connectionString;
            _DerivedType = derivedType;
        }
        #endregion 建構子

        #region 解構子
        ~MsSqlDataProvider()
        {
            if (ManageCollection != null && ManageCollection.Count > 0)
            {
                var keysToRemove = new List<int>();
                foreach (var key in ManageCollection.Keys)
                {
                    keysToRemove.Add(key);
                }

                for (int i = ManageCollection.Count - 1; i >= 0; i--)
                {
                    int key = keysToRemove[i];

                    SqlConnection connection = (SqlConnection)ManageCollection[key] as SqlConnection;

                    if (connection != null)
                    {
                        connection.Close();
                        //connection.Dispose();
                    }

                    // 从 Dictionary 中移除键值对
                    ManageCollection.Remove(key);
                }
            }
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

        #region private void CheckArgs(ref string connectionString, ref string commandText)
        private void CheckArgs(ref string connectionString, ref string commandText)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException($"'{nameof(connectionString)}' 不可為 Null 或空白。", nameof(connectionString));
            }

            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException($"'{nameof(commandText)}' 不可為 Null 或空白。", nameof(commandText));
            }
        }
        #endregion private void CheckArgs(ref string connectionString, ref string commandText)

        #region protected DataTable GetDataTable (string connectionString, string commandText, SqlParameter[] parameters)
        public DataTable GetDataTable(string connectionString, string commandText, SqlParameter[] parameters)
        {
            DataSet ds = GetDataSet(connectionString, commandText, parameters);
            if (ds != null && ds.Tables.Count > 0)
                return ds.Tables[0];

            return null;
        }
		#endregion protected DataTable GetDataTable (string connectionString, string commandText, SqlParameter[] parameters)

		#region public void GetDataReturnDataTable (string connectionString, string commandText, SqlParameter[] parameters)
		public void GetDataReturnDataTable(string connectionString, string commandText, SqlParameter[] parameters)
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
                    }
                    catch (SqlException sqlEx)
                    {
                        throw new Exception(String.Format("MsSqlDataProvider GetDataReturnDataTable Error. {0}", sqlEx.ToString()));
                    }
                }

                sqlCn.Close();
            }
        }
		#endregion public void GetDataReturnDataTable (string connectionString, string commandText, SqlParameter[] parameters)

		#region protected DataSet GetDataSet (string connectionString, string commandText, SqlParameter[] parameters)
		public DataSet GetDataSet(string connectionString, string commandText, SqlParameter[] parameters)
        {
            CheckArgs(ref connectionString, ref commandText);

            using (SqlConnection connection = GetConnection(connectionString, true))
            {
                using (SqlCommand sqlCm = new SqlCommand(commandText, connection))
                {
                    if (parameters != null)
                        sqlCm.Parameters.AddRange(parameters);

                    DataSet ds = new DataSet();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCm))
                    {
                        adapter.Fill(ds);
                    }

                    return ds;
                }
            }
        }
		#endregion protected DataSet GetDataSet (string connectionString, string commandText, SqlParameter[] parameters)

		#region List<T> Get<T>(string connectionString, string commandText, SqlParameter[] parameters) where T : new()
		public List<T> Get<T>(string connectionString, string commandText, SqlParameter[] parameters) where T : new()
        {
            List<T> result = new List<T>();

            SqlConnection connection = GetConnection(connectionString, true);

            using (SqlCommand sqlCm = new SqlCommand(commandText, connection))
            {
                if (parameters != null)
                    sqlCm.Parameters.AddRange(parameters);

                using (SqlDataReader reader = sqlCm.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        T row = new T();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var propertyName = reader.GetName(i);
                            var property = typeof(T).GetProperty(propertyName);

                            if (property != null && !reader.IsDBNull(i))
                            {
                                property.SetValue(row, reader.GetValue(i));
                            }
                        }

                        result.Add(row);
                    }
                }
            }

            return result;
        }
		#endregion List<T> Get<T>(string connectionString, string commandText, SqlParameter[] parameters) where T : new()

		#region List<T> Get1<T>(string connectionString, string commandText, object parameters) where T : new()
		public List<T> Get1<T>(string connectionString, string commandText, object parameters) where T : new()
        {
            List<T> result = new List<T>();

            using (SqlConnection connection = GetConnection(connectionString, true))
            {
				result = connection.Query<T>(commandText, parameters).ToList();
			}

            return result;
        }
		#endregion List<T> Get1<T>(string connectionString, string commandText, object parameters) where T : new()

		#region public T GetOne<T>(string connectionString, string commandText, SqlParameter[] parameters) where T : new()
		public T GetOne<T>(string connectionString, string commandText, SqlParameter[] parameters) where T : new()
        {
            SqlConnection connection = GetConnection(connectionString, true);

            using (SqlCommand sqlCm = new SqlCommand(commandText, connection))
            {
                if (parameters != null)
                    sqlCm.Parameters.AddRange(parameters);

                using (SqlDataReader reader = sqlCm.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        T row = new T();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var propertyName = reader.GetName(i);
                            var property = typeof(T).GetProperty(propertyName);

                            if (property != null && !reader.IsDBNull(i))
                            {
                                if (property.PropertyType == typeof(bool))
                                {
                                    // Convert the 1 or 0 to a boolean
                                    var value = reader.GetValue(i);
                                    bool b = (int)value == 1;
                                    property.SetValue(row, b);
                                }
                                else
                                {
                                    property.SetValue(row, reader.GetValue(i));
                                }
                            }
                        }

                        return row;
                    }
                    else
                    {
                        // If no records were found, return a default instance of T (null)
                        return default(T);
                    }
                }
            }
        }
		#endregion public T GetOne<T>(string connectionString, string commandText, SqlParameter[] parameters) where T : new()

		#region T GetOne1<T>(string connectionString, string commandText, object parameters = null) where T : new()
		public T GetOne1<T>(string connectionString, string commandText, object parameters = null) where T : new()
		{
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();

				// Dapper Query method automatically handles opening and closing connections
				return connection.Query<T>(commandText, parameters).SingleOrDefault();
			}   
		}
		#endregion T GetOne1<T>(string connectionString, string commandText, object parameters = null) where T : new()

		#region public int InsertUpdateDeleteData (string connectionString, string commandText, SqlParameter[] parameters, bool isIsolation = false)
		public int InsertUpdateDeleteData(string connectionString, string commandText, SqlParameter[] parameters, bool isIsolation = false)
        {
            CheckArgs(ref connectionString, ref commandText);

            SqlConnection connection = GetConnection(connectionString, isIsolation);

            using (SqlCommand sqlCm = new SqlCommand(commandText, connection))
            {
                if (parameters != null)
                    sqlCm.Parameters.AddRange(parameters);

                if (!isIsolation && _Transaction == null)
                    _Transaction = connection.BeginTransaction();

                try
                {
                    // 執行 INSERT、UPDATE 或 DELETE 操作，並獲取受影響的資料數
                    int rowsAffected = sqlCm.ExecuteNonQuery();

                    return rowsAffected;
                }
                catch (Exception ex)
                {
                    if (!isIsolation)
                        TransactionOperate(TransactionOperateType.Rollback);

                    StringBuilder sb = new StringBuilder();
                    if (parameters != null)
                    {
                        foreach (SqlParameter parameter in parameters)
                        {
                            int index = Array.IndexOf(parameters, parameter);
                            sb.AppendLine($"    parameter[{index}].ParameterName = {parameter.ParameterName}");
                            sb.AppendLine($"    parameter[{index}].DbType = {parameter.DbType}");
                            sb.AppendLine($"    parameter[{index}].Value = {parameter.Value}\n");
                        }
                    }

                    throw new Exception($"MsSqlDataProvider.cs / InsertUpdateDeleteData 失敗,\n commandText = {commandText},\n parameters = {sb},\n ex = {ex}");
                }
            }
        }
		#endregion public int InsertUpdateDeleteData (string connectionString, string commandText, SqlParameter[] parameters, bool isIsolation = false)

		#region protected int Dapper_InsertUpdateDeleteData (string connectionString, string commandText, object parameters)
		protected int Dapper_InsertUpdateDeleteData(string connectionString, string commandText, object parameters)
        {
            CheckArgs(ref connectionString, ref commandText);

            int result = 0;
            using (SqlConnection connection = GetConnection(connectionString, true))
            {
				try
                {
					result = connection.Execute(commandText, parameters);
				}
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return result;
        }
		#endregion protected int Dapper_InsertUpdateDeleteData (string connectionString, string commandText, object parameters)

		#region protected DB_UpdaetMultiDatas_Msg InsertUpdateDeleteDatas(string connectionString, string commandText, List<SqlParameter[]> parameters)
		protected DB_UpdaetMultiDatas_Msg UpdateMultiDatas(string connectionString, string commandText, List<SqlParameter[]> parameters)
        {
			CheckArgs(ref connectionString, ref commandText);

			SqlConnection connection = GetConnection(connectionString, true);

            DB_UpdaetMultiDatas_Msg returnMsg = new DB_UpdaetMultiDatas_Msg();
            if (returnMsg.Fails == null) returnMsg.Fails = new List<int>();

			using (SqlCommand sqlCm = new SqlCommand(commandText, connection))
            {
                foreach (var sp in parameters)
                {
                    sqlCm.Parameters.Clear();

                    if (sp != null)
                        sqlCm.Parameters.AddRange(sp);

                    try
                    {
                        // 執行 INSERT、UPDATE 或 DELETE 操作，並獲取受影響的資料數
                        int rowsAffected = sqlCm.ExecuteNonQuery();

                        if (rowsAffected == 0)
							returnMsg.Fails.Add((int)sp[0].Value);
					}
					catch (Exception ex)
                    {
                        StringBuilder sb = new StringBuilder();
                        if (parameters != null)
                        {
                            foreach (SqlParameter parameter in sp)
                            {
                                int index = Array.IndexOf(sp, parameter);
                                sb.AppendLine($"    parameter[{index}].ParameterName = {parameter.ParameterName}");
                                sb.AppendLine($"    parameter[{index}].DbType = {parameter.DbType}");
                                sb.AppendLine($"    parameter[{index}].Value = {parameter.Value}\n");
                            }
                        }

                        throw new Exception($"MsSqlDataProvider.cs / InsertUpdateDeleteData 失敗,\n commandText = {commandText},\n parameters = {sb},\n ex = {ex}");
                    }
                }
            }

            return returnMsg;
        }
		#endregion protected DB_UpdaetMultiDatas_Msg InsertUpdateDeleteDatas(ref SqlConnection connection, string commandText, List<SqlParameter[]> parameters)

		#region protected int InsertThenGetIdentityData (string connectionString, string commandText, SqlParameter[] parameters, bool isIsolation = false)
		protected int InsertThenGetIdentityData(string connectionString, string commandText, SqlParameter[] parameters, bool isIsolation = false)
        {
            CheckArgs(ref connectionString, ref commandText);

            SqlConnection connection = GetConnection(connectionString, isIsolation);

            using (SqlCommand sqlCm = new SqlCommand(commandText, connection))
            {
                if (parameters != null)
                    sqlCm.Parameters.AddRange(parameters);

                if (!isIsolation && _Transaction == null)
                    _Transaction = connection.BeginTransaction();

                try
                {
					// 执行插入操作，并获取新生成的 identity 值
					int newId = Convert.ToInt32(sqlCm.ExecuteScalar());

					return newId;
                }
                catch (Exception ex)
                {
                    if (!isIsolation)
                        TransactionOperate(TransactionOperateType.Rollback);

                    StringBuilder sb = new StringBuilder();
                    if (parameters != null)
                    {
                        foreach (SqlParameter parameter in parameters)
                        {
                            int index = Array.IndexOf(parameters, parameter);
                            sb.AppendLine($"    parameter[{index}].ParameterName = {parameter.ParameterName}");
                            sb.AppendLine($"    parameter[{index}].DbTyp e= {parameter.DbType}");
                            sb.AppendLine($"    parameter[{index}].Value = {parameter.Value}\n");
                        }
                    }

                    throw new Exception($"MsSqlDataProvider.cs / InsertThenGetIdentityData 失敗,\n commandText = {commandText},\n parameters = {sb},\n ex = {ex}");
                }
            }
        }
		#endregion protected int InsertThenGetIdentityData (string connectionString, string commandText, SqlParameter[] parameters, bool isIsolation = false)

		#region protected void InsertUpdateDeleteDataThenSelectData (string connectionString, string commandText, SqlParameter[] parameters, ReturnType returnType)
		protected void InsertUpdateDeleteDataThenSelectData(string connectionString, string commandText, SqlParameter[] parameters, ReturnType returnType, bool isIsolation = false)
		{
			CheckArgs(ref connectionString, ref commandText);

			SqlConnection connection = GetConnection(connectionString, isIsolation);

            using (SqlCommand sqlCm = new SqlCommand(commandText, connection))
            {
                if (parameters != null)
                    sqlCm.Parameters.AddRange(parameters);

                if (!isIsolation && _Transaction == null)
                    _Transaction = connection.BeginTransaction();

                SqlDataAdapter da = new SqlDataAdapter(sqlCm);

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
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("MsSqlDataProvider InsertUpdateDeleteDataThenSelectData Error. {0}", ex.ToString()));
                }
            }
		}
        #endregion protected void InsertUpdateDeleteDataThenSelectData (string connectionString, string commandText, SqlParameter[] parameters, ReturnType returnType)

        #region protected void InsertUpdateDeleteDataThenSelectData (string connectionString, string commandText, List<SqlParameter[]> parameterses)
        protected DataTable InsertUpdateDeleteDataThenSelectData(string connectionString, string commandText, List<SqlParameter[]> parameterses)
        {
            CheckArgs(ref connectionString, ref commandText);

            SqlConnection connection = GetConnection(connectionString, true);

            DataTable resultDt = new DataTable();
            DataColumn dc = new DataColumn("result");
            dc.DataType = typeof(string);
            resultDt.Columns.Add(dc);

            // 起始交易，如果有一筆失敗就 RollBack
            SqlTransaction st = connection.BeginTransaction();
            bool isFailed = false;
            using (SqlCommand sqlCm = new SqlCommand(commandText, connection, st))
            {
                foreach (var parameters in parameterses)
                {
                    sqlCm.Parameters.Clear();

                    if (parameters != null)
                        sqlCm.Parameters.AddRange(parameters);

                    SqlDataAdapter da = new SqlDataAdapter(sqlCm);

                    try
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        if (dt.Rows.Count == 1)
                        {
                            DataRow dr = resultDt.NewRow();
                            dr[0] = dt.Rows[0][0];

                            if (dt.Rows[0][0].ToString() == "-1")
                            {
                                st.Rollback();
                                isFailed = true;
                                resultDt.Rows.Clear();
                                DataRow dr1 = resultDt.NewRow();
                                dr1[0] = $"Index = {parameters[0].Value} 失敗";
                                resultDt.Rows.Add(dr1);

                                break;
                            }

                            resultDt.Rows.Add(dr);
                        }

                        da.Dispose();
                        da = null;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(String.Format("MsSqlDataProvider InsertUpdateDeleteDataThenSelectData Error. {0}", ex.ToString()));
                    }
                }
            }

            if (!isFailed)
                st.Commit();

            return resultDt;
        }
        #endregion protected void InsertUpdateDeleteDataThenSelectData (string connectionString, string commandText, List<SqlParameter[]> parameterses)

        #region protected void GetDataReturnObject (string connectionString, CommandType commandType, string commandText, SqlParameter[] parameters)
        protected void GetDataReturnObject(string connectionString, CommandType commandType, string commandText, SqlParameter[] parameters)
        {
            CheckArgs(ref connectionString, ref commandText);

            SqlConnection connection = GetConnection(connectionString, true);

            using (SqlCommand sqlCm = new SqlCommand(commandText, connection))
            {
                sqlCm.CommandType = commandType;

                if (parameters != null)
                    sqlCm.Parameters.AddRange(parameters);

                try
                {
                    SqlDataReader dr = sqlCm.ExecuteReader();
                    dr.Read();

                    if (dr.HasRows)
                        _ResultObject = dr[0];

                    dr.Close();
                    dr = null;
                }
                catch (SqlException sqlEx)
                {
                    throw new Exception(sqlEx.ToString());
                }
            }
        }
		#endregion protected void GetDataReturnObject (string connectionString, CommandType commandType, string commandText, SqlParameter[] parameters)

		#region protected void TransactionOperate(TransactionOperateType operateType)
		protected void TransactionOperate(TransactionOperateType operateType)
        {
            if (_Transaction != null)
            {
                if (operateType == TransactionOperateType.Commit)
                    _Transaction.Commit();
                else
                    _Transaction.Rollback();

                _Transaction.Dispose();
                _Transaction = null;
            }
            else
                throw new Exception("Transaction 為 NULL");
        }
		#endregion protected void TransactionOperate(TransactionOperateType operateType)

		#region 私有方法

		#region SqlConnection GetConnection(string connectionString, bool isIsolation = false)
		private SqlConnection GetConnection(string connectionString, bool isIsolation = false)
        {
            int key = connectionString.GetHashCode();
            SqlConnection connection;
            if (!isIsolation && ManageCollection.ContainsKey(key))
                connection = (SqlConnection)ManageCollection[key] as SqlConnection;
            else
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                if (!isIsolation)
                    ManageCollection.Add(key, connection);
            }

            return connection;
        }
        #endregion SqlConnection GetConnection(string connectionString, bool isIsolation = false)

        #endregion 私有方法

        #endregion 方法/私有方法
    }
    #endregion class MsSqlDataProvider

    public class DB_UpdaetMultiDatas_Msg
    {
        public int Code { get; set; }

        public List<int> Fails { get; set; }
        public List<int> Successes { get; set; }
	}
}
