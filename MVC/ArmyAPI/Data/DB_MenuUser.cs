#define DEBUG // 定义 DEBUG 符号
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ArmyAPI.Commons;
using ArmyAPI.Models;

namespace ArmyAPI.Data
{
	public partial class MsSqlDataProvider : IDisposable
	{
		public class DB_MenuUser : MsSqlDataProvider
		{
			private string _TableName1 = "Menus";
			private string _TableName2 = "Users";

			#region static DB_MenuUser GetInstance ()
			public static DB_MenuUser GetInstance()
			{
				return (new DB_MenuUser());
			}
			#endregion static DB_MenuUser GetInstance ()

			#region 建構子
			public DB_MenuUser()
			{
				_TableName = "MenuUser";
			}
			public DB_MenuUser(string connectionString) : base(connectionString, typeof(DB_Menus))
			{
				_TableName = "MenuUser";
			}
			#endregion 建構子

			#region int Add(int menuIndex, string userId, string loginId)
			public int Add(int menuIndex, string userId, string loginId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"IF NOT EXISTS (SELECT UserID FROM {_TableName} WHERE [MenuIndex] = @MenuIndex AND [UserID] = @UserID) ");
				sb.AppendLine("  BEGIN ");
				sb.AppendLine($"    IF EXISTS (SELECT UserID FROM {_TableName2} WHERE [UserID] = @UserID) AND EXISTS (SELECT [Index] FROM {_TableName1} WHERE [Index] = @MenuIndex) ");
				sb.AppendLine("      BEGIN ");
				sb.AppendLine($"        INSERT INTO {_TableName} ");
				sb.AppendLine("                 ([MenuIndex], [UserID]) ");
				sb.AppendLine("            VALUES (@MenuIndex, @UserID) ");
				sb.AppendLine("        SELECT 1 ");
				sb.AppendLine("      END ");
				sb.AppendLine("    ELSE ");
				sb.AppendLine("      BEGIN ");
				sb.AppendLine("        SELECT -2 ");
				sb.AppendLine("      END ");
				sb.AppendLine("  END ");
				sb.AppendLine("ELSE ");
				sb.AppendLine("  BEGIN ");
				sb.AppendLine("    SELECT -1 ");
				sb.AppendLine("  END ");
				#endregion CommandText

				List <SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@MenuIndex", SqlDbType.Int));
				parameters[parameterIndex++].Value = menuIndex;
				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = userId;

				InsertUpdateDeleteDataThenSelectData(ConnectionString, sb.ToString(), parameters.ToArray(), ReturnType.Int, true);

				int result = 0;
				if (int.TryParse(_ResultObject.ToString(), out int tmp))
					result = tmp;

				return result;
			}
			#endregion int Add(int menuIndex, string userId, string loginId)

			#region int Delete(string userId, string loginId)
			/// <summary>
			/// 刪除
			/// </summary>
			/// <param name="userId"></param>
			/// <returns></returns>
			public int Delete(string userId, string loginId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"DELETE FROM {_TableName} ");
				sb.AppendLine("WHERE 1=1 ");
				sb.AppendLine("  AND [UserID] = @UserID ");
				#endregion CommandText

				//List<SqlParameter> parameters = new List<SqlParameter>();
				//int parameterIndex = 0;

				//parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 50));
				//parameters[parameterIndex++].Value = userId;

				var parameters = new { UserID = userId };

				int result = Dapper_InsertUpdateDeleteData(ConnectionString, sb.ToString(), parameters);

				return result;
			}
			#endregion int Delete(string userId, string loginId)

			#region int Adds(string menuIndexs, string userId, string loginId)
			public int Adds(string menuIndexs, string userId, string loginId)
			{
				Delete(userId, loginId);

				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"INSERT INTO {_TableName} ");
				sb.AppendLine("         ([MenuIndex], [UserID]) ");
				sb.AppendLine("    VALUES (@MenuIndex, @UserID) ");
				#endregion CommandText

				int result = 0;

				var menusUser = new List<MenusUser>();
				if (!string.IsNullOrEmpty(menuIndexs))
				{
					foreach (var m in menuIndexs.Split(','))
					{
						if (!string.IsNullOrEmpty(m.ToString()))
						{
							menusUser.Add(new MenusUser()
							{
								MenuIndex = int.Parse(m),
								UserID = userId
							});
						}
					}
				}

				Dapper_InsertUpdateDeleteData(ConnectionString, sb.ToString(), menusUser);

				return result;
			}
			#endregion int Adds(string menuIndexs, string userId, string loginId)

			#region string GetByUserId(string userId)
			public string GetByUserId(string userId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"SELECT [MenuIndex]  ");
				sb.AppendLine($"FROM {_TableName} ");
				sb.AppendLine("WHERE 1=1 ");
				sb.AppendLine("  AND [UserID] = @UserID ");
				#endregion CommandText

				var parameters = new { UserID = userId };

				DataTable dt = DapperHelper.GetInstance().ExecuteQuery(sb.ToString(), parameters);

				string result = "";
				if (dt != null && dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						if (!string.IsNullOrEmpty(result)) result += ",";

						result += dr[0].ToString();
					}
				}

				return result;
			}
			#endregion string GetByUserId(string userId)
		}
	}
}