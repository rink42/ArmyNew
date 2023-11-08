#define DEBUG // 定义 DEBUG 符号
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

			#region string CheckUserData(string userId, string name, string birthday, string email, string phone)
			public string CheckUserData(string userId, string name, string birthday, string email, string phone)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				string tableName1 = "v_member_data";
				string tableName2 = "Users";
				#region CommandText
				sb.AppendLine("DECLARE @IsAD BIT ");
				sb.AppendLine("SET @IsAD = 0 ");
				sb.AppendLine("IF (SELECT LEN([Password]) FROM Users WHERE USerID = @UserId) = 0 ");
				sb.AppendLine("  BEGIN ");
				sb.AppendLine("    SET @IsAD = 1");
				sb.AppendLine("  END ");

				sb.AppendLine("IF @IsAD = 0 ");
				sb.AppendLine("  BEGIN ");
				sb.AppendLine("    SELECT COUNT(vm.member_id) ");
				sb.AppendLine($"    FROM Army.dbo.{tableName1} AS vm");
				sb.AppendLine($"      LEFT JOIN {tableName2} AS u on u.UserID = vm.member_id ");
				sb.AppendLine("    WHERE 1=1 ");
				sb.AppendLine("      AND vm.member_id = @UserId ");
				sb.AppendLine("      AND vm.member_name = @Name ");
				sb.AppendLine("      AND u.Email = @Email ");
				sb.AppendLine("      AND CONVERT(VARCHAR(8), vm.birthday, 112) = @Birthday ");
				sb.AppendLine("      AND u.Phone = @Phone OR u.PhoneMil = @Phone ");
				sb.AppendLine("  END ");
				sb.AppendLine("ELSE ");
				sb.AppendLine("  BEGIN ");
				sb.AppendLine("    SELECT -1 ");
				sb.AppendLine("  END ");

				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserId", SqlDbType.Char, 10));
				parameters[parameterIndex++].Value = userId;
				parameters.Add(new SqlParameter("@Name", SqlDbType.VarChar, 40));
				parameters[parameterIndex++].Value = name;
				parameters.Add(new SqlParameter("@Birthday", SqlDbType.VarChar, 8));
				parameters[parameterIndex++].Value = birthday;
				parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 128));
				parameters[parameterIndex++].Value = email;
				parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 50));
				parameters[parameterIndex++].Value = phone;

				GetDataReturnObject(ConnectionString, CommandType.Text, sb.ToString(), parameters.ToArray());

				string result = "0";
				if (_ResultObject != null)
					result = _ResultObject.ToString();

				return result;
			}
			#endregion string CheckUserData(string userId, string name, string birthday, string email, string phone)
		}
	}
}