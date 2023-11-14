#define DEBUG // 定义 DEBUG 符号
using ArmyAPI.Commons;
using ArmyAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Web.Http;

namespace ArmyAPI.Data
{
	public partial class MsSqlDataProvider : IDisposable
	{
		public class DB_LimitsUser : MsSqlDataProvider
		{
			#region static DB_LimitsUser GetInstance ()
			public static DB_LimitsUser GetInstance()
			{
				return (new DB_LimitsUser());
			}
			#endregion static DB_LimitsUser GetInstance ()

			#region 建構子
			public DB_LimitsUser()
			{
				_TableName = "LimitsUser";
			}
			public DB_LimitsUser(string connectionString) : base(connectionString, typeof(DB_Limits))
			{
				_TableName = "LimitsUser";
			}
			#endregion 建構子

			#region int Update(string userId, string limitCodes)
			public int Update(string userId, string limitCodes)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				string limitTable = "Limits";
				#region CommandText
				sb.AppendLine($"DELETE FROM {_TableName} ");
				sb.AppendLine("WHERE 1=1 ");
				sb.AppendLine("  AND [UserID] = @UserID ");

				sb.AppendLine($"INSERT INTO {_TableName} ");
				sb.AppendLine($"    SELECT L.[LimitCode], @UserID FROM {limitTable} L CROSS APPLY STRING_SPLIT(@LimitCodes, ',') AS SplitCodes WHERE L.[LimitCode] LIKE SplitCodes.value + '%' ");

				sb.AppendLine("SELECT @@ROWCOUNT ");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@LimitCodes", SqlDbType.VarChar));
				parameters[parameterIndex++].Value = limitCodes;
				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 50));
				parameters[parameterIndex++].Value = userId;

				InsertUpdateDeleteDataThenSelectData(ConnectionString, sb.ToString(), parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion int Update(string userId, string limitCodes)


			#region int DeleteByUserId(string userId)
			/// <summary>
			/// 刪除這個使用者擁有的全部權限
			/// </summary>
			/// <param name="userId"></param>
			/// <returns></returns>
			public int DeleteByUserId(string userId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"DELETE FROM {_TableName} ");
				sb.AppendLine("WHERE 1=1 ");
				sb.AppendLine("  AND [UserID] = @UserID ");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = userId;

				int result = InsertUpdateDeleteData(ConnectionString, sb.ToString(), parameters.ToArray(), true);

				return result;
			}
			#endregion int Delete(string code, string userId)
		}
	}
}