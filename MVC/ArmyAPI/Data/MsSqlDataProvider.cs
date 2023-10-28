using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

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
                        connection.Dispose();
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
        protected DataTable GetDataTable(string connectionString, string commandText, SqlParameter[] parameters)
        {
            CheckArgs(ref connectionString, ref commandText);

            SqlConnection connection = GetConnection(connectionString);

            using (SqlCommand sqlCm = new SqlCommand(commandText, connection))
            {
                if (parameters != null)
                    sqlCm.Parameters.AddRange(parameters);

                DataTable dataTable = new DataTable();
                using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCm))
                {
                    adapter.Fill(dataTable);
                }

                return dataTable;
            }
        }
        #endregion protected DataTable GetDataTable (string connectionString, string commandText, SqlParameter[] parameters)

        #region List<T> Get<T>(string connectionString, string commandText, SqlParameter[] parameters) where T : new()
        public List<T> Get<T>(string connectionString, string commandText, SqlParameter[] parameters) where T : new()
        {
            List<T> result = new List<T>();

            SqlConnection connection = GetConnection(connectionString);

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

        #region public T GetOne<T>(string connectionString, string commandText, SqlParameter[] parameters) where T : new()
        public T GetOne<T>(string connectionString, string commandText, SqlParameter[] parameters) where T : new()
        {
            SqlConnection connection = GetConnection(connectionString);

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
                                property.SetValue(row, reader.GetValue(i));
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

        #region protected int InsertUpdateDeleteData (string connectionString, string commandText, SqlParameter[] parameters, bool isIsolation = false)
        protected int InsertUpdateDeleteData(string connectionString, string commandText, SqlParameter[] parameters, bool isIsolation = false)
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
                            sb.AppendLine($"    parameter[{index}].DbTyp e= {parameter.DbType}");
                            sb.AppendLine($"    parameter[{index}].Value = {parameter.Value}\n");
                        }
                    }

                    throw new Exception($"MsSqlDataProvider.cs / InsertUpdateDeleteData 失敗,\n commandText = {commandText},\n parameters = {sb},\n ex = {ex}");
                }
            }
        }
        #endregion protected int InsertUpdateDeleteData (string connectionString, string commandText, SqlParameter[] parameters, bool isIsolation = false)

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
}
