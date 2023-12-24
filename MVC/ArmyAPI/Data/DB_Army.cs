#define DEBUG // 定义 DEBUG 符号
using ArmyAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

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

			#region ArmyUser GetUser(string memberId)
			public ArmyUser GetUser(string memberId)
			{
				string tableName = "v_member_data";
				#region CommandText
				string commText = $@"
SELECT * 
FROM Army.dbo.{tableName} 
WHERE 1=1 
  AND member_id = @member_id 
";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@member_id", SqlDbType.Char, 10));
				parameters[parameterIndex++].Value = memberId;

				GetDataReturnDataTable(ConnectionString, commText, parameters.ToArray());

				DataTable dt = _ResultDataTable;

				ArmyUser armyUser = null;
				if (dt != null && dt.Rows.Count == 1) {
					DataRow dr = dt.Rows[0];

					armyUser = new ArmyUser();
					armyUser.MemberId = dr["member_id"].ToString();
					armyUser.UnitCode = dr["unit_code"].ToString();
					armyUser.NonEsCode = dr["non_es_code"].ToString();
					armyUser.ItemNo = dr["item_no"].ToString();
					armyUser.ColumnNo = dr["column_no"].ToString();
					armyUser.SerialCode = dr["serial_code"].ToString();
					armyUser.EsRankCode = dr["es_rank_code"].ToString();
					armyUser.PreEsSkillCode = dr["pre_es_skill_code"].ToString();
					armyUser.EsSkillCode = dr["es_skill_code"].ToString();
					armyUser.TitleCode = dr["title_code"].ToString();
					armyUser.PayDate = dr["pay_date"].ToString();
					armyUser.CampaignCode = dr["campaign_code"].ToString();
					armyUser.ServiceCode = dr["service_code"].ToString();
					armyUser.GroupCode = dr["group_code"].ToString();
					armyUser.RankCode = dr["rank_code"].ToString();
					armyUser.RankDate = dr["rank_date"].ToString();
					armyUser.PreMSkillCode = dr["pre_m_skill_code"].ToString();
					armyUser.MSkillCode = dr["m_skill_code"].ToString();
					armyUser.SupplyRank = dr["supply_rank"].ToString();
					armyUser.PayRemark = dr["pay_remark"].ToString();
					armyUser.PayUnitCode = dr["pay_unit_code"].ToString();
					armyUser.FinanceUnitCode = dr["finance_unit_code"].ToString();
					armyUser.MainBonus = dr["main_bonus"].ToString();
					armyUser.BonusCode = dr["bonus_code"].ToString();
					armyUser.UnPromoteCode = dr["un_promote_code"].ToString();
					armyUser.OriginalPay = dr["original_pay"].ToString();
					armyUser.ReCampaignMonth = dr["recampaign_month"].ToString();
					armyUser.EnsureRemark = dr["ensure_remark"].ToString();
					armyUser.AborigineMark = dr["aborigine_mark"].ToString();
					armyUser.BloodType = dr["blood_type"].ToString();
					armyUser.IQScore = dr["iq_score"].ToString();
					armyUser.WorkStatus = dr["work_status"].ToString();
					armyUser.CampaignDate = dr["campaign_date"].ToString();
					armyUser.CampaignSerial = dr["campaign_serial"].ToString();
					armyUser.SalaryDate = dr["salary_date"].ToString();
					armyUser.MemberName = dr["member_name"].ToString();
					armyUser.CornerCode = dr["corner_code"].ToString();
					armyUser.Birthday = dr["birthday"].ToString();
					armyUser.TransCode = dr["trans_code"].ToString();
					armyUser.UpdateDate = dr["update_date"].ToString();
					armyUser.CommonEducCode = dr["common_educ_code"].ToString();
					armyUser.MilitaryEducCode = dr["military_educ_code"].ToString();
					armyUser.SchoolCode = dr["school_code"].ToString();
					armyUser.DisciplineCode = dr["discipline_code"].ToString();
					armyUser.ClassCode = dr["class_code"].ToString();
					armyUser.VolunSoldierDate = dr["volun_soldier_date"].ToString();
					armyUser.VolunSergeantDate = dr["volun_sergeant_date"].ToString();
					armyUser.VolunOfficerDate = dr["volun_officer_date"].ToString();
					armyUser.LocalMark = dr["local_mark"].ToString();
					armyUser.AgainCampaignDate = dr["again_campaign_date"].ToString();
					armyUser.StopVolunteerDate = dr["stop_volunteer_date"].ToString();
				}
				return armyUser;
			}
			#endregion ArmyUser GetUser(string memberId)

			#region List<Rank> GetRanks()
			public List<Rank> GetRanks()
			{
				string tableName = "Rank";
				#region CommandText
				string commText = $@"
SELECT TRIM(rank_code) as rank_code, TRIM(rank_title) as rank_title 
FROM Army.dbo.{tableName} 
WHERE rank_code BETWEEN '01' AND '92' 
ORDER BY rank_code; 
";
				#endregion CommandText

				List<Rank> result = Get<Rank>(ConnectionString, commText, null);

				return result;
			}
			#endregion List<Rank> GetRanks()

			#region List<Skill> GetSkills()
			public List<Skill> GetSkills()
			{
				string tableName = "skill";
				#region CommandText
				string commText = $@"
SELECT TRIM(skill_code) as skill_code, TRIM(skill_desc) as skill_desc 
FROM Army.dbo.{tableName} 
WHERE skill_status != '0' 
";
				#endregion CommandText

				List<Skill> result = Get<Skill>(ConnectionString, commText, null);

				return result;
			}
			#endregion List<Skill> GetSkills()

			#region List<Title> GetTitles(string name)
			public List<Title> GetTitles(string name)
			{
				string tableName = "title";
				#region CommandText
				string commText = $@"
SELECT TOP 100 TRIM(title_code) as title_code, TRIM(title_name) as title_name 
FROM Army.dbo.{tableName} 
WHERE 1=1 
  AND title_name LIKE '%' + @TitleName + '%' 
";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@TitleName", SqlDbType.NVarChar));
				parameters[parameterIndex++].Value = name;

				List<Title> result = Get<Title>(ConnectionString, commText, parameters.ToArray());

				return result;
			}
			#endregion List<Title> GetTitles(string name)

			#region string CheckUserData(string userId, string name, string birthday, string email, string phone)
			public string CheckUserData(string userId, string name, string birthday, string email, string phone)
			{
				string tableName1 = "v_member_data";
				string tableName2 = "Users";
				#region CommandText
				string commText = $@"
DECLARE @IsAD BIT 
SET @IsAD = 0 
IF (SELECT LEN([Password]) FROM Users WHERE USerID = @UserId) = 0 
  BEGIN 
    SET @IsAD = 1
  END 

IF @IsAD = 0 
  BEGIN 
    SELECT COUNT(vm.member_id) 
    FROM Army.dbo.{tableName1} AS vm
      LEFT JOIN {tableName2} AS u on u.UserID = vm.member_id 
    WHERE 1=1 
      AND vm.member_id = @UserId 
      AND vm.member_name = @Name 
      AND u.Email = @Email 
      AND CONVERT(VARCHAR(8), vm.birthday, 112) = @Birthday 
      AND u.Phone = @Phone OR u.PhoneMil = @Phone 
  END 
ELSE 
  BEGIN 
    SELECT -1 
  END 
";
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

				GetDataReturnObject(ConnectionString, CommandType.Text, commText, parameters.ToArray());

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

				string tableName = "Army.dbo.v_mu_unit AS u";
				#region CommandText
				for (int i = 0; i < getlevel; i++)
				{
					sb.AppendLine($"DECLARE @Level{i} TABLE (unit_code VARCHAR(32), unit_title VARCHAR(100), ulevel_code CHAR(1), unit_status CHAR(1), parent_unit_code VARCHAR(32))");
				}

				sb.AppendLine("--第0層 ");
				sb.AppendLine("INSERT INTO @Level0 ");
				sb.AppendLine("    SELECT u.unit_code, u.unit_title, u.ulevel_code, u.unit_status, u.parent_unit_code ");
				sb.AppendLine($"	FROM {tableName} ");
				sb.AppendLine("	WHERE 1=1 ");
				sb.AppendLine("	  --AND u.unit_code = '4C68CEA7E58591B579FD074BCDAFF740' ");
				sb.AppendLine("	  AND u.unit_code = '00001' ");
				sb.AppendLine("   --AND u.unit_status != '0' ");

				sb.AppendLine("SELECT * FROM @Level0 ");

				for (int i = 1; i < getlevel; i++)
				{
					sb.AppendLine($"--第{i}層 ");
					sb.AppendLine($"INSERT INTO @Level{i} ");
					sb.AppendLine("    SELECT u.unit_code, u.unit_title, u.ulevel_code, u.unit_status, u.parent_unit_code ");
					sb.AppendLine($"	FROM {tableName} ");
					sb.AppendLine($"	  RIGHT JOIN @Level{i - 1} l{i - 1} ON l{i - 1}.unit_code = u.parent_unit_code ");
					sb.AppendLine("	WHERE 1=1 ");
					sb.AppendLine("   AND (u.unit_status = '1' ");
				    sb.AppendLine("    OR EXISTS (SELECT 1 FROM Army.dbo.v_member_data vmd WHERE vmd.unit_code = u.unit_code)) ");
					sb.AppendLine($"SELECT * FROM @Level{i} ");
				}
				#endregion CommandText

				DataSet ds = GetDataSet(ConnectionString, sb.ToString(), null);

				result = ConvertToUnits(ds.Tables);

				return result;
			}
			#endregion List<Army_Unit> GetOriginal()

			#region DataSet GetOriginalNotSorted()
			public DataSet GetOriginalNotSorted()
			{
				Army_Unit result = new Army_Unit();

				string tableName = "Army.dbo.v_mu_unit AS vm";
				#region CommandText
				string commText = $@"
DECLARE @TotalUnsorted TABLE (unit_code VARCHAR(32), unit_title VARCHAR(100), ulevel_code CHAR(1), unit_status CHAR(1), parent_unit_code VARCHAR(32))

NSERT INTO @TotalUnsorted  
   SELECT vm.unit_code, vm.unit_title, vm.ulevel_code, vm.unit_status, vm.parent_unit_code 
	FROM {tableName} 
WHERE 1=1  
  AND (vm.unit_status = '1'  
   OR EXISTS (SELECT 1 FROM Army.dbo.v_member_data vmd WHERE vmd.unit_code = vm.unit_code)) 
  AND vm.unit_code NOT IN (SELECT unit_code FROM ArmyWeb.dbo.ArmyUnits) 
{(ConfigurationManager.AppSettings.Get("UnsortedWhere").Length > 0 ? $"   {ConfigurationManager.AppSettings.Get("UnsortedWhere")} " : "")}

SELECT * FROM @TotalUnsorted ORDER BY unit_code 
";
				#endregion CommandText

				DataSet ds = GetDataSet(ConnectionString, commText, null);
				//WriteLog.Log(Newtonsoft.Json.JsonConvert.SerializeObject(ds));

                return ds;
			}
			#endregion DataSet GetOriginalNotSorted()

			#region DataTable GetUnitData(string unitCode)
			public DataTable GetUnitData(string unitCode)
			{
				Army_Unit result = new Army_Unit();
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				int getlevel = int.Parse(ConfigurationManager.AppSettings.Get("GetArmyUnitLevel"));

				string tableName = "Army.dbo.v_mu_unit AS u";
				#region CommandText

				string commText = $@"
SELECT u.start_date, u.end_date, u.unit_status, u.unit_code
FROM {tableName}
WHERE 1=1
  AND u.unit_code = @unit_code";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@unit_code", SqlDbType.Char, 5));
				parameters[parameterIndex++].Value = unitCode;

				GetDataReturnDataTable(ConnectionString, commText, parameters.ToArray());

				DataTable dt = _ResultDataTable;

				return dt;
			}
            #endregion DataTable GetUnitData(string unitCode)

            #region private Army_Unit ConvertToUnits(DataTableCollection dataTables)
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
							//currentUnit.level = level;
						}
						else
						{
							currentUnit = new Army_Unit { code = code, title = title };
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

				return rootUnit;
			}
            #endregion private Army_Unit ConvertToUnits(DataTableCollection dataTables)
        }
    }
}