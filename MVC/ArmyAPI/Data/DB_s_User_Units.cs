#define DEBUG // 定义 DEBUG 符号
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI.WebControls;
using ArmyAPI.Commons;
using ArmyAPI.Models;
using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.SqlServer.Server;
using NPOI.SS.Formula.Functions;
using static ArmyAPI.Data.MsSqlDataProvider.DB_Tableau;

namespace ArmyAPI.Data
{
	public partial class MsSqlDataProvider : IDisposable
	{
		public class DB_s_User_Units : MsSqlDataProvider
		{
			#region static DB_s_User_Units GetInstance ()
			public static DB_s_User_Units GetInstance()
			{
				return (new DB_s_User_Units());
			}
			#endregion static DB_s_User_Units GetInstance ()

			#region 建構子
			public DB_s_User_Units()
			{
				_TableName = "s_User_Units";
			}
			public DB_s_User_Units(string connectionString) : base(connectionString, typeof(DB_Menus))
			{
				_TableName = "s_User_Units";
			}
			#endregion 建構子

			#region string GetByUserId(string userId)
			public string GetByUserId(string userId)
			{
				#region CommandText
				string commText = $@"
SELECT UnitCode
FROM {_TableName}
WHERE 1=1
  AND UserID = @UserId
";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = userId;

				GetDataReturnDataTable(ConnectionString, commText, parameters.ToArray());

				string result = "";

				if (_ResultDataTable != null && _ResultDataTable.Rows.Count > 0)
				{
					foreach (DataRow row in _ResultDataTable.Rows)
					{
						if (result.Length > 0)
							result += ",";
						result += row["UnitCode"].ToString();
					}
				}

				return result;
			}
			#endregion string GetByUserId(string userId)

			#region int Inserts(List<string> units, string userId)
			public int Inserts(List<string> units, string userId)
			{
				int result = 0;
				using (IDbConnection conn = new SqlConnection(BaseController._ConnectionString))
				{
					conn.Open();

					using (var tran = conn.BeginTransaction())
					{
						try
						{
							string commText = $@"
DELETE FROM {_TableName}
WHERE UserID = @UserID";
							result = conn.Execute(commText, new { UserID = userId }, tran);

							commText = $@"
INSERT INTO {_TableName}
			(UserID, UnitCode)
	VALUES (@UserID, @Units)
";
							foreach (string u in units)
							{
								result += conn.Execute(commText, new { UserID = userId, Units = u }, tran);
							}

							tran.Commit();
						}
						catch (Exception ex)
						{
							WriteLog.Log("DB_s_User_Units Insert error", ex.ToString());
							tran.Rollback();
						}
					}
				}

				return result;
			}
			#endregion int Inserts(List<string> units, string userId)
		}
	}
}