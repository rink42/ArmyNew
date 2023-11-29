#define DEBUG // 定义 DEBUG 符号
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using ArmyAPI.Commons;

namespace ArmyAPI.Data
{
	public partial class MsSqlDataProvider : IDisposable
	{
		public class DB_TableauUsed : MsSqlDataProvider
		{
			#region static DB_TableauUsed GetInstance ()
			public static DB_TableauUsed GetInstance()
			{
				return (new DB_TableauUsed());
			}
			#endregion static DB_TableauUsed GetInstance ()

			#region 建構子
			public DB_TableauUsed()
			{
			}
			public DB_TableauUsed(string connectionString) : base(connectionString, typeof(DB_TableauUsed))
			{
			}
			#endregion 建構子

			#region public int Record(string url)
			public int Record(string url)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				string tableName = "TableauUsed";
				#region CommandText
				sb.AppendLine("-- 檢查 URL 是否存在於表中 ");
				sb.AppendLine($"IF NOT EXISTS (SELECT 1 FROM {tableName} WHERE URL = @URL) ");
				sb.AppendLine("BEGIN ");
				sb.AppendLine("    -- 如果 URL 不存在，則插入新記錄 ");
				sb.AppendLine($"    INSERT INTO {tableName} (URL, Count) ");
				sb.AppendLine("    VALUES (@URL, 1) ");
				sb.AppendLine("END ");
				sb.AppendLine("ELSE ");
				sb.AppendLine("BEGIN ");
				sb.AppendLine("    -- 如果 URL 存在，則更新 Count = Count + 1 ");
				sb.AppendLine($"    UPDATE {tableName} ");
				sb.AppendLine("    SET Count = Count + 1 ");
				sb.AppendLine("    WHERE URL = @URL ");
				sb.AppendLine("END ");
				sb.AppendLine("SELECT @@ROWCOUNT");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@URL", SqlDbType.VarChar));
				parameters[parameterIndex++].Value = url;

				InsertUpdateDeleteDataThenSelectData(ConnectionString, sb.ToString(), parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion public int Record(string url)
		}
	}
}