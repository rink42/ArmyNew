using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Configuration;


namespace ArmyAPI.Services
{
    
    public class DbHelper
    {
        private readonly string _ArmyConnectionString;
        private readonly string _ArmyWebConnectionString;

        public DbHelper()
        {
            _ArmyConnectionString = WebConfigurationManager.ConnectionStrings["ArmyConnection"].ToString();
            _ArmyWebConnectionString = WebConfigurationManager.ConnectionStrings["ArmyWebConnection"].ToString();
        }

        public DataTable ArmyExecuteQuery(string sql, SqlParameter[] parameters = null)
        {
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(_ArmyConnectionString))
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    try
                    {
                        connection.Open();
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog.Log(String.Format("MsSqlDataProvider ArmyExecuteQuery Error. {0}", ex.ToString()));
                        throw new Exception(String.Format("MsSqlDataProvider ArmyExecuteQuery Error. {0}", ex.ToString()));
                    }
                }
            }

            return dataTable;
        }


        public DataTable ArmyWebExecuteQuery(string sql, SqlParameter[] parameters = null)
        {
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(_ArmyWebConnectionString))
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    try
                    {
                        connection.Open();
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog.Log(String.Format("MsSqlDataProvider Army2ExecuteQuery Error. {0}", ex.ToString()));
                        throw new Exception(String.Format("MsSqlDataProvider Army2ExecuteQuery Error. {0}", ex.ToString()));
                    }
                }
            }

            return dataTable;
        }

        public bool ArmyWebUpdate(string sql, SqlParameter[] parameters = null)
        {
            bool nRow = true;
            int sqlCheck = 0;
            string outMsg = string.Empty;
            try
            {
                using (SqlConnection connection = new SqlConnection(_ArmyWebConnectionString))
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        connection.Open();
                        sqlCheck = command.ExecuteNonQuery();
                        if (sqlCheck == 0)
                        {
                            nRow = false;
                        }
                        
                    }
                }
                outMsg = "";
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("MsSqlDataProvider Army2Update Error. {0}", ex.ToString()));
                outMsg = "Database operation failed" + ex.ToString();
                nRow = false;
                throw new Exception(String.Format("MsSqlDataProvider Army2Update Error. {0}", ex.ToString()));
            }
            return nRow;
        }

        public bool ArmyUpdate(string sql, SqlParameter[] parameters = null)
        {
            bool nRow = true;
            int sqlCheck = 0;
            string outMsg = string.Empty;
            try
            {
                using (SqlConnection connection = new SqlConnection(_ArmyConnectionString))
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        connection.Open();
                        sqlCheck = command.ExecuteNonQuery();
                        if (sqlCheck == 0)
                        {
                            nRow = false;
                        }
                    }
                }
                outMsg = "";
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("MsSqlDataProvider ArmyUpdate Error. {0}", ex.ToString()));
                outMsg = "Database operation failed" + ex.ToString(); ;
                nRow = false;
                throw new Exception(String.Format("MsSqlDataProvider ArmyUpdate Error. {0}", ex.ToString()));
            }
            return nRow;
        }


    }
}