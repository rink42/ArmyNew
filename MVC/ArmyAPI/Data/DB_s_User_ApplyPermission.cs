#define DEBUG // 定义 DEBUG 符号
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ArmyAPI.Commons;

namespace ArmyAPI.Data
{
	public partial class MsSqlDataProvider : IDisposable
	{
		public class DB_s_User_ApplyPermission : MsSqlDataProvider
		{
			#region static DB_s_User_ApplyPermission GetInstance ()
			public static DB_s_User_ApplyPermission GetInstance()
			{
				return (new DB_s_User_ApplyPermission());
			}
			#endregion static DB_s_User_ApplyPermission GetInstance ()

			#region 建構子
			public DB_s_User_ApplyPermission()
			{
				_TableName = "s_User_ApplyPermission";
			}
			public DB_s_User_ApplyPermission(string connectionString) : base(connectionString, typeof(DB_Menus))
			{
				_TableName = "s_User_ApplyPermission";
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

			#region int Inserts(string userId, string permissions)
			public int Inserts(string userId, string permissions)
			{
				List<string> queries = new List<string>();
				List<object> parametersList = new List<object>();
				dynamic userIdObj = new { UserID = userId };
				#region CommandText
				string commText = $@"
DELETE FROM {_TableName}
WHERE UserID = @UserID";
				queries.Add(commText);
				parametersList.Add(userIdObj);

				commText = $@"
INSERT INTO {_TableName}
	VALUES (@UserID, @PermissionStr) 

SELECT @@ROWCOUNT
";
				queries.Add(commText);
				#endregion CommandText
				dynamic insertDatas = new { UserID = userId, PermissionStr = permissions };

				parametersList.Add(insertDatas);

				int result = (new DapperHelper(BaseController._ConnectionString)).ExecuteTransaction(queries, parametersList);

				return result;
			}
			#endregion int Inserts(string userId, string permissions)
		}
	}
}