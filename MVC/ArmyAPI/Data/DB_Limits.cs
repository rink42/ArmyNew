﻿#define DEBUG // 定义 DEBUG 符号
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
			}
			public DB_Limits(string connectionString) : base(connectionString, typeof(DB_Limits))
			{
			}
			#endregion 建構子

			#region List<Limits> GetAll()
			public List<Limits> GetAll()
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("SELECT * ");
				sb.AppendLine("FROM Limits ");
				sb.AppendLine("ORDER BY Sort; ");
				#endregion CommandText

				List<Limits> result = Get<Limits>(ConnectionString, sb.ToString(), null);

				return result;
			}
			#endregion List<Limits> GetAll()

			#region int Add(string title, int sort, bool isEnable, string userId)
			public int Add(string title, int sort, bool isEnable, string userId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("INSERT INTO Limits ");
				sb.AppendLine("         ([Sort], [Title], [IsEnable], [ModifyUserID]) ");
				sb.AppendLine("    VALUES (@Sort, @Title, @IsEnable, @ModifyUserID)");

				sb.AppendLine("SELECT SCOPE_IDENTITY();");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 50));
				parameters[parameterIndex++].Value = title;
				parameters.Add(new SqlParameter("@Sort", SqlDbType.Int));
				parameters[parameterIndex++].Value = sort;
				parameters.Add(new SqlParameter("@IsEnable", SqlDbType.Bit));
				parameters[parameterIndex++].Value = isEnable;
				parameters.Add(new SqlParameter("@ModifyUserID", SqlDbType.VarChar, 50));
				parameters[parameterIndex++].Value = userId;

				InsertUpdateDeleteDataThenSelectData(ConnectionString, sb.ToString(), parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion int Add(string title, int sort, bool isEnable, string userId)


			#region int Delete(int index, string userId)
			/// <summary>
			/// 刪除
			/// </summary>
			/// <param name="index"></param>
			/// <param name="id"></param>
			/// <param name="userId"></param>
			/// <returns></returns>
			public int Delete(int index, string userId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("DELETE FROM Limits ");
				sb.AppendLine("WHERE 1=1 ");
				sb.AppendLine("  AND [Index] = @Index ");

				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@Index", SqlDbType.Int));
				parameters[parameterIndex++].Value = index;

				int result = InsertUpdateDeleteData(ConnectionString, sb.ToString(), parameters.ToArray(), true);

				return result;
			}
			#endregion int Delete(int index, string userId)
		}
	}
}