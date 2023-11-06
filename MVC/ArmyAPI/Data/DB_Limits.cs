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
		public class DB_Limits : MsSqlDataProvider
		{
			#region static DB_Limits GetInstance ()
			public static DB_Limits GetInstance()
			{
				return (new DB_Limits());
			}
			#endregion static DB_Limits GetInstance ()

			#region 建構子
			public DB_Limits()
			{
				_TableName = "Limits";
			}
			public DB_Limits(string connectionString) : base(connectionString, typeof(DB_Limits))
			{
				_TableName = "Limits";
			}
			#endregion 建構子

			#region List<Limits> GetAll()
			public List<Limits> GetAll()
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("SELECT * ");
				sb.AppendLine($"FROM {_TableName} ");
				sb.AppendLine("ORDER BY Sort; ");
				#endregion CommandText

				List<Limits> result = Get<Limits>(ConnectionString, sb.ToString(), null);

				return result;
			}
			#endregion List<Limits> GetAll()

			#region int Add(short category, string title, int sort, bool isEnable, string parentCode, string userId)
			public int Add(short category, string title, int sort, bool isEnable, string parentCode, string userId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("IF LEN(@ParentCode) > 0 ");
				sb.AppendLine("BEGIN ");
				sb.AppendLine($"  IF NOT EXISTS (SELECT 1 FROM {_TableName} WHERE LimitCode = @ParentCode) ");
				sb.AppendLine("  BEGIN ");
				sb.AppendLine("    SELECT -1");
				sb.AppendLine("    RETURN ");
				sb.AppendLine("  END ");
				sb.AppendLine("END ");

				sb.AppendLine("DECLARE @Sort1 INT");
				sb.AppendLine("SET @Sort1 = @Sort;");

				sb.AppendLine("IF @Sort1 = 0");
				sb.AppendLine("  BEGIN ");
				sb.AppendLine($"    SELECT @Sort1 = MAX([Sort]) + 1 FROM {_TableName} WHERE [Category] = @Category ");
				sb.AppendLine("  END ");

				sb.AppendLine($"INSERT INTO {_TableName} ");
				sb.AppendLine("         ([LimitCode], [Category], [Title], [IsEnable], [Sort], [ParentCode], [ModifyUserID]) ");
				sb.AppendLine("    VALUES (@LimitCode, @Category, @Title, @IsEnable, @Sort1, @ParentCode, @ModifyUserID) ");

				sb.AppendLine("SELECT @@ROWCOUNT ");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@LimitCode", SqlDbType.VarChar, 32));
				parameters[parameterIndex++].Value = Md5.Encode($"{title}{category}");
				parameters.Add(new SqlParameter("@Category", SqlDbType.TinyInt));
				parameters[parameterIndex++].Value = category;
				parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 50));
				parameters[parameterIndex++].Value = title;
				parameters.Add(new SqlParameter("@IsEnable", SqlDbType.Bit));
				parameters[parameterIndex++].Value = isEnable;
				parameters.Add(new SqlParameter("@Sort", SqlDbType.Int));
				parameters[parameterIndex++].Value = sort;
				parameters.Add(new SqlParameter("@ParentCode", SqlDbType.VarChar, 32));
				parameters[parameterIndex++].Value = parentCode;
				parameters.Add(new SqlParameter("@ModifyUserID", SqlDbType.VarChar, 50));
				parameters[parameterIndex++].Value = userId;

				InsertUpdateDeleteDataThenSelectData(ConnectionString, sb.ToString(), parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion int Add(short category, string title, int sort, bool isEnable, string parentCode, string userId)

			#region int Update(string code,  short category, string title, int sort, bool isEnable, string parentCode, string userId)
			public int Update(string code,  short category, string title, int sort, bool isEnable, string parentCode, string userId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"UPDATE {_TableName} ");
				sb.AppendLine("         SET [Category] = @Category, [Title] = @Title, [IsEnable] = @IsEnable, [Sort] = @Sort, [ParentCode] = @ParentCode, [ModifyDatetime] = GETDATE(), [ModifyUserID] = @ModifyUserID ");
				sb.AppendLine("WHERE 1=1 ");
				sb.AppendLine("  AND [LimitCode] = @LimitCode ");

				sb.AppendLine("SELECT @@ROWCOUNT ");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@LimitCode", SqlDbType.VarChar, 32));
				parameters[parameterIndex++].Value = code;
				parameters.Add(new SqlParameter("@Category", SqlDbType.TinyInt));
				parameters[parameterIndex++].Value = category;
				parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 50));
				parameters[parameterIndex++].Value = title;
				parameters.Add(new SqlParameter("@IsEnable", SqlDbType.Bit));
				parameters[parameterIndex++].Value = isEnable;
				parameters.Add(new SqlParameter("@Sort", SqlDbType.Int));
				parameters[parameterIndex++].Value = sort;
				parameters.Add(new SqlParameter("@ParentCode", SqlDbType.VarChar, 32));
				parameters[parameterIndex++].Value = parentCode;
				parameters.Add(new SqlParameter("@ModifyUserID", SqlDbType.VarChar, 50));
				parameters[parameterIndex++].Value = userId;

				InsertUpdateDeleteDataThenSelectData(ConnectionString, sb.ToString(), parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion int Update(string code,  short category, string title, int sort, bool isEnable, string parentCode, string userId)


			#region int Delete(string code, string userId)
			/// <summary>
			/// 刪除
			/// </summary>
			/// <param name="index"></param>
			/// <param name="id"></param>
			/// <param name="userId"></param>
			/// <returns></returns>
			public int Delete(string code, string userId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"DELETE FROM {_TableName} ");
				sb.AppendLine("WHERE 1=1 ");
				sb.AppendLine("  AND [LimitCode] = @LimitCode ");

				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@LimitCode", SqlDbType.VarChar, 32));
				parameters[parameterIndex++].Value = code;

				int result = InsertUpdateDeleteData(ConnectionString, sb.ToString(), parameters.ToArray(), true);

				return result;
			}
			#endregion int Delete(string code, string userId)
		}
	}
}