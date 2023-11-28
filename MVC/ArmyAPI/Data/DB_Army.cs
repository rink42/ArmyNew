#define DEBUG // 定义 DEBUG 符号
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using ArmyAPI.Models;
using System.Configuration;
using System.Web.UI.WebControls;
using System.Diagnostics;

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

			#region Army_Unit GetOriginal()
			public Army_Unit GetOriginal()
			{
				Army_Unit result = new Army_Unit();
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				int getlevel = int.Parse(ConfigurationManager.AppSettings.Get("GetArmyUnitLevel"));

				string tableName = "army.dbo.v_mu_unit AS u";
				#region CommandText
				for (int i = 0; i < getlevel; i++)
				{
					sb.AppendLine($"DECLARE @Level{i} TABLE (unit_code VARCHAR(32), unit_title VARCHAR(100), ulevel_code CHAR(1), parent_unit_code VARCHAR(32))");
				}

				sb.AppendLine("--第0層 ");
				sb.AppendLine("INSERT INTO @Level0 ");
				sb.AppendLine("    SELECT u.unit_code, u.unit_title, u.ulevel_code, u.parent_unit_code ");
				sb.AppendLine($"	FROM {tableName} ");
				sb.AppendLine("	WHERE 1=1 ");
				sb.AppendLine("	  --AND u.unit_code = '4C68CEA7E58591B579FD074BCDAFF740' ");
				sb.AppendLine("	  AND u.unit_code = '00001' ");

				sb.AppendLine("SELECT * FROM @Level0 ");

				for (int i = 1; i < getlevel; i++)
				{
					sb.AppendLine($"--第{i}層 ");
					sb.AppendLine($"INSERT INTO @Level{i} ");
					sb.AppendLine("    SELECT u.unit_code, u.unit_title, u.ulevel_code, u.parent_unit_code ");
					sb.AppendLine($"	FROM {tableName} ");
					sb.AppendLine($"	  RIGHT JOIN @Level{i - 1} l{i - 1} ON l{i - 1}.unit_code = u.parent_unit_code ");
					sb.AppendLine("	WHERE 1=1 ");
					sb.AppendLine("	  --AND u.unit_status != '0' ");
					sb.AppendLine($"SELECT * FROM @Level{i - 1} ");
				}
				#endregion CommandText

				DataSet ds = GetDataSet(ConnectionString, sb.ToString(), null);

				result = ConvertToUnits(ds.Tables);

				return result;
			}
			#endregion List<Army_Unit> GetOriginal()

			private Army_Unit ConvertToUnits(DataTableCollection dataTables)
			{
				Dictionary<string, Army_Unit> unitDictionary = new Dictionary<string, Army_Unit>();

				foreach (DataTable dataTable in dataTables)
				{
					foreach (DataRow row in dataTable.Rows)
					{
						string code = row["unit_code"].ToString().Trim();
						string title = row["unit_title"].ToString().Trim();
						string level = row["ulevel_code"].ToString().Trim();
						string parentCode = row["parent_unit_code"].ToString().Trim();

						Army_Unit currentUnit;
						if (unitDictionary.ContainsKey(code))
						{
							currentUnit = unitDictionary[code];
							currentUnit.title = title;
							currentUnit.level = level;
						}
						else
						{
							currentUnit = new Army_Unit { code = code, title = title, level = level };
							unitDictionary[code] = currentUnit;
						}

						if (!string.IsNullOrEmpty(parentCode) && unitDictionary.ContainsKey(parentCode))
						{
							Army_Unit parentUnit = unitDictionary[parentCode];
							if (parentUnit != null)
							{
								if (parentUnit.children == null)
									parentUnit.children = new List<Army_Unit>();
								currentUnit.parent_code = parentCode;
								parentUnit.children.Add(currentUnit);
							}
						}
					}
				}

				// Find and return the root units (units without a parent)
				Army_Unit rootUnit = unitDictionary[dataTables[0].Rows[0]["unit_code"].ToString()];
				//foreach (var unit in unitDictionary.Values)
				//{
				//	if (unit.Items.Count == 0)
				//	{
				//		rootUnits.Add(unit);
				//	}
				//}

				return rootUnit;
			}
		}
	}
}