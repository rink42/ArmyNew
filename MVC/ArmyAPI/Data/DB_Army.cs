﻿#define DEBUG // 定义 DEBUG 符号
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using ArmyAPI.Models;

namespace ArmyAPI.Data
{
	public partial class MsSqlDataProvider : IDisposable
	{
		public class DB_Army : MsSqlDataProvider
		{
			#region static DB_Army_Rank GetInstance ()
			public static DB_Army GetInstance()
			{
				return (new DB_Army());
			}
			#endregion static DB_Army_Rank GetInstance ()

			#region 建構子
			public DB_Army()
			{
			}
			public DB_Army(string connectionString) : base(connectionString, typeof(DB_Army))
			{
			}
			#endregion 建構子

			#region List<Rank> GetRanks()
			public List<Rank> GetRanks()
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				string tableName = "Rank";
				#region CommandText
				sb.AppendLine("SELECT TRIM(rank_code) as rank_code, TRIM(rank_title) as rank_title ");
				sb.AppendLine($"FROM Army.dbo.{tableName} ");
				sb.AppendLine("WHERE rank_code BETWEEN '01' AND '92' ");
				sb.AppendLine("ORDER BY rank_code; ");
				#endregion CommandText

				List<Rank> result = Get<Rank>(ConnectionString, sb.ToString(), null);

				return result;
			}
			#endregion List<Rank> GetRanks()

			#region List<Skill> GetSkills()
			public List<Skill> GetSkills()
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				string tableName = "skill";
				#region CommandText
				sb.AppendLine("SELECT TRIM(skill_code) as skill_code, TRIM(skill_desc) as skill_desc ");
				sb.AppendLine($"FROM Army.dbo.{tableName} ");
				sb.AppendLine("WHERE skill_status != '0' ");
				#endregion CommandText

				List<Skill> result = Get<Skill>(ConnectionString, sb.ToString(), null);

				return result;
			}
			#endregion List<Skill> GetSkills()

			#region List<Title> GetTitles(string name)
			public List<Title> GetTitles(string name)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				string tableName = "title";
				#region CommandText
				sb.AppendLine("SELECT TOP 100 TRIM(title_code) as title_code, TRIM(title_name) as title_name ");
				sb.AppendLine($"FROM Army.dbo.{tableName} ");
				sb.AppendLine("WHERE 1=1 ");
				sb.AppendLine("  AND title_name LIKE '%' + @TitleName + '%' ");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@TitleName", SqlDbType.NVarChar));
				parameters[parameterIndex++].Value = name;

				List<Title> result = Get<Title>(ConnectionString, sb.ToString(), parameters.ToArray());

				return result;
			}
			#endregion List<Title> GetTitles(string name)
		}
	}
}