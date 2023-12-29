using ArmyAPI.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web.Http;
using ArmyAPI.Models;
using System.Configuration;
using Microsoft.Ajax.Utilities;
using System.Reflection;
using System.Web.Http.Results;
using System.Runtime.Remoting.Channels;

namespace ArmyAPI.Controllers
{
    public class AdvancedSearchController : ApiController
    {
        private readonly DbHelper _dbHelper;
        private readonly MakeReport _makeReport;
        private readonly CodetoName _codeToName;

        public AdvancedSearchController()
        {
            _dbHelper = new DbHelper();
            _makeReport = new MakeReport();
            _codeToName = new CodetoName();
        }

        // GET: AdvancedSearch
        [HttpPost]
        [ActionName("advancedSearchMember")]
        public IHttpActionResult advancedSearchMember([FromBody]advancedSearchMemberReq keyWord)
        {
            //DataTable dataList = new DataTable();
            string query = @"
                SELECT 
                    vmd.member_id";

            //要取得的資料欄位
            for(int i = 0; i < keyWord.ColumnName.Count; i++)
            {                
                if (keyWord.ColumnName[i].ToString().Trim() == "sex")
                {
                    query += @",
                            CASE 
                                WHEN SUBSTRING(vmd.member_id, 2, 1) = '1' THEN '男'
                                WHEN SUBSTRING(vmd.member_id, 2, 1) = '2' THEN '女'
                            END AS Sex";
                }
                else
                {
                    query += ", vmd." + keyWord.ColumnName[i].ToString().Trim();
                }               
            }           

            // 欲搜尋的表和搜尋條件
            if (keyWord.Performance)
            {
                query += @" FROM 
                                Army.dbo.v_member_data as vmd
                            LEFT JOIN
                                Army.dbo.v_mu_unit as vmu on vmu.unit_code = vmd.unit_code
                            LEFT JOIN
                                Army.dbo.v_performance as vp on vmd.member_id = vp.member_id
                            ";
            }
            else
            {
                query += @" FROM 
                                Army.dbo.v_member_data as vmd
                            LEFT JOIN
                                Army.dbo.v_mu_unit as vmu on vmu.unit_code = vmd.unit_code
                            ";
            }
            
            
            // 搜尋的詳細條件
            for(int j = 0; j < keyWord.SearchData.Count; j++) 
            {
                advSearchConditionReq Condition = keyWord.SearchData[j];
                if (j == 0)
                {
                    query += " WHERE ";
                }
                else
                {
                    query += " AND ";
                }           

                switch (Condition.ConditionName.ToString())
                {
                    case "sex":
                        string sexNum = "0";
                        if (Condition.ConditionValue[0].ToString() == "男")
                        {
                            sexNum = "1";
                        }
                        else if(Condition.ConditionValue[0].ToString() == "女")
                        {
                            sexNum = "2";
                        }
                        query += "SUBSTRING(vmd.member_id, 2, 1) = " + sexNum;
                        break;
                    case "pay_date":
                    case "rank_date":
                    case "salary_date":
                        query += "vmd." + Condition.ConditionName.ToString() + " BETWEEN '" + Condition.ConditionValue[0].ToString() + "' AND '" + Condition.ConditionValue[1].ToString() + "'";
                        break;
                    case "service_code":
                        query += "left(vmd.unit_code,1) between '1'and '3'";
                        break;
                    case "service_rank":
                        query += "left(vmd.service_code,1) not between '2'and '4'";
                        break;
                    default:
                        switch (Condition.ConditionName.ToString().Trim())
                        {
                            case "perform_code":
                            case "p_year":
                                query += "vp.";
                                break;
                            default:
                                query += "vmd.";
                                break;
                        }
                        
                        query += Condition.ConditionName.ToString() + " in (";
                        for (int i = 0; i < Condition.ConditionValue.Count; i++) 
                        {
                            if (i != 0)
                            {
                                query += ",";
                            }                            
                            query += "'" + Condition.ConditionValue[i].ToString() + "'";
                        }
                        query += ")";
                        break;
                }
            }
            query += " ORDER BY vmu.unit_code";
                        
            try
            {
                // 呼叫先前定義的資料庫查詢功能
                DataTable resultTable = _dbHelper.ArmyWebExecuteQuery(query);
                keyWord.ColumnName.Add("member_id");
                if (resultTable != null && resultTable.Rows.Count > 0)
                {
                    
                    DataTable finalTB = new DataTable();
                    // TODO: 根據需要將DataTable轉換為API要回傳的物件或結構

                    DataTable newTable = resultTable.Clone();
                    finalTB = _codeToName.Transformer(resultTable, keyWord.ColumnName);                    
                    return Ok(new { Result = "Success", Data = finalTB });
                }
                else
                {
                    return Ok(new { Result = "Fail", Message = "No records found." });
                }
            }
            catch (Exception ex)
            {
                // 處理任何可能的異常
                WriteLog.Log(String.Format("【advancedSearchMember Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【advancedSearchMember Fail】" + ex.ToString());
            }
        }

        // 限定現階/編階
        [HttpGet]
        [ActionName("advRank")]
        public IHttpActionResult advRank()
        {
            try
            {
                List<advancedCodeRes> advList = new List<advancedCodeRes>();
                string Sql = @"
                            SELECT 
                                LTRIM(RTRIM(rank_code)) as Code, LTRIM(RTRIM(rank_title)) as Name, LTRIM(RTRIM(rank_code + '-' + rank_title)) as CodeName
                            FROM
                                Army.dbo.rank";                       
                
                DataTable TB = _dbHelper.ArmyWebExecuteQuery(Sql);
                if (TB != null && TB.Rows.Count != 0)
                {                    
                    return Ok(new { Result = "Success", Rank = TB });
                }
                return Ok(new { Result = "Fail", Rank = TB });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【advRank Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【advRank Fail】" + ex.ToString());
            }
        }

        // 限定役別
        [HttpGet]
        [ActionName("advCampaign")]
        public IHttpActionResult advCampaign()
        {
            try
            {
                List<advancedCodeRes> advList = new List<advancedCodeRes>();
                string Sql = @"
                            SELECT 
                                LTRIM(RTRIM(campaign_code)) as Code, LTRIM(RTRIM(campaign_desc)) as Name, LTRIM(RTRIM(campaign_code + '-' + campaign_desc)) as CodeName
                            FROM
                                Army.dbo.memb_campaign_code";

                DataTable TB = _dbHelper.ArmyWebExecuteQuery(Sql);
                if (TB != null && TB.Rows.Count != 0)
                {                    
                    return Ok(new { Result = "Success", Campaign = TB });
                }
                return Ok(new { Result = "Fail", Campaign = TB });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【advCampaign Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【advCampaign Fail】" + ex.ToString());
            }
        }

        // 限定單位
        [HttpGet]
        [ActionName("advUnit")]
        public IHttpActionResult advUnit(string keyWord)
        {
            try
            {
                List<advancedCodeRes> advList = new List<advancedCodeRes>();
                string Sql = @"
                            SELECT 
                                LTRIM(RTRIM(unit_code)) as Code, LTRIM(RTRIM(unit_title)) as Name, LTRIM(RTRIM(unit_code + '-' + unit_title)) as CodeName
                            FROM
                                Army.dbo.v_mu_unit
                            WHERE
                                concat(unit_code, unit_title)
                            LIKE
                                @keyWord";

                SqlParameter[] sqlPara = { new SqlParameter("@keyWord", SqlDbType.VarChar) { Value = "%" + keyWord + "%" } };

                DataTable TB = _dbHelper.ArmyWebExecuteQuery(Sql, sqlPara);
                if (TB != null && TB.Rows.Count != 0)
                {
                    return Ok(new { Result = "Success", Unit = TB });
                }
                return Ok(new { Result = "Fail", Unit = TB });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【advUnit Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【advUnit Fail】" + ex.ToString());
            }
        }

        // 限定個人專長/編制專長
        [HttpGet]
        [ActionName("advSkill")]
        public IHttpActionResult advSkill(string keyWord)
        {
            try
            {
                List<advancedCodeRes> advList = new List<advancedCodeRes>();
                string Sql = @"
                            SELECT 
                                LTRIM(RTRIM(skill_code)) as Code, LTRIM(RTRIM(skill_desc)) as Name, LTRIM(RTRIM(skill_code + '-' + skill_desc)) as CodeName
                            FROM
                                Army.dbo.skill
                            Where
                                concat(skill_code, skill_desc) like @keyWord";
                SqlParameter[] sqlPara = { new SqlParameter("@keyWord", SqlDbType.VarChar){Value = "%" + keyWord + "%"  } };

                DataTable TB = _dbHelper.ArmyWebExecuteQuery(Sql, sqlPara);
                if (TB != null && TB.Rows.Count != 0)
                {
                    return Ok(new { Result = "Success", Skill = TB });
                }
                return Ok(new { Result = "Fail", Skill = TB });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【advSkill Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【advSkill Fail】" + ex.ToString());
            }
        }

        

        // 限定官科
        [HttpGet]
        [ActionName("advGroup")]
        public IHttpActionResult advGroup()
        {
            try
            {
                List<advancedCodeRes> advList = new List<advancedCodeRes>();
                string Sql = @"
                            SELECT 
                                LTRIM(RTRIM(group_code)) as Code, LTRIM(RTRIM(group_title)) as Name, LTRIM(RTRIM(group_code + '-' + group_title)) as CodeName
                            FROM
                                Army.dbo.tgroup";                

                DataTable TB = _dbHelper.ArmyWebExecuteQuery(Sql);
                if (TB != null && TB.Rows.Count != 0)
                {
                    return Ok(new { Result = "Success", Group = TB });
                }
                return Ok(new { Result = "Fail", Group = TB });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【advGroup Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【advGroup Fail】" + ex.ToString());
            }
        }

        //限定最高軍事教育

        // 限定最高教育/學位
        [HttpGet]
        [ActionName("advEduc")]
        public IHttpActionResult advEduc()
        {
            try
            {
                List<advancedCodeRes> advList = new List<advancedCodeRes>();
                string Sql = @"
                            SELECT 
                                LTRIM(RTRIM(educ_code)) as Code, LTRIM(RTRIM(educ_name)) as Name, LTRIM(RTRIM(educ_code + '-' + educ_name)) as CodeName
                            FROM
                                Army.dbo.educ_code";

                DataTable TB = _dbHelper.ArmyWebExecuteQuery(Sql);
                if (TB != null && TB.Rows.Count != 0)
                {
                    return Ok(new { Result = "Success", Educ = TB });
                }
                return Ok(new { Result = "Fail", Educ = TB });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【advEduc Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【advEduc Fail】" + ex.ToString());
            }
        }

        // 限定職稱
        [HttpGet]
        [ActionName("advTitle")]
        public IHttpActionResult advTitle(string keyWord)
        {
            try
            {
                List<advancedCodeRes> advList = new List<advancedCodeRes>();
                string Sql = @"
                            SELECT 
                                LTRIM(RTRIM(title_code)) as Code, LTRIM(RTRIM(title_name)) as Name, LTRIM(RTRIM(title_code + '-' + title_name)) as CodeName
                            FROM
                                Army.dbo.title
                            Where
                                concat(title_code, title_name) like @keyWord";
                SqlParameter[] sqlPara = { new SqlParameter("@keyWord", SqlDbType.VarChar) { Value = "%" + keyWord + "%" } };

                DataTable TB = _dbHelper.ArmyWebExecuteQuery(Sql, sqlPara);
                if (TB != null && TB.Rows.Count != 0)
                {
                    return Ok(new { Result = "Success", Title = TB });
                }
                return Ok(new { Result = "Fail", Title = TB });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【advTitle Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【advTitle Fail】" + ex.ToString());
            }
        }

        // 限定考績
        [HttpGet]
        [ActionName("advPerformance")]
        public IHttpActionResult advPerformance()
        {
            try
            {
                List<advancedCodeRes> advList = new List<advancedCodeRes>();
                string Sql = @"
                            SELECT 
                                LTRIM(RTRIM(perform_code)) as Code, LTRIM(RTRIM(perform_name)) as Name, LTRIM(RTRIM(perform_code + '-' + perform_name)) as CodeName
                            FROM
                                Army.dbo.perf_code";                

                DataTable TB = _dbHelper.ArmyWebExecuteQuery(Sql);
                if (TB != null && TB.Rows.Count != 0)
                {
                    return Ok(new { Result = "Success", Performance = TB });
                }
                return Ok(new { Result = "Fail", Performance = TB });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【advPerformance Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【advPerformance Fail】" + ex.ToString());
            }
        }

        // 限定學校
        [HttpGet]
        [ActionName("advSchool")]
        public IHttpActionResult advSchool(string keyWord)
        {
            try
            {
                List<advancedCodeRes> advList = new List<advancedCodeRes>();
                string Sql = @"
                            SELECT 
                                LTRIM(RTRIM(school_code)) as Code, LTRIM(RTRIM(school_desc)) as Name, LTRIM(RTRIM(school_code + '-' + school_desc)) as CodeName
                            FROM
                                Army.dbo.educ_school
                            Where
                                concat(school_code, school_desc) like @keyWord";
                SqlParameter[] sqlPara = { new SqlParameter("@keyWord", SqlDbType.VarChar) { Value = "%" + keyWord + "%" } };

                DataTable TB = _dbHelper.ArmyWebExecuteQuery(Sql, sqlPara);
                if (TB != null && TB.Rows.Count != 0)
                {
                    return Ok(new { Result = "Success", School = TB });
                }
                return Ok(new { Result = "Fail", School = TB });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【advSchool Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【advSchool Fail】" + ex.ToString());
            }
        }


        //匯出excel
        [HttpPost]
        [ActionName("advExport")]
        public IHttpActionResult advExport([FromBody] advExcelDataReq excelData)
        {
            try
            {
                List<GeneralReq> generalReq = new List<GeneralReq>();
                
                string dateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

                string excelName = "~/Report/" + dateTime + "_進階查詢.xlsx";

                string excelOutputPath = System.Web.Hosting.HostingEnvironment.MapPath(excelName);
				string urlPath = Request.RequestUri.GetLeftPart(UriPartial.Authority) + $"/{ConfigurationManager.AppSettings.Get("ApiPath")}/Report/";
                
                string excelHttpPath = string.Empty;
                
                bool excelResult = _makeReport.exportAdvSearchExcel(excelData, excelOutputPath);

                if (!excelResult)
                {
                    return Ok( new { Result = "Fail",  excelPath = excelHttpPath });
                }

                
                DataTable generalTB = new DataTable();
                
                int total = excelData.MemberId.Count;
                int batchSize = 1000;
                int numberOfBatches = (int)Math.Ceiling((double)total / batchSize);
                

                for (int i = 0; i < numberOfBatches; i++)
                {
                    var batch = excelData.MemberId.Skip(i * batchSize).Take(batchSize).ToList();
                    string generalSql = @"SELECT
                                        member_id, member_name, rank_code
                                      FROM
                                        Army.dbo.v_member_data
                                      WHERE
                                        member_id in (";
                    SqlParameter[] generalPara = new SqlParameter[batch.Count];
                    for (int j = 0; j < batch.Count; j++)
                    {
                        if (j != 0)
                        {
                            generalSql += ",";
                        }
                        generalSql += "@value" + j;
                        generalPara[j] = new SqlParameter("@value" + j, SqlDbType.VarChar) { Value = batch[j] };
                    }

                    generalSql += ")";
                    DataTable batchTB = _dbHelper.ArmyWebExecuteQuery(generalSql, generalPara);
                    generalTB.Merge(batchTB); 
                }                

                if(generalTB != null && generalTB.Rows.Count != 0) 
                {
                    foreach (DataRow row in generalTB.Rows) 
                    {
                        int rank = int.Parse(row["rank_code"].ToString());
                        if (rank > 0 && rank <= 23)
                        {
                            GeneralReq generalRecord = new GeneralReq
                            {
                                GeneralId = row["member_id"].ToString(),

                                GeneralName = row["member_name"].ToString(),

                                GeneralRank = row["rank_code"].ToString()
                            };
                            generalReq.Add(generalRecord);
                        }
                    }
                }

                excelName = dateTime + "_進階查詢.xlsx";
                excelHttpPath = urlPath + excelName;
                _makeReport.checkGeneral(generalReq, excelData.UserId, excelName, "進階查詢下載");

                return Ok(new { Result = "Success", excelPath = excelHttpPath });
            }
            catch (Exception ex) 
            {
                WriteLog.Log(String.Format("【advExport Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【advExport Fail】" + ex.ToString());
            }
            
        }

    }
}
