using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using ArmyAPI.Commons;
using ArmyAPI.Filters;
using ArmyAPI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ArmyAPI.Services;
using System.Data.SqlClient;
using ArmyAPI.Data;

namespace ArmyAPI.Controllers
{
	public class SearchsController : BaseController
    {

		private readonly DbHelper _dbHelper;
		private readonly CodetoName _codeToName;
		public SearchsController()
		{
			_dbHelper = new DbHelper();
			_codeToName = new CodetoName();
		}


		#region ContentResult searchMember(string keyWord)
		//[CustomAuthorizationFilter]
		public ContentResult searchMember(string keyWord)
		{
			string query = @"
            SELECT 
                m.member_id, LTRIM(RTRIM(m.member_name)) asmember_name , LTRIM(RTRIM(u.unit_title)) AS unit_title, LTRIM(RTRIM(m.rank_code + ' - ' + r.rank_title)) as rank_title, LTRIM(RTRIM(m.title_code + ' - ' + t.title_name)) as title_name
            FROM 
                Army.dbo.v_member_data AS m
            JOIN 
                Army.dbo.title AS t ON m.title_code = t.title_code
            JOIN 
                Army.dbo.rank AS r ON m.rank_code = r.rank_code
            JOIN 
                Army.dbo.v_mu_unit AS u ON m.unit_code = u.unit_code
            WHERE 
                concat( m.member_id, 
                        m.member_name,
                        m.unit_code,
                        u.unit_title)
                LIKE '%' + @keyWord + '%'";

			// 使用SqlParameter防止SQL注入
			SqlParameter[] parameters = new SqlParameter[]
			{
				new SqlParameter("@keyWord", SqlDbType.VarChar) { Value = keyWord }
			};

			ApiResult result = null;
			try
			{
				// 呼叫先前定義的資料庫查詢功能
				//DataTable resultTable = _dbHelper.ArmyExecuteQuery(query, parameters);
				MsSqlDataProvider db = new MsSqlDataProvider();
				DataTable resultTable = db.GetDataTable(BaseController._ConnectionString, query, parameters);

				if (resultTable != null && resultTable.Rows.Count > 0)
				{
					result = new ApiResult(JsonConvert.SerializeObject(resultTable));
				}
			}
			catch (Exception ex)
			{
				// 處理任何可能的異常
				WriteLog.Log(String.Format("【searchMember Fail】" + ex.Message));
				result = new ApiResult("001", ", " + ex.Message);
			}

			return this.Content(JsonConvert.SerializeObject(result), "application/json");
		}
		#endregion ContentResult searchMember(string keyWord)

		#region ContentResult searchRetireMember(string keyWord)
		// 退伍人員列表
		//[CustomAuthorizationFilter]
		public ContentResult searchRetireMember(string keyWord)
		{
			string query = @"
            SELECT 
                m.member_id, LTRIM(RTRIM(m.member_name)) as member_name, LTRIM(RTRIM(u.unit_title)) AS unit_title, retire_date, LTRIM(RTRIM(m.rank_code + ' - ' + r.rank_title)) as rank_title, LTRIM(RTRIM(m.title_code + ' - ' + t.title_name)) as title_name
            FROM 
                Army.dbo.v_member_retire AS m
            LEFT JOIN 
                Army.dbo.title AS t ON m.title_code = t.title_code
            LEFT JOIN
                Army.dbo.rank AS r ON m.rank_code = r.rank_code
            LEFT JOIN
                Army.dbo.v_mu_unit AS u ON m.unit_code = u.unit_code
            WHERE 
                concat( m.member_id, 
                        m.member_name,
                        m.unit_code,
                        u.unit_title)
                LIKE '%' + @keyWord" + '%';

			// 使用SqlParameter防止SQL注入
			SqlParameter[] parameters = new SqlParameter[]
			{
				new SqlParameter("@keyWord", SqlDbType.VarChar) { Value = keyWord }
			};

			ApiResult result = null;
			try
			{
				// 呼叫先前定義的資料庫查詢功能
				DataTable resultTable = _dbHelper.ArmyExecuteQuery(query, parameters);
				resultTable.Columns.Add("retire_date_tw");
				if (resultTable != null && resultTable.Rows.Count > 0)
				{
					// TODO: 根據需要將DataTable轉換為API要回傳的物件或結構
					foreach (DataRow row in resultTable.Rows)
					{
						row["retire_date_tw"] = _codeToName.dateTimeTran(row["retire_date"].ToString().Trim(), "yyy年MM月dd日", true);
					}
					result = new ApiResult(JsonConvert.SerializeObject(resultTable));
				}
				else
				{
					result = new ApiResult(JsonConvert.SerializeObject("No records found."));
				}
			}
			catch (Exception ex)
			{
				// 處理任何可能的異常
				WriteLog.Log(String.Format("【searchRetireMember Fail】" + ex.Message));
				result = new ApiResult("002", ", " + ex.Message);
			}

			return this.Content(JsonConvert.SerializeObject(result), "application/json");
		}
		#endregion ContentResult searchRetireMember(string keyWord)
	}
}