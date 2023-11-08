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
		public class DB_MenuUserGroup : MsSqlDataProvider
		{
			private string _TableName1 = "Menus";
			private string _TableName2 = "UserGroup";

			#region static DB_MenuUserGroup GetInstance ()
			public static DB_MenuUserGroup GetInstance()
			{
				return (new DB_MenuUserGroup());
			}
			#endregion static DB_MenuUserGroup GetInstance ()

			#region 建構子
			public DB_MenuUserGroup()
			{
				_TableName = "MenuUserGroup";
			}
			public DB_MenuUserGroup(string connectionString) : base(connectionString, typeof(DB_Menus))
			{
				_TableName = "MenuUserGroup";
			}
			#endregion 建構子

			#region int Add(int menuIndex, int userGroupIndex, string userId)
			public int Add(int menuIndex, int userGroupIndex, string userId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"IF NOT EXISTS (SELECT UserGroupIndex FROM {_TableName} WHERE MenuIndex = @MenuIndex AND UserGroupIndex = @UserGroupIndex) ");
				sb.AppendLine("  BEGIN ");
				sb.AppendLine($"   IF EXISTS (SELECT [Index] FROM {_TableName1} WHERE [Index] = @MenuIndex) AND EXISTS (SELECT [Index] FROM {_TableName2} WHERE [Index] = @UserGroupIndex) ");
				sb.AppendLine("      BEGIN ");
				sb.AppendLine($"        INSERT INTO {_TableName} ");
				sb.AppendLine("                 ([MenuIndex], [UserGroupIndex]) ");
				sb.AppendLine("            VALUES (@MenuIndex, @UserGroupIndex) ");
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
				parameters.Add(new SqlParameter("@UserGroupIndex", SqlDbType.Int));
				parameters[parameterIndex++].Value = userGroupIndex;

				InsertUpdateDeleteDataThenSelectData(ConnectionString, sb.ToString(), parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion int Add(int menuIndex, int userGroupIndex, string userId)
		}
	}
}