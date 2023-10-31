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
		public class DB_UserGroup : MsSqlDataProvider
		{
			#region static DB_UserGroup GetInstance ()
			public static DB_UserGroup GetInstance()
			{
				return (new DB_UserGroup());
			}
			#endregion static DB_UserGroup GetInstance ()

			#region 建構子
			public DB_UserGroup()
			{
			}
			public DB_UserGroup(string connectionString) : base(connectionString, typeof(DB_UserGroup))
			{
			}
			#endregion 建構子

			#region List<UserGroup> GetAll()
			public List<UserGroup> GetAll()
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("SELECT * ");
				sb.AppendLine("FROM UserGroup ");
				sb.AppendLine("ORDER BY Sort; ");
				#endregion CommandText

				List<UserGroup> result = Get<UserGroup>(ConnectionString, sb.ToString(), null);

				return result;
			}
			#endregion List<UserGroup> GetAll()

			#region int Add(string title, bool isEnable, string userId)
			public int Add(string title, bool isEnable, string userId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("INSERT INTO UserGroup ");
				sb.AppendLine("         ([Sort], [Title], [IsEnable], [ModifyUserID]) ");
				sb.AppendLine("    VALUES (@Sort, @Title, @Route_Tableau, @IsEnable, @ModifyUserID)");

				sb.AppendLine("SELECT SCOPE_IDENTITY();");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 50));
				parameters[parameterIndex++].Value = title;
				parameters.Add(new SqlParameter("@IsEnable", SqlDbType.Bit));
				parameters[parameterIndex++].Value = isEnable;
				parameters.Add(new SqlParameter("@ModifyUserID", SqlDbType.VarChar, 50));
				parameters[parameterIndex++].Value = userId;

				InsertUpdateDeleteDataThenSelectData(ConnectionString, sb.ToString(), parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion int Add(string title, bool isEnable, string userId)


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
				sb.AppendLine("DELETE FROM UserGroup ");
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