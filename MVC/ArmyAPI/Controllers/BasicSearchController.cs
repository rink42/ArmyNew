using ArmyAPI.Models;
using ArmyAPI.Services;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Data.SqlClient;
using System.Data;
using NPOI.Util;


namespace ArmyAPI.Controllers
{
    public class BasicSearchController : ApiController
    {
        private readonly DbHelper _dbHelper;
        private readonly CodetoName _codeToName;
        public BasicSearchController()
        {
            _dbHelper = new DbHelper();
            _codeToName = new CodetoName();
        }

        // 現員列表        
		[HttpGet]
        [ActionName("searchMember")]

		public IHttpActionResult searchMember(string keyWord)
        {
            
            string unitSql = @"
            SELECT 
                m.unit_code, 
                LTRIM(RTRIM(u.unit_title)) AS unit_title
            FROM 
                Army.dbo.v_member_data AS m            
            LEFT JOIN 
                Army.dbo.v_mu_unit AS u ON m.unit_code = u.unit_code
            WHERE 
                CONCAT(m.unit_code, u.unit_title) LIKE @keyWord
            GROUP BY				
                m.unit_code, 
                LTRIM(RTRIM(u.unit_title))
            ORDER BY                
                m.unit_code";

            string memberSql = @"
            SELECT 
                m.member_id, m.member_name, LTRIM(RTRIM(vmu.item_no)) as item_no, LTRIM(RTRIM(m.unit_code + '-' + u.unit_title)) as unit_title, LTRIM(RTRIM(m.rank_code + ' - ' + r.rank_title)) as rank_title, LTRIM(RTRIM(m.title_code + ' - ' + t.title_name)) as title_name, m.salary_date
            FROM 
                Army.dbo.v_member_data AS m
            LEFT JOIN 
	            Army.dbo.v_item_name_unit as vmu on vmu.unit_code = m.unit_code and vmu.item_no = m.item_no --組別
            LEFT JOIN 
                Army.dbo.title AS t ON m.title_code = t.title_code
            LEFT JOIN 
                Army.dbo.rank AS r ON m.rank_code = r.rank_code
            LEFT JOIN 
                Army.dbo.v_mu_unit AS u ON m.unit_code = u.unit_code
            WHERE 
                m.member_name LIKE @keyWord
                OR m.member_id = @idKeyWord                
            ORDER BY
                CASE WHEN vmu.item_no IS NULL THEN 1 ELSE 0 END,
                m.unit_code,                
                m.rank_code,
                m.member_id,
                m.title_code";

            // 使用SqlParameter防止SQL注入
            SqlParameter[] unitPara = new SqlParameter[]
            {
                new SqlParameter("@keyWord", SqlDbType.VarChar) { Value = "%" + keyWord + "%" }                
            };

            SqlParameter[] memberPara = new SqlParameter[]
            {
                new SqlParameter("@keyWord", SqlDbType.VarChar) { Value = "%" + keyWord + "%" },
                new SqlParameter("@idKeyWord", SqlDbType.VarChar) {Value = keyWord}
            };

            try
            {
                // 呼叫先前定義的資料庫查詢功能
                DataTable unitTB = _dbHelper.ArmyWebExecuteQuery(unitSql, unitPara);

                if (unitTB != null && unitTB.Rows.Count > 0)
                {
                    return Ok(new { Result = "Success", isMemberList = false, Data = unitTB });
                }
                else
                {
                    DataTable memberTB = _dbHelper.ArmyWebExecuteQuery(memberSql, memberPara);
                    if (memberTB != null && memberTB.Rows.Count > 0) 
                    {
                        memberTB.Columns.Add("salary_date_tw");
                        foreach (DataRow row in memberTB.Rows)
                        {
                            row["salary_date_tw"] = _codeToName.dateTimeTran(row["salary_date"].ToString(), "yyy年MM月dd日", true);
                        }
                        return Ok(new { Result = "Success", isMemberList = true, Data = memberTB });
                    }
                    else
                    {
                        return Ok(new { Result = "No member found", isMemberList = false, Data = memberTB });
                    }
                }
            }
            catch (Exception ex)
            {
                // 處理任何可能的異常
                WriteLog.Log(String.Format("【searchMember Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【searchMember Fail】" + ex.Message);
            }
        }

        // 退伍人員列表
        [HttpGet]
        [ActionName("searchRetireMember")]
        public IHttpActionResult searchRetireMember(string keyWord)
        {
            string unitSql = @"
            SELECT 
                m.unit_code, 
                LTRIM(RTRIM(u.unit_title)) AS unit_title
            FROM 
                Army.dbo.v_member_retire AS m            
            LEFT JOIN 
                Army.dbo.v_mu_unit AS u ON m.unit_code = u.unit_code
            WHERE 
                CONCAT(m.unit_code, u.unit_title) LIKE @keyWord
            GROUP BY				
                m.unit_code, 
                LTRIM(RTRIM(u.unit_title))
            ORDER BY                
                m.unit_code";

            string memberSql = @"
            SELECT 
                m.member_id, m.member_name, LTRIM(RTRIM(vmu.item_no)) as item_no, LTRIM(RTRIM(m.unit_code + '-' + u.unit_title)) as unit_title, m.retire_date, LTRIM(RTRIM(m.rank_code + ' - ' + r.rank_title)) as rank_title, LTRIM(RTRIM(m.title_code + ' - ' + t.title_name)) as title_name
            FROM 
                Army.dbo.v_member_retire AS m
            LEFT JOIN 
	            Army.dbo.v_item_name_unit as vmu on vmu.unit_code = m.unit_code and vmu.item_no = m.item_no --組別
            LEFT JOIN 
                Army.dbo.title AS t ON m.title_code = t.title_code
            LEFT JOIN
                Army.dbo.rank AS r ON m.rank_code = r.rank_code
            LEFT JOIN
                Army.dbo.v_mu_unit AS u ON m.unit_code = u.unit_code
            WHERE 
                m.member_name LIKE @keyWord
                OR m.member_id = @idKeyWord
            ORDER BY
               CASE WHEN vmu.item_no IS NULL THEN 1 ELSE 0 END,
               m.unit_code,               
               m.rank_code,
               m.member_id,
               m.title_code";

            // 使用SqlParameter防止SQL注入
            SqlParameter[] unitPara = new SqlParameter[]
            {
                new SqlParameter("@keyWord", SqlDbType.VarChar) { Value = "%" + keyWord + "%" }
            };

            SqlParameter[] memberPara = new SqlParameter[]
            {
                new SqlParameter("@keyWord", SqlDbType.VarChar) { Value = "%" + keyWord + "%" },
                new SqlParameter("@idKeyWord", SqlDbType.VarChar) {Value = keyWord}
            };

            try
            {
                // 呼叫先前定義的資料庫查詢功能
                DataTable unitTB = _dbHelper.ArmyWebExecuteQuery(unitSql, unitPara);

                if (unitTB != null && unitTB.Rows.Count > 0)
                {
                    return Ok(new { Result = "Success", isMemberList = false, Data = unitTB });
                }
                else
                {
                    DataTable memberTB = _dbHelper.ArmyWebExecuteQuery(memberSql, memberPara);
                    if (memberTB != null && memberTB.Rows.Count > 0)
                    {
                        memberTB.Columns.Add("retire_date_tw");
                        foreach (DataRow row in memberTB.Rows)
                        {
                            row["retire_date_tw"] = _codeToName.dateTimeTran(row["retire_date"].ToString().Trim(), "yyy年MM月dd日", true);
                        }
                        return Ok(new { Result = "Success", isMemberList = true, Data = memberTB });
                    }
                    else
                    {
                        return Ok(new { Result = "No member found", isMemberList = false, Data = memberTB });
                    }
                }
            }
            catch (Exception ex)
            {
                // 處理任何可能的異常
                WriteLog.Log(String.Format("【searchRetireMember Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【searchRetireMember Fail】" + ex.Message);
            }
        }

        // 未到人員列表
        [HttpGet]
        [ActionName("searchRelayMember")]
        public IHttpActionResult searchRelayMember(string keyWord)
        {
            string unitSql = @"
            SELECT 
                m.unit_code, 
                LTRIM(RTRIM(u.unit_title)) AS unit_title
            FROM 
                Army.dbo.v_member_relay AS m         
            LEFT JOIN 
                Army.dbo.v_mu_unit AS u ON m.unit_code = u.unit_code
            WHERE 
                CONCAT(m.unit_code, u.unit_title) LIKE @keyWord
            GROUP BY				
                m.unit_code, 
                LTRIM(RTRIM(u.unit_title))
            ORDER BY                
                m.unit_code";

            string memberSql = @"
            SELECT 
                m.member_id, m.member_name, LTRIM(RTRIM(vmu.item_no)) as item_no, LTRIM(RTRIM(m.unit_code + '-' + u.unit_title)) as unit_title, LTRIM(RTRIM(m.rank_code + ' - ' + r.rank_title)) as rank_title, LTRIM(RTRIM(m.title_code + ' - ' + t.title_name)) as title_name, m.salary_date
            FROM 
                Army.dbo.v_member_relay AS m
            LEFT JOIN 
	            Army.dbo.v_item_name_unit as vmu on vmu.unit_code = m.unit_code and vmu.item_no = m.item_no --組別
            LEFT JOIN 
                Army.dbo.title AS t ON m.title_code = t.title_code
            LEFT JOIN 
                Army.dbo.rank AS r ON m.rank_code = r.rank_code
            LEFT JOIN 
                Army.dbo.v_mu_unit AS u ON m.unit_code = u.unit_code
            WHERE 
                 m.member_name LIKE @keyWord
                OR m.member_id = @idKeyWord
            ORDER BY
                CASE WHEN vmu.item_no IS NULL THEN 1 ELSE 0 END,
                m.unit_code,
                m.rank_code,
                m.member_id,
                m.title_code";

            // 使用SqlParameter防止SQL注入
            SqlParameter[] unitPara = new SqlParameter[]
            {
                new SqlParameter("@keyWord", SqlDbType.VarChar) { Value = "%" + keyWord + "%" }
            };

            SqlParameter[] memberPara = new SqlParameter[]
            {
                new SqlParameter("@keyWord", SqlDbType.VarChar) { Value = "%" + keyWord + "%" },
                new SqlParameter("@idKeyWord", SqlDbType.VarChar) {Value = keyWord}
            };

            try
            {
                // 呼叫先前定義的資料庫查詢功能
                DataTable unitTB = _dbHelper.ArmyWebExecuteQuery(unitSql, unitPara);

                if (unitTB != null && unitTB.Rows.Count > 0)
                {
                    return Ok(new { Result = "Success", isMemberList = false, Data = unitTB });
                }
                else
                {
                    DataTable memberTB = _dbHelper.ArmyWebExecuteQuery(memberSql, memberPara);
                    if (memberTB != null && memberTB.Rows.Count > 0)
                    {
                        memberTB.Columns.Add("salary_date_tw");
                        foreach (DataRow row in memberTB.Rows)
                        {
                            row["salary_date_tw"] = _codeToName.dateTimeTran(row["salary_date"].ToString(), "yyy年MM月dd日", true);
                        }
                        return Ok(new { Result = "Success", isMemberList = true, Data = memberTB });
                    }
                    else
                    {
                        return Ok(new { Result = "No member found", isMemberList = false, Data = memberTB });
                    }
                }
            }
            catch (Exception ex)
            {
                // 處理任何可能的異常
                WriteLog.Log(String.Format("【searchRelayMember Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【searchRelayMember Fail】" + ex.Message);
            }
        }
        
        /*
        // 現員列表
        [HttpGet]
        [ActionName("searchMember")]
        public IHttpActionResult searchMember(string keyWord)
        {

            string query = @"
            SELECT 
                m.member_id, m.member_name, LTRIM(RTRIM(vmu.item_no)) as item_no, LTRIM(RTRIM(u.unit_title)) as unit_title, LTRIM(RTRIM(m.rank_code + ' - ' + r.rank_title)) as rank_title, LTRIM(RTRIM(m.title_code + ' - ' + t.title_name)) as title_name, m.salary_date
            FROM 
                Army.dbo.v_member_data AS m
            LEFT JOIN 
	            Army.dbo.v_item_name_unit as vmu on vmu.unit_code = m.unit_code and vmu.item_no = m.item_no --組別
            LEFT JOIN 
                Army.dbo.title AS t ON m.title_code = t.title_code
            LEFT JOIN 
                Army.dbo.rank AS r ON m.rank_code = r.rank_code
            LEFT JOIN 
                Army.dbo.v_mu_unit AS u ON m.unit_code = u.unit_code
            WHERE 
                concat( m.member_name,
                        m.unit_code,
                        u.unit_title)
                LIKE @keyWord
                OR m.member_id = @idKeyWord
            ORDER BY
                CASE WHEN vmu.item_no IS NULL THEN 1 ELSE 0 END,
                m.unit_code,                
                m.rank_code,
                m.member_id,
                m.title_code";

            // 使用SqlParameter防止SQL注入
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@keyWord", SqlDbType.VarChar) { Value = "%" + keyWord + "%" },
                new SqlParameter("@idKeyWord", SqlDbType.VarChar) {Value = keyWord}
            };

            try
            {
                // 呼叫先前定義的資料庫查詢功能
                DataTable resultTable = _dbHelper.ArmyWebExecuteQuery(query, parameters);

                if (resultTable != null && resultTable.Rows.Count > 0)
                {
                    resultTable.Columns.Add("salary_date_tw");
                    foreach (DataRow row in resultTable.Rows)
                    {
                        row["salary_date_tw"] = _codeToName.dateTimeTran(row["salary_date"].ToString(), "yyy年MM月dd日", true);
                    }
                    return Ok(new { Result = "Success", Data = resultTable });
                }
                else
                {
                    return Ok(new { Result = "No member found", Data = resultTable });
                }
            }
            catch (Exception ex)
            {
                // 處理任何可能的異常
                WriteLog.Log(String.Format("【searchMember Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【searchMember Fail】" + ex.Message);
            }
        }

        // 退伍人員列表
        [HttpGet]
        [ActionName("searchRetireMember")]
        public IHttpActionResult searchRetireMember(string keyWord)
        {
            string query = @"
            SELECT 
                m.member_id, m.member_name, LTRIM(RTRIM(vmu.item_no)) as item_no, LTRIM(RTRIM(m.unit_code + '-' + u.unit_title)) as unit_title, m.retire_date, LTRIM(RTRIM(m.rank_code + ' - ' + r.rank_title)) as rank_title, LTRIM(RTRIM(m.title_code + ' - ' + t.title_name)) as title_name
            FROM 
                Army.dbo.v_member_retire AS m
            LEFT JOIN 
	            Army.dbo.v_item_name_unit as vmu on vmu.unit_code = m.unit_code and vmu.item_no = m.item_no --組別
            LEFT JOIN 
                Army.dbo.title AS t ON m.title_code = t.title_code
            LEFT JOIN
                Army.dbo.rank AS r ON m.rank_code = r.rank_code
            LEFT JOIN
                Army.dbo.v_mu_unit AS u ON m.unit_code = u.unit_code
            WHERE 
                concat( m.member_name,
                        m.unit_code,
                        u.unit_title)
                LIKE @keyWord
                OR m.member_id = @idKeyWord
            ORDER BY
               CASE WHEN vmu.item_no IS NULL THEN 1 ELSE 0 END,
               m.unit_code,               
               m.rank_code,
               m.member_id,
               m.title_code";

            // 使用SqlParameter防止SQL注入
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@keyWord", SqlDbType.VarChar) { Value = "%" + keyWord + "%" },
                new SqlParameter("@idKeyWord", SqlDbType.VarChar) {Value = keyWord}
            };

            try
            {
                // 呼叫先前定義的資料庫查詢功能
                DataTable resultTable = _dbHelper.ArmyWebExecuteQuery(query, parameters);

                if (resultTable != null && resultTable.Rows.Count > 0)
                {
                    resultTable.Columns.Add("retire_date_tw");
                    // TODO: 根據需要將DataTable轉換為API要回傳的物件或結構
                    foreach (DataRow row in resultTable.Rows)
                    {
                        row["retire_date_tw"] = _codeToName.dateTimeTran(row["retire_date"].ToString().Trim(), "yyy年MM月dd日", true);
                    }
                    return Ok(new { Result = "Success", Data = resultTable });
                }
                else
                {
                    return Ok(new { Result = "Success", Message = "No records found." , Data = resultTable});
                }
            }
            catch (Exception ex)
            {
                // 處理任何可能的異常
                WriteLog.Log(String.Format("【searchRetireMember Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【searchRetireMember Fail】" + ex.Message);
            }
        }

        // 未到人員列表
        [HttpGet]
        [ActionName("searchRelayMember")]
        public IHttpActionResult searchRelayMember(string keyWord)
        {
            string query = @"
            SELECT 
                m.member_id, m.member_name, LTRIM(RTRIM(vmu.item_no)) as item_no, LTRIM(RTRIM(u.unit_title)) as unit_title, LTRIM(RTRIM(m.rank_code + ' - ' + r.rank_title)) as rank_title, LTRIM(RTRIM(m.title_code + ' - ' + t.title_name)) as title_name, m.salary_date
            FROM 
                Army.dbo.v_member_relay AS m
            LEFT JOIN 
	            Army.dbo.v_item_name_unit as vmu on vmu.unit_code = m.unit_code and vmu.item_no = m.item_no --組別
            LEFT JOIN 
                Army.dbo.title AS t ON m.title_code = t.title_code
            LEFT JOIN 
                Army.dbo.rank AS r ON m.rank_code = r.rank_code
            LEFT JOIN 
                Army.dbo.v_mu_unit AS u ON m.unit_code = u.unit_code
            WHERE 
                concat( m.member_name,
                        m.unit_code,
                        u.unit_title)
                LIKE @keyWord
                OR m.member_id = @idKeyWord
            ORDER BY
                CASE WHEN vmu.item_no IS NULL THEN 1 ELSE 0 END,
                m.unit_code,
                m.rank_code,
                m.member_id,
                m.title_code";

            // 使用SqlParameter防止SQL注入
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@keyWord", SqlDbType.VarChar) { Value = "%" + keyWord + "%" },
                new SqlParameter("@idKeyWord", SqlDbType.VarChar) {Value = keyWord}
            };

            try
            {
                // 呼叫先前定義的資料庫查詢功能
                DataTable resultTable = _dbHelper.ArmyWebExecuteQuery(query, parameters);
                resultTable.Columns.Add("salary_date_tw");

                if (resultTable != null && resultTable.Rows.Count > 0)
                {
                    // TODO: 根據需要將DataTable轉換為API要回傳的物件或結構
                    foreach (DataRow row in resultTable.Rows)
                    {
                        row["salary_date_tw"] = _codeToName.dateTimeTran(row["salary_date"].ToString().Trim(), "yyy年MM月dd日", true);
                    }
                    return Ok(new { Result = "Success", Data = resultTable });
                }
                else
                {
                    return Ok(new { Result = "Success", Message = "No records found." , Data = resultTable});
                }
            }
            catch (Exception ex)
            {
                // 處理任何可能的異常
                WriteLog.Log(String.Format("【searchRelayMember Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【searchRelayMember Fail】" + ex.Message);
            }
        }

        */
        // 單位成員查詢
        [HttpGet]
        [ActionName("unitMember")]
        public IHttpActionResult unitMember(string unitCode)
        {
            string memberSql = @"
            SELECT 
                m.member_id, m.member_name, LTRIM(RTRIM(vmu.item_no)) as item_no, LTRIM(RTRIM(m.unit_code + '-' + u.unit_title)) as unit_title, LTRIM(RTRIM(m.rank_code + ' - ' + r.rank_title)) as rank_title, LTRIM(RTRIM(m.title_code + ' - ' + t.title_name)) as title_name, m.salary_date
            FROM 
                Army.dbo.v_member_data AS m
            LEFT JOIN 
	            Army.dbo.v_item_name_unit as vmu on vmu.unit_code = m.unit_code and vmu.item_no = m.item_no --組別
            LEFT JOIN 
                Army.dbo.title AS t ON m.title_code = t.title_code
            LEFT JOIN 
                Army.dbo.rank AS r ON m.rank_code = r.rank_code
            LEFT JOIN 
                Army.dbo.v_mu_unit AS u ON m.unit_code = u.unit_code
            WHERE 
                m.unit_code = @unitCode               
            ORDER BY
                CASE WHEN vmu.item_no IS NULL THEN 1 ELSE 0 END,
                m.unit_code,                
                m.rank_code,
                m.member_id,
                m.title_code";

            // 使用SqlParameter防止SQL注入
            SqlParameter[] memberPara = new SqlParameter[]
            {
                new SqlParameter("@unitCode", SqlDbType.VarChar) { Value = unitCode }
            };

            try
            {
                // 呼叫先前定義的資料庫查詢功能
                DataTable memberTB = _dbHelper.ArmyWebExecuteQuery(memberSql, memberPara);
                if (memberTB != null && memberTB.Rows.Count > 0)
                {
                    memberTB.Columns.Add("salary_date_tw");
                    foreach (DataRow row in memberTB.Rows)
                    {
                        row["salary_date_tw"] = _codeToName.dateTimeTran(row["salary_date"].ToString(), "yyy年MM月dd日", true);
                    }
                    return Ok(new { Result = "Success", isMemberList = true, Data = memberTB });
                }
                else
                {
                    return Ok(new { Result = "No member found", isMemberList = false, Data = memberTB });
                }
            }
            catch (Exception ex)
            {
                // 處理任何可能的異常
                WriteLog.Log(String.Format("【unitMember Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【unitMember Fail】" + ex.Message);
            }
        }

        // 單位退休成員查詢
        [HttpGet]
        [ActionName("unitRetireMember")]
        public IHttpActionResult unitRetireMember(string unitCode)
        {
            string memberSql = @"
           SELECT 
                m.member_id, m.member_name, LTRIM(RTRIM(vmu.item_no)) as item_no, LTRIM(RTRIM(m.unit_code + '-' + u.unit_title)) as unit_title, m.retire_date, LTRIM(RTRIM(m.rank_code + ' - ' + r.rank_title)) as rank_title, LTRIM(RTRIM(m.title_code + ' - ' + t.title_name)) as title_name
            FROM 
                Army.dbo.v_member_retire AS m
            LEFT JOIN 
	            Army.dbo.v_item_name_unit as vmu on vmu.unit_code = m.unit_code and vmu.item_no = m.item_no --組別
            LEFT JOIN 
                Army.dbo.title AS t ON m.title_code = t.title_code
            LEFT JOIN
                Army.dbo.rank AS r ON m.rank_code = r.rank_code
            LEFT JOIN
                Army.dbo.v_mu_unit AS u ON m.unit_code = u.unit_code
            WHERE 
                m.unit_code = @unitCode
            ORDER BY
               CASE WHEN vmu.item_no IS NULL THEN 1 ELSE 0 END,
               m.unit_code,               
               m.rank_code,
               m.member_id,
               m.title_code";

            // 使用SqlParameter防止SQL注入
            SqlParameter[] memberPara = new SqlParameter[]
            {
                new SqlParameter("@unitCode", SqlDbType.VarChar) { Value = unitCode }
            };

            try
            {
                // 呼叫先前定義的資料庫查詢功能
                DataTable memberTB = _dbHelper.ArmyWebExecuteQuery(memberSql, memberPara);
                if (memberTB != null && memberTB.Rows.Count > 0)
                {
                    memberTB.Columns.Add("retire_date_tw");
                    foreach (DataRow row in memberTB.Rows)
                    {
                        row["retire_date_tw"] = _codeToName.dateTimeTran(row["retire_date"].ToString().Trim(), "yyy年MM月dd日", true);
                    }
                    return Ok(new { Result = "Success", isMemberList = true, Data = memberTB });
                }
                else
                {
                    return Ok(new { Result = "No member found", isMemberList = false, Data = memberTB });
                }
            }
            catch (Exception ex)
            {
                // 處理任何可能的異常
                WriteLog.Log(String.Format("【unitRetireMember Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【unitRetireMember Fail】" + ex.Message);
            }
        }

        // 單位未到成員查詢
        [HttpGet]
        [ActionName("unitRelayMember")]
        public IHttpActionResult unitRelayMember(string unitCode)
        {
            string memberSql = @"
             SELECT 
                m.member_id, m.member_name, LTRIM(RTRIM(vmu.item_no)) as item_no, LTRIM(RTRIM(m.unit_code + '-' + u.unit_title)) as unit_title, LTRIM(RTRIM(m.rank_code + ' - ' + r.rank_title)) as rank_title, LTRIM(RTRIM(m.title_code + ' - ' + t.title_name)) as title_name, m.salary_date
            FROM 
                Army.dbo.v_member_relay AS m
            LEFT JOIN 
	            Army.dbo.v_item_name_unit as vmu on vmu.unit_code = m.unit_code and vmu.item_no = m.item_no --組別
            LEFT JOIN 
                Army.dbo.title AS t ON m.title_code = t.title_code
            LEFT JOIN 
                Army.dbo.rank AS r ON m.rank_code = r.rank_code
            LEFT JOIN 
                Army.dbo.v_mu_unit AS u ON m.unit_code = u.unit_code
            WHERE 
                 m.unit_code = @unitCode
            ORDER BY
                CASE WHEN vmu.item_no IS NULL THEN 1 ELSE 0 END,
                m.unit_code,
                m.rank_code,
                m.member_id,
                m.title_code";

            // 使用SqlParameter防止SQL注入
            SqlParameter[] memberPara = new SqlParameter[]
            {
                new SqlParameter("@unitCode", SqlDbType.VarChar) { Value = unitCode }
            };

            try
            {
                // 呼叫先前定義的資料庫查詢功能
                DataTable memberTB = _dbHelper.ArmyWebExecuteQuery(memberSql, memberPara);
                if (memberTB != null && memberTB.Rows.Count > 0)
                {
                    memberTB.Columns.Add("salary_date_tw");
                    foreach (DataRow row in memberTB.Rows)
                    {
                        row["salary_date_tw"] = _codeToName.dateTimeTran(row["salary_date"].ToString(), "yyy年MM月dd日", true);
                    }
                    return Ok(new { Result = "Success", isMemberList = true, Data = memberTB });
                }
                else
                {
                    return Ok(new { Result = "No member found", isMemberList = false, Data = memberTB });
                }
            }
            catch (Exception ex)
            {
                // 處理任何可能的異常
                WriteLog.Log(String.Format("【unitRelayMember Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【unitRelayMember Fail】" + ex.Message);
            }
        }

        //現原詳細資料查詢
        [HttpGet]
        [ActionName("memberData")]
        public IHttpActionResult memberData(string memberId)
        {
            try
            {
                string memberDataSql = @"
                            select 
	                            vm.*, vmu.item_title '組別', LTRIM(RTRIM(ec1.educ_code + '-' + ec1.educ_name)) as '最高軍事教育', es1.school_desc '最高畢業學校', ve1.year_class '最高期別', LTRIM(RTRIM(vm.non_es_code + '-' + mnec.non_es_name)) '編外因素'
                            from 
	                            Army.dbo.v_member_data as vm 
                            left join 
	                            Army.dbo.v_item_name_unit as vmu on vmu.unit_code = vm.unit_code and vmu.item_no = vm.item_no --組別
                            left join 
	                            Army.dbo.educ_code as ec1 on ec1.educ_code = vm.military_educ_code --最高軍事教育
                            left join 
	                            Army.dbo.v_education as ve1 on ve1.member_id = vm.member_id and ec1.educ_code = ve1.educ_code --最高軍事教育
                            left join 
	                            Army.dbo.educ_school as es1 on es1.school_code = ve1.school_code   
                            left join
                                Army.dbo.memb_non_es_code as mnec on mnec.non_es_code = vm.non_es_code
                            WHERE
                                vm.member_id = @memberId";

                string basciEducSql = @"with 
	                            temptable as (
		                            select 
			                            ve.member_id '兵籍號碼', ve.educ_code, ec.educ_name, es.school_desc, ve.year_class, group_id= ROW_NUMBER() over (partition by ve.member_id order by study_date)
		                            from 
			                            Army.dbo.v_education as ve
		                            left join 
			                            Army.dbo.educ_code as ec on ec.educ_code = ve.educ_code
		                            left join 
			                            Army.dbo.educ_school as es on es.school_code = ve.school_code
		                            where 
			                            0=0
			                            and ve.educ_code in ('H','N')
			                    )
								select replace(tt.educ_code+'-'+tt.educ_name,' ','') '基礎軍事教育',tt.school_desc '基礎畢業學校', tt.year_class '基礎期別'
								from temptable as tt
								where tt.兵籍號碼 = @memberId and tt.group_id = @groupId";

                string localDataSql = @"
                            SELECT
                                *
                            FROM
                                Army.dbo.v_address
                            WHERE
                                member_id = @memberId";

                string skillDataSql = @"
                            SELECT
                                command_skill_code, skill1_code, skill2_code, skill3_code
                            FROM
                                Army.dbo.v_skill_profession
                            WHERE
                                member_id = @memberId";

                SqlParameter[] memberDataPara = { new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId } };
                SqlParameter[] basciEducPara = { 
                    new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId },
                    new SqlParameter("@groupId", SqlDbType.VarChar){Value = "1" }
                };
                SqlParameter[] localDataPara = { new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId } };
                SqlParameter[] skillDataPara = { new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId } };

                DataTable memberTB = _dbHelper.ArmyWebExecuteQuery(memberDataSql, memberDataPara);
                DataTable basciEducTB = _dbHelper.ArmyWebExecuteQuery(basciEducSql, basciEducPara);
                DataTable localTB = _dbHelper.ArmyWebExecuteQuery(localDataSql, localDataPara);
                DataTable skillTB = _dbHelper.ArmyWebExecuteQuery(skillDataSql, skillDataPara);

                string address = string.Empty;
                string locate = string.Empty;
                string basicMilitary = string.Empty;
                string basicSchool = string.Empty;
                string basicClass = string.Empty;
                string[] skillList = new string[4] { string.Empty, string.Empty, string.Empty, string.Empty };
                if (memberTB == null || memberTB.Rows.Count == 0)
                {
                    return Ok(new { Result = "No Memeber" });
                }
                if (basciEducTB != null && basciEducTB.Rows.Count != 0) 
                {
                    basicMilitary = basciEducTB.Rows[0]["基礎軍事教育"].ToString().Trim();
                    basicSchool = basciEducTB.Rows[0]["基礎畢業學校"].ToString().Trim();
                    basicClass = basciEducTB.Rows[0]["基礎期別"].ToString().Trim();
                }
                if (localTB != null && localTB.Rows.Count != 0)
                {
                    localTB.Rows[0]["city"] = _codeToName.cityName(localTB.Rows[0]["city"].ToString().Trim(), false);
                    locate = _codeToName.locateName(localTB.Rows[0]["locate"].ToString().Trim(), false);
                    address = localTB.Rows[0]["city"].ToString().Trim() + localTB.Rows[0]["village"].ToString().Trim() + 
                                localTB.Rows[0]["neighbor"].ToString().Trim() + localTB.Rows[0]["street"].ToString().Trim();
                }
                if (skillTB != null && skillTB.Rows.Count != 0)
                {
                    skillList[0] = _codeToName.skillName(skillTB.Rows[0]["command_skill_code"].ToString().Trim());
                    skillList[1] = _codeToName.skillName(skillTB.Rows[0]["skill1_code"].ToString().Trim());
                    skillList[2] = _codeToName.skillName(skillTB.Rows[0]["skill2_code"].ToString().Trim());
                    skillList[3] = _codeToName.skillName(skillTB.Rows[0]["skill3_code"].ToString().Trim());
                }

                // 時間格式處理
                string birthday = _codeToName.dateTimeTran(memberTB.Rows[0]["birthday"].ToString().Trim(), "yyy年MM月dd日", true);                          // 生日
                string salary_date = _codeToName.dateTimeTran(memberTB.Rows[0]["salary_date"].ToString().Trim(), "yyy年MM月dd日", true);                    // 任官日期
                string rank_date = _codeToName.dateTimeTran(memberTB.Rows[0]["rank_date"].ToString().Trim(), "yyy年MM月dd日", true);                        // 現階日期
                string pay_date = _codeToName.dateTimeTran(memberTB.Rows[0]["pay_date"].ToString().Trim(), "yyy年MM月dd日", true);                 // 任職日期
                string campaign_date = _codeToName.dateTimeTran(memberTB.Rows[0]["campaign_date"].ToString().Trim(), "yyy年MM月dd日", true);       // 入伍日期
                string volun_officer_date = _codeToName.dateTimeTran(memberTB.Rows[0]["volun_officer_date"].ToString().Trim(), "yyy年MM月dd日", true);      // 轉服志願軍官日期
                string volun_sergeant_date = _codeToName.dateTimeTran(memberTB.Rows[0]["volun_sergeant_date"].ToString().Trim(), "yyy年MM月dd日", true);    // 轉服志願士官日期
                string volun_soldier_date = _codeToName.dateTimeTran(memberTB.Rows[0]["volun_soldier_date"].ToString().Trim(), "yyy年MM月dd日", true);      // 轉服志願士兵日期
                string stop_volunteer_date = _codeToName.dateTimeTran(memberTB.Rows[0]["stop_volunteer_date"].ToString().Trim(), "yyy年MM月dd日", true);    // 廢止志願役日期

                // 代碼(code) 轉 名稱(name)
                memberTB.Rows[0]["group_code"] = _codeToName.groupName(memberTB.Rows[0]["group_code"].ToString().Trim());                      // 官科
                memberTB.Rows[0]["rank_code"] = _codeToName.rankName(memberTB.Rows[0]["rank_code"].ToString().Trim());                         // 現階
                memberTB.Rows[0]["m_skill_code"] = _codeToName.skillName(memberTB.Rows[0]["m_skill_code"].ToString().Trim());                  // 個人主專
                memberTB.Rows[0]["service_code"] = _codeToName.serviceName(memberTB.Rows[0]["service_code"].ToString().Trim());                // 軍種
                memberTB.Rows[0]["es_rank_code"] = _codeToName.rankName(memberTB.Rows[0]["es_rank_code"].ToString().Trim());                   // 編階
                memberTB.Rows[0]["title_code"] = _codeToName.titleName(memberTB.Rows[0]["title_code"].ToString().Trim());                      // 職稱  
                memberTB.Rows[0]["es_skill_code"] = _codeToName.skillName(memberTB.Rows[0]["es_skill_code"].ToString().Trim());                // 編專
                memberTB.Rows[0]["campaign_code"] = _codeToName.campaignName(memberTB.Rows[0]["campaign_code"].ToString().Trim());             // 役別
                memberTB.Rows[0]["trans_code"] = _codeToName.transName(memberTB.Rows[0]["trans_code"].ToString().Trim());                      // 異動代號
                memberTB.Rows[0]["military_educ_code"] = _codeToName.militaryName(memberTB.Rows[0]["military_educ_code"].ToString().Trim());   // 最高軍事教育
                memberTB.Rows[0]["school_code"] = _codeToName.schoolName(memberTB.Rows[0]["school_code"].ToString().Trim());                   // 畢業學校
                memberTB.Rows[0]["common_educ_code"] = _codeToName.educName(memberTB.Rows[0]["common_educ_code"].ToString().Trim());           // 民間學歷
                memberTB.Rows[0]["non_es_code"] = _codeToName.esName(memberTB.Rows[0]["non_es_code"].ToString().Trim());                       // 編外因素
                string unitName = _codeToName.unitName(memberTB.Rows[0]["unit_code"].ToString().Trim(), false);
                string unPromote = memberTB.Rows[0]["un_promote_code"].ToString().Trim();
                if (memberTB.Rows[0]["un_promote_code"].ToString().Trim() == "1") 
                {
                    unPromote = memberTB.Rows[0]["un_promote_code"].ToString().Trim() + " - 常備役";
                }
                else if(memberTB.Rows[0]["un_promote_code"].ToString().Trim() == "2")
                {
                    unPromote = memberTB.Rows[0]["un_promote_code"].ToString().Trim() + " - 預備役";
                }

                memberDetailedRes basicMemData = new memberDetailedRes
                {
                    MemberName = memberTB.Rows[0]["member_name"].ToString().Trim(),
                    MemberId = memberTB.Rows[0]["member_id"].ToString().Trim(),
                    BloodType = memberTB.Rows[0]["blood_type"].ToString().Trim(),
                    Birthday = birthday,
                    GroupCode = memberTB.Rows[0]["group_code"].ToString().Trim(),
                    RankCode = memberTB.Rows[0]["rank_code"].ToString().Trim(),
                    SupplyRank = memberTB.Rows[0]["supply_rank"].ToString().Trim(),
                    MSkillCode = memberTB.Rows[0]["m_skill_code"].ToString().Trim(),
                    GroupSkill = skillList[0],
                    FirstSkill = skillList[1],
                    SecondSkill = skillList[2],
                    ThirdSkill = skillList[3],
                    SalaryDate = salary_date,
                    RankDate = rank_date,
                    ServiceCode = memberTB.Rows[0]["service_code"].ToString().Trim(),
                    UnPromoteCode = unPromote,
                    UnitName = unitName,
                    UnitCode = memberTB.Rows[0]["unit_code"].ToString().Trim(),
                    PayUnitCode = memberTB.Rows[0]["pay_unit_code"].ToString().Trim(),
                    CornerCode = memberTB.Rows[0]["corner_code"].ToString().Trim(),
                    EsRankCode = memberTB.Rows[0]["es_rank_code"].ToString().Trim(),
                    TitleCode = memberTB.Rows[0]["title_code"].ToString().Trim(),
                    EsSkillCode = memberTB.Rows[0]["es_skill_code"].ToString().Trim(),
                    PayDate = pay_date,
                    CampaignCode = memberTB.Rows[0]["campaign_code"].ToString().Trim(),
                    CampaignSerial = memberTB.Rows[0]["campaign_serial"].ToString().Trim(),
                    CampaignDate = campaign_date,
                    NonEsCode = memberTB.Rows[0]["編外因素"].ToString().Trim(),
                    ColumnNo = memberTB.Rows[0]["column_no"].ToString().Trim(),
                    GroupNo = memberTB.Rows[0]["組別"].ToString().Trim(),
                    SerialCode = memberTB.Rows[0]["serial_code"].ToString().Trim(),
                    TransCode = memberTB.Rows[0]["trans_code"].ToString().Trim(),
                    BasicMilitaryCode = basicMilitary,
                    SchoolCode = basicSchool,
                    ClassCode = basicClass,
                    MilitaryEducCode = memberTB.Rows[0]["最高軍事教育"].ToString().Trim(),
                    HighSchoolCode = memberTB.Rows[0]["最高畢業學校"].ToString().Trim(),
                    HighClassCode = memberTB.Rows[0]["最高期別"].ToString().Trim(),
                    CommonEducCode = memberTB.Rows[0]["common_educ_code"].ToString().Trim(),
                    IqScore = memberTB.Rows[0]["iq_score"].ToString().Trim(),
                    LocalCode = locate,
                    ResidenceAddress = address,
                    VolunOfficerDate = volun_officer_date,
                    VolunSergeantDate = volun_sergeant_date,
                    VolunSoldierDate = volun_soldier_date,
                    StopVolunteerDate = stop_volunteer_date,
                    LocalMark = memberTB.Rows[0]["local_mark"].ToString().Trim(),
                    RetireDate = " "
                };
                return Ok(new { Result = "Success", basicMemData });
            }
            catch (Exception ex) 
            {
                WriteLog.Log(String.Format("【memberData Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【memberData Fail】" + ex.Message);
            }  
        }

        //人事現員退伍
        [HttpGet]
        [ActionName("memberRetireData")]
        public IHttpActionResult memberRetireData(string memberId)
        {
            try
            {                               
                string memberDataSql = @"                            
                            select 
	                            vm.*, vmu.item_title '組別', LTRIM(RTRIM(ec1.educ_code + '-' + ec1.educ_name)) as '最高軍事教育', es1.school_desc '最高畢業學校', ve1.year_class '最高期別', LTRIM(RTRIM(vm.non_es_code + '-' + mnec.non_es_name)) '編外因素'
                            from 
	                            Army.dbo.v_member_retire as vm 
                            left join 
	                            Army.dbo.v_item_name_unit as vmu on vmu.unit_code = vm.unit_code and vmu.item_no = vm.item_no
                            left join 
	                            Army.dbo.educ_code as ec1 on ec1.educ_code = vm.military_educ_code
                            left join 
	                            Army.dbo.v_education_retire as ve1 on ve1.member_id = vm.member_id and ec1.educ_code = ve1.educ_code 
                            left join 
	                            Army.dbo.educ_school as es1 on es1.school_code = ve1.school_code
                            left join
                                Army.dbo.memb_non_es_code as mnec on mnec.non_es_code = vm.non_es_code
                            WHERE
                                vm.member_id = @memberId";

                string basciEducSql = @"with 
	                            temptable as (
		                            select 
			                            ve.member_id '兵籍號碼', ve.educ_code, ec.educ_name, es.school_desc, ve.year_class, group_id= ROW_NUMBER() over (partition by ve.member_id order by study_date)
		                            from 
			                            Army.dbo.v_education_retire as ve
		                            left join 
			                            Army.dbo.educ_code as ec on ec.educ_code = ve.educ_code
		                            left join 
			                            Army.dbo.educ_school as es on es.school_code = ve.school_code
		                            where 
			                            0=0
			                            and ve.educ_code in ('H','N')
			                    )
								select replace(tt.educ_code+'-'+tt.educ_name,' ','') '基礎軍事教育',tt.school_desc '基礎畢業學校', tt.year_class '基礎期別'
								from temptable as tt
								where tt.兵籍號碼 = @memberId and tt.group_id = @groupId";

                string localDataSql = @"
                            SELECT
                                *
                            FROM
                                Army.dbo.v_address_retire
                            WHERE
                                member_id = @memberId";

                string skillDataSql = @"
                            SELECT
                                command_skill_code, skill1_code, skill2_code, skill3_code
                            FROM
                                Army.dbo.v_skill_profession
                            WHERE
                                member_id = @memberId";

                SqlParameter[] memberDataPara = { new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId } };
                SqlParameter[] basciEducPara = {
                    new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId },
                    new SqlParameter("@groupId", SqlDbType.VarChar){Value = "1" }
                };
                SqlParameter[] localDataPara = { new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId } };
                SqlParameter[] skillDataPara = { new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId } };

                DataTable memberTB = _dbHelper.ArmyWebExecuteQuery(memberDataSql, memberDataPara);
                DataTable basciEducTB = _dbHelper.ArmyWebExecuteQuery(basciEducSql, basciEducPara);
                DataTable localTB = _dbHelper.ArmyWebExecuteQuery(localDataSql, localDataPara);
                DataTable skillTB = _dbHelper.ArmyWebExecuteQuery(skillDataSql, skillDataPara);

                string address = string.Empty;
                string locate = string.Empty;
                string basicMilitary = string.Empty;
                string basicSchool = string.Empty;
                string basicClass = string.Empty;
                string[] skillList = new string[4] { string.Empty, string.Empty, string.Empty, string.Empty };
                if (memberTB == null || memberTB.Rows.Count == 0)
                {
                    return Ok(new { Result = "No Memeber" });
                }
                if (basciEducTB != null && basciEducTB.Rows.Count != 0)
                {
                    basicMilitary = basciEducTB.Rows[0]["基礎軍事教育"].ToString().Trim();
                    basicSchool = basciEducTB.Rows[0]["基礎畢業學校"].ToString().Trim();
                    basicClass = basciEducTB.Rows[0]["基礎期別"].ToString().Trim();
                }
                if (localTB != null && localTB.Rows.Count != 0)
                {
                    localTB.Rows[0]["city"] = _codeToName.cityName(localTB.Rows[0]["city"].ToString().Trim(), false);
                    locate = _codeToName.locateName(localTB.Rows[0]["locate"].ToString().Trim(), false);
                    address = localTB.Rows[0]["city"].ToString().Trim() + localTB.Rows[0]["village"].ToString().Trim() +
                                localTB.Rows[0]["neighbor"].ToString().Trim() + localTB.Rows[0]["street"].ToString().Trim();
                }
                if (skillTB != null && skillTB.Rows.Count != 0)
                {
                    skillList[0] = _codeToName.skillName(skillTB.Rows[0]["command_skill_code"].ToString().Trim());
                    skillList[1] = _codeToName.skillName(skillTB.Rows[0]["skill1_code"].ToString().Trim());
                    skillList[2] = _codeToName.skillName(skillTB.Rows[0]["skill2_code"].ToString().Trim());
                    skillList[3] = _codeToName.skillName(skillTB.Rows[0]["skill3_code"].ToString().Trim());
                }

                // 時間格式處理
                string birthday = _codeToName.dateTimeTran(memberTB.Rows[0]["birthday"].ToString().Trim(), "yyy年MM月dd日", true);                          // 生日
                string salary_date = _codeToName.dateTimeTran(memberTB.Rows[0]["salary_date"].ToString().Trim(), "yyy年MM月dd日", true);                    // 任官日期
                string rank_date = _codeToName.dateTimeTran(memberTB.Rows[0]["rank_date"].ToString().Trim(), "yyy年MM月dd日", true);                        // 現階日期
                string pay_date = _codeToName.dateTimeTran(memberTB.Rows[0]["pay_date"].ToString().Trim(), "yyy年MM月dd日", true);                 // 任職日期
                string campaign_date = _codeToName.dateTimeTran(memberTB.Rows[0]["campaign_date"].ToString().Trim(), "yyy年MM月dd日", true);       // 入伍日期
                string volun_officer_date = _codeToName.dateTimeTran(memberTB.Rows[0]["volun_officer_date"].ToString().Trim(), "yyy年MM月dd日", true);      // 轉服志願軍官日期
                string volun_sergeant_date = _codeToName.dateTimeTran(memberTB.Rows[0]["volun_sergeant_date"].ToString().Trim(), "yyy年MM月dd日", true);    // 轉服志願士官日期
                string volun_soldier_date = _codeToName.dateTimeTran(memberTB.Rows[0]["volun_soldier_date"].ToString().Trim(), "yyy年MM月dd日", true);      // 轉服志願士兵日期
                string stop_volunteer_date = _codeToName.dateTimeTran(memberTB.Rows[0]["stop_volunteer_date"].ToString().Trim(), "yyy年MM月dd日", true);    // 廢止志願役日期
                string retire_date = _codeToName.dateTimeTran(memberTB.Rows[0]["retire_date"].ToString().Trim(), "yyy年MM月dd日", true);

                // 代碼(code) 轉 名稱(name)
                memberTB.Rows[0]["group_code"] = _codeToName.groupName(memberTB.Rows[0]["group_code"].ToString().Trim());                      // 官科
                memberTB.Rows[0]["rank_code"] = _codeToName.rankName(memberTB.Rows[0]["rank_code"].ToString().Trim());                         // 現階
                memberTB.Rows[0]["m_skill_code"] = _codeToName.skillName(memberTB.Rows[0]["m_skill_code"].ToString().Trim());                  // 個人主專
                memberTB.Rows[0]["service_code"] = _codeToName.serviceName(memberTB.Rows[0]["service_code"].ToString().Trim());                // 軍種
                memberTB.Rows[0]["es_rank_code"] = _codeToName.rankName(memberTB.Rows[0]["es_rank_code"].ToString().Trim());                   // 編階
                memberTB.Rows[0]["title_code"] = _codeToName.titleName(memberTB.Rows[0]["title_code"].ToString().Trim());                      // 職稱  
                memberTB.Rows[0]["es_skill_code"] = _codeToName.skillName(memberTB.Rows[0]["es_skill_code"].ToString().Trim());                // 編專
                memberTB.Rows[0]["campaign_code"] = _codeToName.campaignName(memberTB.Rows[0]["campaign_code"].ToString().Trim());             // 役別
                memberTB.Rows[0]["trans_code"] = _codeToName.transName(memberTB.Rows[0]["trans_code"].ToString().Trim());                      // 異動代號
                //memberTB.Rows[0]["military_educ_code"] = _codeToName.militaryName(memberTB.Rows[0]["military_educ_code"].ToString().Trim());   // 最高軍事教育
                //memberTB.Rows[0]["hool_code"] = _codeToName.schoolName(memberTB.Rows[0]["hool_code"].ToString().Trim());                   // 畢業學校
                memberTB.Rows[0]["common_educ_code"] = _codeToName.educName(memberTB.Rows[0]["common_educ_code"].ToString().Trim());           // 民間學歷
                memberTB.Rows[0]["non_es_code"] = _codeToName.esName(memberTB.Rows[0]["non_es_code"].ToString().Trim());                       // 編外因素
                string unitName = _codeToName.unitName(memberTB.Rows[0]["unit_code"].ToString().Trim(), false);
                string unPromote = memberTB.Rows[0]["un_promote_code"].ToString().Trim();
                if (memberTB.Rows[0]["un_promote_code"].ToString().Trim() == "1")
                {
                    unPromote = memberTB.Rows[0]["un_promote_code"].ToString().Trim() + " - 常備役";
                }
                else if (memberTB.Rows[0]["un_promote_code"].ToString().Trim() == "2")
                {
                    unPromote = memberTB.Rows[0]["un_promote_code"].ToString().Trim() + " - 預備役";
                }

                memberDetailedRes basicMemData = new memberDetailedRes
                {
                    MemberName = memberTB.Rows[0]["member_name"].ToString().Trim(),
                    MemberId = memberTB.Rows[0]["member_id"].ToString().Trim(),
                    BloodType = memberTB.Rows[0]["blood_type"].ToString().Trim(),
                    Birthday = birthday,
                    GroupCode = memberTB.Rows[0]["group_code"].ToString().Trim(),
                    RankCode = memberTB.Rows[0]["rank_code"].ToString().Trim(),
                    SupplyRank = memberTB.Rows[0]["supply_rank"].ToString().Trim(),
                    MSkillCode = memberTB.Rows[0]["m_skill_code"].ToString().Trim(),
                    GroupSkill = skillList[0],
                    FirstSkill = skillList[1],
                    SecondSkill = skillList[2],
                    ThirdSkill = skillList[3],
                    SalaryDate = salary_date,
                    RankDate = rank_date,
                    ServiceCode = memberTB.Rows[0]["service_code"].ToString().Trim(),
                    UnPromoteCode = unPromote,
                    UnitName = unitName,
                    UnitCode = memberTB.Rows[0]["unit_code"].ToString().Trim(),
                    PayUnitCode = memberTB.Rows[0]["pay_unit_code"].ToString().Trim(),
                    CornerCode = memberTB.Rows[0]["corner_code"].ToString().Trim(),
                    EsRankCode = memberTB.Rows[0]["es_rank_code"].ToString().Trim(),
                    TitleCode = memberTB.Rows[0]["title_code"].ToString().Trim(),
                    EsSkillCode = memberTB.Rows[0]["es_skill_code"].ToString().Trim(),
                    PayDate = pay_date,
                    CampaignCode = memberTB.Rows[0]["campaign_code"].ToString().Trim(),
                    CampaignSerial = memberTB.Rows[0]["campaign_serial"].ToString().Trim(),
                    CampaignDate = campaign_date,
                    NonEsCode = memberTB.Rows[0]["non_es_code"].ToString().Trim(),
                    ColumnNo = memberTB.Rows[0]["column_no"].ToString().Trim(),
                    GroupNo = memberTB.Rows[0]["組別"].ToString().Trim(),
                    SerialCode = memberTB.Rows[0]["serial_code"].ToString().Trim(),
                    TransCode = memberTB.Rows[0]["trans_code"].ToString().Trim(),
                    BasicMilitaryCode = basicMilitary,
                    SchoolCode = basicSchool,
                    ClassCode = basicClass,
                    MilitaryEducCode = memberTB.Rows[0]["最高軍事教育"].ToString().Trim(),
                    HighSchoolCode = memberTB.Rows[0]["最高畢業學校"].ToString().Trim(),
                    HighClassCode = memberTB.Rows[0]["最高期別"].ToString().Trim(),
                    CommonEducCode = memberTB.Rows[0]["common_educ_code"].ToString().Trim(),
                    IqScore = memberTB.Rows[0]["iq_score"].ToString().Trim(),
                    LocalCode = locate,
                    ResidenceAddress = address,
                    VolunOfficerDate = volun_officer_date,
                    VolunSergeantDate = volun_sergeant_date,
                    VolunSoldierDate = volun_soldier_date,
                    StopVolunteerDate = stop_volunteer_date,
                    LocalMark = memberTB.Rows[0]["local_mark"].ToString().Trim(),
                    RetireDate = retire_date
                };
                return Ok(new { Result = "Success", basicMemData });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【memberRetireData Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【memberRetireData Fail】" + ex.Message);
            }
        }

        //人事現員未到
        [HttpGet]
        [ActionName("memberRelayData")]
        public IHttpActionResult memberRelayData(string memberId)
        {
            try
            {
                string memberDataSql = @"                            
                            select 
	                            vm.*, vmu.item_title '組別', LTRIM(RTRIM(ec1.educ_code + '-' + ec1.educ_name)) as '最高軍事教育', es1.school_desc '最高畢業學校', ve1.year_class '最高期別', LTRIM(RTRIM(vm.non_es_code + '-' + mnec.non_es_name)) '編外因素'
                            from 
	                            Army.dbo.v_member_relay as vm 
                            left join 
	                            Army.dbo.v_item_name_unit as vmu on vmu.unit_code = vm.unit_code and vmu.item_no = vm.item_no --組別
                            left join 
	                            Army.dbo.educ_code as ec1 on ec1.educ_code = vm.military_educ_code --最高軍事教育
                            left join 
	                            Army.dbo.v_education as ve1 on ve1.member_id = vm.member_id and ec1.educ_code = ve1.educ_code --最高軍事教育
                            left join 
	                            Army.dbo.educ_school as es1 on es1.school_code = ve1.school_code
                            left join
                                Army.dbo.memb_non_es_code as mnec on mnec.non_es_code = vm.non_es_code
                            WHERE
                                vm.member_id = @memberId";

                string basciEducSql = @"
                                with 
	                            temptable as (
		                            select 
			                            ve.member_id '兵籍號碼', ve.educ_code, ec.educ_name, es.school_desc, ve.year_class, group_id= ROW_NUMBER() over (partition by ve.member_id order by study_date)
		                            from 
			                            Army.dbo.v_education as ve
		                            left join 
			                            Army.dbo.educ_code as ec on ec.educ_code = ve.educ_code
		                            left join 
			                            Army.dbo.educ_school as es on es.school_code = ve.school_code
		                            where 
			                            0=0
			                            and ve.educ_code in ('H','N')
			                    )
								select replace(tt.educ_code+'-'+tt.educ_name,' ','') '基礎軍事教育',tt.school_desc '基礎畢業學校', tt.year_class '基礎期別'
								from temptable as tt
								where tt.兵籍號碼 = @memberId and tt.group_id = @groupId";

                string localDataSql = @"
                            SELECT
                                *
                            FROM
                                Army.dbo.v_address
                            WHERE
                                member_id = @memberId";
                string skillDataSql = @"
                            SELECT
                                command_skill_code, skill1_code, skill2_code, skill3_code
                            FROM
                                Army.dbo.v_skill_profession
                            WHERE
                                member_id = @memberId";

                SqlParameter[] memberDataPara = { new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId } };
                SqlParameter[] basciEducPara = {
                    new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId },
                    new SqlParameter("@groupId", SqlDbType.VarChar){Value = "1" }
                };
                SqlParameter[] localDataPara = { new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId } };
                SqlParameter[] skillDataPara = { new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId } };

                DataTable memberTB = _dbHelper.ArmyWebExecuteQuery(memberDataSql, memberDataPara);
                DataTable basciEducTB = _dbHelper.ArmyWebExecuteQuery(basciEducSql, basciEducPara);
                DataTable localTB = _dbHelper.ArmyWebExecuteQuery(localDataSql, localDataPara);
                DataTable skillTB = _dbHelper.ArmyWebExecuteQuery(skillDataSql, skillDataPara);

                string address = string.Empty;
                string locate = string.Empty;
                string basicMilitary = string.Empty;
                string basicSchool = string.Empty;
                string basicClass = string.Empty;
                string[] skillList = new string[4] { string.Empty, string.Empty, string.Empty, string.Empty };
                if (memberTB == null || memberTB.Rows.Count == 0)
                {
                    return Ok(new { Result = "No Memeber" });
                }
                if (basciEducTB != null && basciEducTB.Rows.Count != 0)
                {
                    basicMilitary = basciEducTB.Rows[0]["基礎軍事教育"].ToString().Trim();
                    basicSchool = basciEducTB.Rows[0]["基礎畢業學校"].ToString().Trim();
                    basicClass = basciEducTB.Rows[0]["基礎期別"].ToString().Trim();
                }
                if (localTB != null && localTB.Rows.Count != 0)
                {
                    localTB.Rows[0]["city"] = _codeToName.cityName(localTB.Rows[0]["city"].ToString().Trim(), false);
                    locate = _codeToName.locateName(localTB.Rows[0]["locate"].ToString().Trim(), false);
                    address = localTB.Rows[0]["city"].ToString().Trim() + localTB.Rows[0]["village"].ToString().Trim() +
                                localTB.Rows[0]["neighbor"].ToString().Trim() + localTB.Rows[0]["street"].ToString().Trim();
                }
                if (skillTB != null && skillTB.Rows.Count != 0)
                {
                    skillList[0] = _codeToName.skillName(skillTB.Rows[0]["command_skill_code"].ToString().Trim());
                    skillList[1] = _codeToName.skillName(skillTB.Rows[0]["skill1_code"].ToString().Trim());
                    skillList[2] = _codeToName.skillName(skillTB.Rows[0]["skill2_code"].ToString().Trim());
                    skillList[3] = _codeToName.skillName(skillTB.Rows[0]["skill3_code"].ToString().Trim());
                }

                // 時間格式處理
                string birthday = _codeToName.dateTimeTran(memberTB.Rows[0]["birthday"].ToString().Trim(), "yyy年MM月dd日", true);                          // 生日
                string salary_date = _codeToName.dateTimeTran(memberTB.Rows[0]["salary_date"].ToString().Trim(), "yyy年MM月dd日", true);                    // 任官日期
                string rank_date = _codeToName.dateTimeTran(memberTB.Rows[0]["rank_date"].ToString().Trim(), "yyy年MM月dd日", true);                        // 現階日期
                string pay_date = _codeToName.dateTimeTran(memberTB.Rows[0]["pay_date"].ToString().Trim(), "yyy年MM月dd日", true);                 // 任職日期
                string campaign_date = _codeToName.dateTimeTran(memberTB.Rows[0]["campaign_date"].ToString().Trim(), "yyy年MM月dd日", true);       // 入伍日期
                string volun_officer_date = _codeToName.dateTimeTran(memberTB.Rows[0]["volun_officer_date"].ToString().Trim(), "yyy年MM月dd日", true);      // 轉服志願軍官日期
                string volun_sergeant_date = _codeToName.dateTimeTran(memberTB.Rows[0]["volun_sergeant_date"].ToString().Trim(), "yyy年MM月dd日", true);    // 轉服志願士官日期
                string volun_soldier_date = _codeToName.dateTimeTran(memberTB.Rows[0]["volun_soldier_date"].ToString().Trim(), "yyy年MM月dd日", true);      // 轉服志願士兵日期
                string stop_volunteer_date = _codeToName.dateTimeTran(memberTB.Rows[0]["stop_volunteer_date"].ToString().Trim(), "yyy年MM月dd日", true);    // 廢止志願役日期
                string retire_date = _codeToName.dateTimeTran(memberTB.Rows[0]["retire_date"].ToString().Trim(), "yyy年MM月dd日", true);

                // 代碼(code) 轉 名稱(name)
                memberTB.Rows[0]["group_code"] = _codeToName.groupName(memberTB.Rows[0]["group_code"].ToString().Trim());                      // 官科
                memberTB.Rows[0]["rank_code"] = _codeToName.rankName(memberTB.Rows[0]["rank_code"].ToString().Trim());                         // 現階
                memberTB.Rows[0]["m_skill_code"] = _codeToName.skillName(memberTB.Rows[0]["m_skill_code"].ToString().Trim());                  // 個人主專
                memberTB.Rows[0]["service_code"] = _codeToName.serviceName(memberTB.Rows[0]["service_code"].ToString().Trim());                // 軍種
                memberTB.Rows[0]["es_rank_code"] = _codeToName.rankName(memberTB.Rows[0]["es_rank_code"].ToString().Trim());                   // 編階
                memberTB.Rows[0]["title_code"] = _codeToName.titleName(memberTB.Rows[0]["title_code"].ToString().Trim());                      // 職稱  
                memberTB.Rows[0]["es_skill_code"] = _codeToName.skillName(memberTB.Rows[0]["es_skill_code"].ToString().Trim());                // 編專
                memberTB.Rows[0]["campaign_code"] = _codeToName.campaignName(memberTB.Rows[0]["campaign_code"].ToString().Trim());             // 役別
                memberTB.Rows[0]["trans_code"] = _codeToName.transName(memberTB.Rows[0]["trans_code"].ToString().Trim());                      // 異動代號
                memberTB.Rows[0]["military_educ_code"] = _codeToName.militaryName(memberTB.Rows[0]["military_educ_code"].ToString().Trim());   // 最高軍事教育
                memberTB.Rows[0]["school_code"] = _codeToName.schoolName(memberTB.Rows[0]["school_code"].ToString().Trim());                   // 畢業學校
                memberTB.Rows[0]["common_educ_code"] = _codeToName.educName(memberTB.Rows[0]["common_educ_code"].ToString().Trim());           // 民間學歷
                memberTB.Rows[0]["non_es_code"] = _codeToName.esName(memberTB.Rows[0]["non_es_code"].ToString().Trim());                       // 編外因素
                string unitName = _codeToName.unitName(memberTB.Rows[0]["unit_code"].ToString().Trim(), false);
                string unPromote = memberTB.Rows[0]["un_promote_code"].ToString().Trim();
                if (memberTB.Rows[0]["un_promote_code"].ToString().Trim() == "1")
                {
                    unPromote = memberTB.Rows[0]["un_promote_code"].ToString().Trim() + " - 常備役";
                }
                else if (memberTB.Rows[0]["un_promote_code"].ToString().Trim() == "2")
                {
                    unPromote = memberTB.Rows[0]["un_promote_code"].ToString().Trim() + " - 預備役";
                }

                memberDetailedRes basicMemData = new memberDetailedRes
                {
                    MemberName = memberTB.Rows[0]["member_name"].ToString().Trim(),
                    MemberId = memberTB.Rows[0]["member_id"].ToString().Trim(),
                    BloodType = memberTB.Rows[0]["blood_type"].ToString().Trim(),
                    Birthday = birthday,
                    GroupCode = memberTB.Rows[0]["group_code"].ToString().Trim(),
                    RankCode = memberTB.Rows[0]["rank_code"].ToString().Trim(),
                    SupplyRank = memberTB.Rows[0]["supply_rank"].ToString().Trim(),
                    MSkillCode = memberTB.Rows[0]["m_skill_code"].ToString().Trim(),
                    GroupSkill = skillList[0],
                    FirstSkill = skillList[1],
                    SecondSkill = skillList[2],
                    ThirdSkill = skillList[3],
                    SalaryDate = salary_date,
                    RankDate = rank_date,
                    ServiceCode = memberTB.Rows[0]["service_code"].ToString().Trim(),
                    UnPromoteCode = unPromote,
                    UnitName = unitName,
                    UnitCode = memberTB.Rows[0]["unit_code"].ToString().Trim(),
                    PayUnitCode = memberTB.Rows[0]["pay_unit_code"].ToString().Trim(),
                    CornerCode = memberTB.Rows[0]["corner_code"].ToString().Trim(),
                    EsRankCode = memberTB.Rows[0]["es_rank_code"].ToString().Trim(),
                    TitleCode = memberTB.Rows[0]["title_code"].ToString().Trim(),
                    EsSkillCode = memberTB.Rows[0]["es_skill_code"].ToString().Trim(),
                    PayDate = pay_date,
                    CampaignCode = memberTB.Rows[0]["campaign_code"].ToString().Trim(),
                    CampaignSerial = memberTB.Rows[0]["campaign_serial"].ToString().Trim(),
                    CampaignDate = campaign_date,
                    NonEsCode = memberTB.Rows[0]["non_es_code"].ToString().Trim(),
                    ColumnNo = memberTB.Rows[0]["column_no"].ToString().Trim(),
                    GroupNo = memberTB.Rows[0]["組別"].ToString().Trim(),
                    SerialCode = memberTB.Rows[0]["serial_code"].ToString().Trim(),
                    TransCode = memberTB.Rows[0]["trans_code"].ToString().Trim(),
                    BasicMilitaryCode = basicMilitary,
                    SchoolCode = basicSchool,
                    ClassCode = basicClass,
                    MilitaryEducCode = memberTB.Rows[0]["最高軍事教育"].ToString().Trim(),
                    HighSchoolCode = memberTB.Rows[0]["最高畢業學校"].ToString().Trim(),
                    HighClassCode = memberTB.Rows[0]["最高期別"].ToString().Trim(),
                    CommonEducCode = memberTB.Rows[0]["common_educ_code"].ToString().Trim(),
                    IqScore = memberTB.Rows[0]["iq_score"].ToString().Trim(),
                    LocalCode = locate,
                    ResidenceAddress = address,
                    VolunOfficerDate = volun_officer_date,
                    VolunSergeantDate = volun_sergeant_date,
                    VolunSoldierDate = volun_soldier_date,
                    StopVolunteerDate = stop_volunteer_date,
                    LocalMark = memberTB.Rows[0]["local_mark"].ToString().Trim(),
                    RetireDate = retire_date
                };
                return Ok(new { Result = "Success", basicMemData });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【memberRelayData Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【memberRelayData Fail】" + ex.Message);
            }
        }

        // 經歷資料
        [HttpGet]
        [ActionName("memberExperience")]
        public IHttpActionResult memberExperience(string memberId)
        {
            List<experienceRes> experiencesList = new List<experienceRes>();
            string experienceSql = @"
                                    SELECT 
                                       LTRIM(RTRIM(unit_code)) as unit_code, LTRIM(RTRIM(rank_code)) as rank_code, 
                                       LTRIM(RTRIM(title_code)) as title_code, LTRIM(RTRIM(es_rank_code)) as es_rank_code, 
                                       LTRIM(RTRIM(skill_code)) as skill_code, effect_date, 
                                       LTRIM(RTRIM(doc_no)) as doc_no, doc_date, 
                                       LTRIM(RTRIM(non_es_code)) as non_es_code, LTRIM(RTRIM(trans_code)) as trans_code
                                    FROM 
                                        Army.dbo.v_experience 
                                    WHERE 
                                        member_id = @memberId
                                    ORDER BY
                                        doc_date DESC";

            // 創建一個SqlParameter的實例來防止SQL注入
            SqlParameter[] experiencePara = new SqlParameter[]
            {
                new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId }
            };

            try
            {
                // 調用之前定義的方法執行查詢，返回一個DataTable
                DataTable experienceTB = _dbHelper.ArmyWebExecuteQuery(experienceSql, experiencePara);

                if (experienceTB != null && experienceTB.Rows.Count > 0)
                {
                    foreach (DataRow row in experienceTB.Rows)
                    {
                        experienceRes experience = new experienceRes()
                        {                            
                            UnitCode = _codeToName.unitName(row["unit_code"].ToString()),

                            RankCode = _codeToName.rankName(row["rank_code"].ToString()),

                            TitleCode = _codeToName.titleName(row["title_code"].ToString()),

                            EsRankCode = _codeToName.rankName(row["es_rank_code"].ToString()),

                            SkillCode = _codeToName.skillName(row["skill_code"].ToString()),
                                                   
                            EffectDate = _codeToName.dateTimeTran(row["effect_date"].ToString(), "yyy年MM月dd日", true),

                            DocDate = _codeToName.dateTimeTran(row["doc_date"].ToString().Trim(), "yyy年MM月dd日", true),

                            DocNo = row["doc_no"].ToString(),

                            NonEsCode = row["non_es_code"].ToString(),

                            TransCode = row["trans_code"].ToString()
                        };
                        experiencesList.Add(experience);
                    }
                    return Ok(new { Result = "Success", experiencesList });
                }                    
                else
                {
                    return Ok(new { Result = "Fail", experiencesList });
                }
            }
            catch (Exception ex)
            {
                // 如果出現異常，返回錯誤信息
                WriteLog.Log(String.Format("【memberExperience Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【memberExperience Fail】" + ex.Message);
            }
        }

        // 經歷退伍資料
        [HttpGet]
        [ActionName("memberRetireExperience")]
        public IHttpActionResult memberRetireExperience(string memberId)
        {
            List<experienceRes> experiencesList = new List<experienceRes>();
            string experienceSql = @"
                                    SELECT 
                                       LTRIM(RTRIM(unit_code)) as unit_code, LTRIM(RTRIM(rank_code)) as rank_code, 
                                       LTRIM(RTRIM(title_code)) as title_code, LTRIM(RTRIM(es_rank_code)) as es_rank_code, 
                                       LTRIM(RTRIM(skill_code)) as skill_code, effect_date, 
                                       LTRIM(RTRIM(doc_no)) as doc_no, doc_date, 
                                       LTRIM(RTRIM(non_es_code)) as non_es_code, LTRIM(RTRIM(trans_code)) as trans_code
                                    FROM 
                                       Army.dbo.v_experience_retire
                                    WHERE 
                                       member_id = @memberId
                                    ORDER BY
                                        doc_date DESC";

            // 創建一個SqlParameter的實例來防止SQL注入
            SqlParameter[] experiencePara = new SqlParameter[]
            {
                new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId }
            };

            try
            {
                // 調用之前定義的方法執行查詢，返回一個DataTable
                DataTable experienceTB = _dbHelper.ArmyWebExecuteQuery(experienceSql, experiencePara);

                if (experienceTB != null && experienceTB.Rows.Count > 0)
                {
                    foreach (DataRow row in experienceTB.Rows)
                    {
                        experienceRes experience = new experienceRes()
                        {
                            UnitCode = _codeToName.unitName(row["unit_code"].ToString()),

                            RankCode = _codeToName.rankName(row["rank_code"].ToString()),

                            TitleCode = _codeToName.titleName(row["title_code"].ToString()),

                            EsRankCode = _codeToName.rankName(row["es_rank_code"].ToString()),

                            SkillCode = _codeToName.skillName(row["skill_code"].ToString()),

                            EffectDate = _codeToName.dateTimeTran(row["effect_date"].ToString(), "yyy年MM月dd日", true),

                            DocDate = _codeToName.dateTimeTran(row["doc_date"].ToString(), "yyy年MM月dd日", true),

                            DocNo = row["doc_no"].ToString(),

                            NonEsCode = row["non_es_code"].ToString(),

                            TransCode = row["trans_code"].ToString()
                        };
                        experiencesList.Add(experience);
                    }
                    return Ok(new { Result = "Success", experiencesList });
                }
                else
                {
                    return Ok(new { Result = "Fail", experiencesList });
                }
            }
            catch (Exception ex)
            {
                // 如果出現異常，返回錯誤信息
                WriteLog.Log(String.Format("【memberRetireExperience Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【memberRetireExperience Fail】" + ex.Message);
            }
        }

        // 考績資料
        [HttpGet]
        [ActionName("memberPerformance")]
        public IHttpActionResult memberPerformance(string memberId)
        {
            List<PerformanceRes> perfList = new List<PerformanceRes>();

            string perfSql = @"
                            SELECT 
                                p_year, LTRIM(RTRIM(perform_code)) as perform_code, 
                                LTRIM(RTRIM(ideology_code)) as ideology_code, LTRIM(RTRIM(quality_code)) as quality_code, 
                                LTRIM(RTRIM(potential_code)) as potential_code, LTRIM(RTRIM(work_perform_code)) as work_perform_code, 
                                LTRIM(RTRIM(body_code)) as body_code, LTRIM(RTRIM(knowledge_code)) as knowledge_code, 
                                LTRIM(RTRIM(perform_rank)) as perform_rank, LTRIM(RTRIM(total_rank)) as total_rank
                            FROM 
                                Army.dbo.v_performance
                            WHERE 
                                member_id = @memberId
                            ORDER BY
                                p_year DESC";

            
            SqlParameter[] perfPara = new SqlParameter[]
            {
                new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId }
            };

            try
            {
                // 調用之前定義的方法執行查詢，返回一個DataTable
                DataTable perfTB = _dbHelper.ArmyWebExecuteQuery(perfSql, perfPara);

                if (perfTB != null && perfTB.Rows.Count > 0)
                {                    
                    foreach (DataRow row in perfTB.Rows)
                    {
                        PerformanceRes perf = new PerformanceRes()
                        {
                            PYear = row["p_year"].ToString(),
                            PerformCode = _codeToName.perfName(row["perform_code"].ToString()),
                            IdeologyCode = _codeToName.perfName(row["ideology_code"].ToString()),
                            QualityCode = _codeToName.perfName(row["quality_code"].ToString()),
                            PotentialCode = _codeToName.perfName(row["potential_code"].ToString()),
                            WorkPerformCode = _codeToName.perfName(row["work_perform_code"].ToString()),
                            BodyCode = _codeToName.perfName(row["body_code"].ToString()),
                            KnowledgeCode = _codeToName.perfName(row["knowledge_code"].ToString()),
                            PerformRank = row["perform_rank"].ToString(),
                            TotalRank = row["total_rank"].ToString()
                        };                   
                        perfList.Add(perf);
                    }
                                       
                    return Ok(new { Result = "Success", perfList });
                }
                else
                {
                    return Ok(new { Result = "Fail", perfList });
                }
            }
            catch (Exception ex)
            {
                // 如果出現異常，返回錯誤信息
                WriteLog.Log(String.Format("【memberPerformance Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【memberPerformance Fail】" + ex.Message);
            }
        }

        // 退伍考績資料
        [HttpGet]
        [ActionName("memberRetirePerformance")]
        public IHttpActionResult memberRetirePerformance(string memberId)
        {
            List<PerformanceRes> perfList = new List<PerformanceRes>();

            string perfSql = @"
                            SELECT 
                                p_year, LTRIM(RTRIM(perform_code)) as perform_code, 
                                LTRIM(RTRIM(ideology_code)) as ideology_code, LTRIM(RTRIM(quality_code)) as quality_code, 
                                LTRIM(RTRIM(potential_code)) as potential_code, LTRIM(RTRIM(work_perform_code)) as work_perform_code, 
                                LTRIM(RTRIM(body_code)) as body_code, LTRIM(RTRIM(knowledge_code)) as knowledge_code, 
                                LTRIM(RTRIM(perform_rank)) as perform_rank, LTRIM(RTRIM(total_rank)) as total_rank
                            FROM 
                                Army.dbo.v_performance_retire
                            WHERE 
                                member_id = @memberId
                            ORDER BY
                                p_year DESC";


            SqlParameter[] perfPara = new SqlParameter[]
            {
                new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId }
            };

            try
            {
                // 調用之前定義的方法執行查詢，返回一個DataTable
                DataTable perfTB = _dbHelper.ArmyWebExecuteQuery(perfSql, perfPara);

                if (perfTB != null && perfTB.Rows.Count > 0)
                {
                    foreach (DataRow row in perfTB.Rows)
                    {
                        PerformanceRes perf = new PerformanceRes()
                        {
                            PYear = row["p_year"].ToString(),
                            PerformCode = _codeToName.perfName(row["perform_code"].ToString()),
                            IdeologyCode = _codeToName.perfName(row["ideology_code"].ToString()),
                            QualityCode = _codeToName.perfName(row["quality_code"].ToString()),
                            PotentialCode = _codeToName.perfName(row["potential_code"].ToString()),
                            WorkPerformCode = _codeToName.perfName(row["work_perform_code"].ToString()),
                            BodyCode = _codeToName.perfName(row["body_code"].ToString()),
                            KnowledgeCode = _codeToName.perfName(row["knowledge_code"].ToString()),
                            PerformRank = row["perform_rank"].ToString(),
                            TotalRank = row["total_rank"].ToString()
                        };
                        perfList.Add(perf);
                    }

                    return Ok(new { Result = "Success", perfList });
                }
                else
                {
                    return Ok(new { Result = "Fail", perfList });
                }
            }
            catch (Exception ex)
            {
                // 如果出現異常，返回錯誤信息
                WriteLog.Log(String.Format("【memberRetirePerformance Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【memberRetirePerformance Fail】" + ex.Message);
            }
        }

        //PQPM
        [HttpGet]
        [ActionName("PQPM")]
        public IHttpActionResult PQPM(string memberId)
        {
            try
            {
                /*
                string memberDataSql = @"
                            with 
	                            temptable as (
		                            select 
			                            ve.member_id,ve.educ_code,ec.educ_name,es.school_desc,ve.school_code,ve.discipline_code,group_id= ROW_NUMBER() over (partition by ve.member_id order by study_date)
		                            from 
			                            Army.dbo.v_education as ve
		                            left join 
			                            Army.dbo.educ_code as ec on ec.educ_code = ve.educ_code
		                            left join 
			                            Army.dbo.educ_school as es on es.school_code = ve.school_code
		                            where 
			                            0=0
			                            and ve.educ_code in ('H','N') 
			                    )
                            SELECT
                                vmd.*,
                                REPLACE(vepj.item_no + '' + vepj.column_code + '' + t1.group_code + '' + vepj.serial_code, ' ', '') AS EsNumber
								,tt.school_code++isnull(tt.discipline_code,'') 'PQPM基礎教育(32)'
                            FROM
                                Army.dbo.v_member_data AS vmd
                            LEFT JOIN 
                                Army.dbo.v_es_person_join AS vepj ON vepj.member_id = vmd.member_id
                            LEFT JOIN 
                                Army.dbo.tgroup AS t1 ON t1.group_code = vepj.group_code
							right join 
								temptable as tt on tt.member_id = vmd.member_id
                            WHERE
                                vmd.member_id = @memberId and tt.group_id= @groupId";
                */
                string memberDataSql = @"
                            SELECT
                                vmr.*,
                                REPLACE(vepj.item_no + '' + vepj.column_code + '' + t1.group_code + '' + vepj.serial_code, ' ', '') AS EsNumber								
                            FROM
                                Army.dbo.v_member_data AS vmr
                            LEFT JOIN 
                                Army.dbo.v_es_person_join AS vepj ON vepj.member_id = vmr.member_id
                            LEFT JOIN 
                                Army.dbo.tgroup AS t1 ON t1.group_code = vepj.group_code							
                            WHERE
                                vmr.member_id = @memberId ";

                string basciEducSql = @"
                                with 
	                            temptable as (
		                             select 
			                            ve.member_id,ve.educ_code,ec.educ_name,es.school_desc,ve.school_code,ve.discipline_code,group_id= ROW_NUMBER() over (partition by ve.member_id order by study_date)
		                            from 
			                            Army.dbo.v_education as ve
		                            left join 
			                            Army.dbo.educ_code as ec on ec.educ_code = ve.educ_code
		                            left join 
			                            Army.dbo.educ_school as es on es.school_code = ve.school_code
		                            where 
			                            0=0
			                            and ve.educ_code in ('H','N')
			                    )
								select tt.school_code++isnull(tt.discipline_code,'') 'PQPM基礎教育(32)'
								from temptable as tt
								where tt.member_id = @memberId and tt.group_id = @groupId";

                string localDataSql = @"
                            SELECT
                                *
                            FROM
                                Army.dbo.v_address
                            WHERE
                                member_id = @memberId";

                SqlParameter[] memberDataPara = {
                    new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId }
                };
                SqlParameter[] basicEducPara = {
                    new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId },
                    new SqlParameter("@groupId", SqlDbType.VarChar) {Value = "1" }
                };
                SqlParameter[] localDataPara = { new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId } };


                DataTable memberTB = _dbHelper.ArmyWebExecuteQuery(memberDataSql, memberDataPara);
                DataTable basicEducTB = _dbHelper.ArmyWebExecuteQuery(basciEducSql, basicEducPara);
                DataTable localTB = _dbHelper.ArmyWebExecuteQuery(localDataSql, localDataPara);

                string basicEduc = string.Empty;
                string localCode = string.Empty;
                if (memberTB == null || memberTB.Rows.Count == 0)
                {
                    return Ok(new { Result = "No Memeber" });
                }
                if (basicEducTB != null && basicEducTB.Rows.Count != 0)
                {
                    basicEduc = basicEducTB.Rows[0]["PQPM基礎教育(32)"].ToString().Trim();
                }
                if (localTB != null && localTB.Rows.Count != 0)
                {
                    localCode = localTB.Rows[0]["locate"].ToString().Trim();
                }


                // 時間格式處理
                string birthday = _codeToName.dateTimeTran(memberTB.Rows[0]["birthday"].ToString().Trim(), "yyy年MM月dd日", true);                          // 生日
                string salary_date = _codeToName.dateTimeTran(memberTB.Rows[0]["salary_date"].ToString().Trim(), "yyy年MM月dd日", true);                    // 任官日期
                string rank_date = _codeToName.dateTimeTran(memberTB.Rows[0]["rank_date"].ToString().Trim(), "yyy年MM月dd日", true);                        // 現階日期
                string pay_date = _codeToName.dateTimeTran(memberTB.Rows[0]["pay_date"].ToString().Trim(), "yyy年MM月dd日", true);                 // 任職日期
                string campaign_date = _codeToName.dateTimeTran(memberTB.Rows[0]["campaign_date"].ToString().Trim(), "yyy年MM月dd日", true);       // 入伍日期
                string update_date = _codeToName.dateTimeTran(memberTB.Rows[0]["update_date"].ToString().Trim(), "yyy年MM月dd日", true);                //異動日期
                string volun_officer_date = _codeToName.dateTimeTran(memberTB.Rows[0]["volun_officer_date"].ToString().Trim(), "yyy年MM月dd日", true);      // 轉服志願軍官日期
                string volun_sergeant_date = _codeToName.dateTimeTran(memberTB.Rows[0]["volun_sergeant_date"].ToString().Trim(), "yyy年MM月dd日", true);    // 轉服志願士官日期
                string volun_soldier_date = _codeToName.dateTimeTran(memberTB.Rows[0]["volun_soldier_date"].ToString().Trim(), "yyy年MM月dd日", true);      // 轉服志願士兵日期
                string again_campaign_date = _codeToName.dateTimeTran(memberTB.Rows[0]["again_campaign_date"].ToString().Trim(), "yyy年MM月dd日", true);     // 再入營日期
                string stop_volunteer_date = _codeToName.dateTimeTran(memberTB.Rows[0]["stop_volunteer_date"].ToString().Trim(), "yyy年MM月dd日", true);   // 廢止志願役日期

                PQPMRes basicMemData = new PQPMRes
                {
                    MemberName = memberTB.Rows[0]["member_name"].ToString().Trim(),

                    MemberId = memberTB.Rows[0]["member_id"].ToString().Trim(),

                    CampaignSerial = memberTB.Rows[0]["campaign_serial"].ToString().Trim(),

                    UnitCode = memberTB.Rows[0]["unit_code"].ToString().Trim(),

                    GroupCode = memberTB.Rows[0]["group_code"].ToString().Trim(),

                    SalaryDate = salary_date,

                    Birthday = birthday,

                    RankCode = memberTB.Rows[0]["rank_code"].ToString().Trim(),

                    CommonEducCode = memberTB.Rows[0]["common_educ_code"].ToString().Trim(),

                    LocalCode = localCode,

                    RankDate = rank_date,

                    BasicEdu = basicEduc, //基礎教育 class_code	基礎教育期別?  discipline_code	基礎教育科系?

                    NonEsCode = memberTB.Rows[0]["non_es_code"].ToString().Trim(),

                    SupplyRank = memberTB.Rows[0]["supply_rank"].ToString().Trim(),

                    MilitaryEducCode = memberTB.Rows[0]["military_educ_code"].ToString().Trim(),

                    EsRankCode = memberTB.Rows[0]["es_rank_code"].ToString().Trim(),

                    PayUnitCode = memberTB.Rows[0]["pay_unit_code"].ToString().Trim(),

                    BloodType = memberTB.Rows[0]["blood_type"].ToString().Trim(),

                    EsSkillCode = memberTB.Rows[0]["es_skill_code"].ToString().Trim(),

                    PayRemark = memberTB.Rows[0]["pay_remark"].ToString().Trim(),

                    IqScore = memberTB.Rows[0]["iq_score"].ToString().Trim(),

                    TitleCode = memberTB.Rows[0]["title_code"].ToString().Trim(),

                    BonusCode = memberTB.Rows[0]["bonus_code"].ToString().Trim(), // 勤務性加給  專業給付?

                    UnPromoteCode = memberTB.Rows[0]["un_promote_code"].ToString().Trim(),

                    PayDate = pay_date,

                    EsNumber = memberTB.Rows[0]["EsNumber"].ToString().Trim(), // 編制號

                    OriginalPay = memberTB.Rows[0]["original_pay"].ToString().Trim(),

                    CampaignCode = memberTB.Rows[0]["campaign_code"].ToString().Trim(),

                    MSkillCode = memberTB.Rows[0]["m_skill_code"].ToString().Trim(),

                    CornerCode = memberTB.Rows[0]["corner_code"].ToString().Trim(),

                    ServiceCode = memberTB.Rows[0]["service_code"].ToString().Trim(),

                    WorkStatus = memberTB.Rows[0]["work_status"].ToString().Trim(),

                    RecampaignMonth = memberTB.Rows[0]["recampaign_month"].ToString().Trim(),

                    UpdateDate = update_date,

                    TransCode = memberTB.Rows[0]["trans_code"].ToString().Trim(),

                    MainBonus = memberTB.Rows[0]["main_bonus"].ToString().Trim(),

                    VolunOfficerDate = volun_officer_date,

                    VolunSergeantDate = volun_sergeant_date,

                    VolunSoldierDate = volun_soldier_date,

                    AgainCampaignDate = again_campaign_date,

                    StopVolunteerDate = stop_volunteer_date,

                    LocalMark = memberTB.Rows[0]["local_mark"].ToString().Trim(),

                    RetireDate = " "
                };
                return Ok(new { Result = "Success", basicMemData });
            }
            catch (Exception ex) 
            {
                WriteLog.Log(String.Format("【PQPW Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【PQPW Fail】" + ex.Message);
            }
        }

        //retirePQPM
        [HttpGet]
        [ActionName("retirePQPM")]
        public IHttpActionResult retirePQPM(string memberId)
        {
            try
            {  /*
                string memberDataSql = @"
                            with 
	                            temptable as (
		                            select 
			                            ve.member_id,ve.educ_code,ec.educ_name,es.school_desc,ve.school_code,ve.discipline_code,group_id= ROW_NUMBER() over (partition by ve.member_id order by study_date)
		                            from 
			                            Army.dbo.v_education_retire as ve
		                            left join 
			                            Army.dbo.educ_code as ec on ec.educ_code = ve.educ_code
		                            left join 
			                            Army.dbo.educ_school as es on es.school_code = ve.school_code
		                            where 
			                            0=0
			                            and ve.educ_code in ('H','N') 
			                    )
                            SELECT
                                vmr.*,
                                REPLACE(vepj.item_no + '' + vepj.column_code + '' + t1.group_code + '' + vepj.serial_code, ' ', '') AS EsNumber
								,tt.school_code++isnull(tt.discipline_code,'') 'PQPM基礎教育(32)'
                            FROM
                                Army.dbo.v_member_retire AS vmr
                            LEFT JOIN 
                                Army.dbo.v_es_person_join AS vepj ON vepj.member_id = vmr.member_id
                            LEFT JOIN 
                                Army.dbo.tgroup AS t1 ON t1.group_code = vepj.group_code
							right join 
								temptable as tt on tt.member_id = vmr.member_id
                            WHERE
                                vmr.member_id = @memberId and tt.group_id = @groupId";
                */

                string memberDataSql = @"
                            SELECT
                                vmr.*,
                                REPLACE(vepj.item_no + '' + vepj.column_code + '' + t1.group_code + '' + vepj.serial_code, ' ', '') AS EsNumber								
                            FROM
                                Army.dbo.v_member_retire AS vmr
                            LEFT JOIN 
                                Army.dbo.v_es_person_join AS vepj ON vepj.member_id = vmr.member_id
                            LEFT JOIN 
                                Army.dbo.tgroup AS t1 ON t1.group_code = vepj.group_code							
                            WHERE
                                vmr.member_id = @memberId ";

                string basciEducSql = @"
                                with 
	                            temptable as (
		                             select 
			                            ve.member_id,ve.educ_code,ec.educ_name,es.school_desc,ve.school_code,ve.discipline_code,group_id= ROW_NUMBER() over (partition by ve.member_id order by study_date)
		                            from 
			                            Army.dbo.v_education_retire as ve
		                            left join 
			                            Army.dbo.educ_code as ec on ec.educ_code = ve.educ_code
		                            left join 
			                            Army.dbo.educ_school as es on es.school_code = ve.school_code
		                            where 
			                            0=0
			                            and ve.educ_code in ('H','N')
			                    )
								select tt.school_code++isnull(tt.discipline_code,'') 'PQPM基礎教育(32)'
								from temptable as tt
								where tt.member_id = @memberId and tt.group_id = @groupId";

                string localDataSql = @"
                            SELECT
                                *
                            FROM
                                Army.dbo.v_address_retire
                            WHERE
                                member_id = @memberId";
               
                

                SqlParameter[] memberDataPara = { 
                    new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId }
                };
                SqlParameter[] basicEducPara = {
                    new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId },
                    new SqlParameter("@groupId", SqlDbType.VarChar) {Value = "1" }
                };
                SqlParameter[] localDataPara = { new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId } };
               

                DataTable memberTB = _dbHelper.ArmyWebExecuteQuery(memberDataSql, memberDataPara);
                DataTable basicEducTB = _dbHelper.ArmyWebExecuteQuery(basciEducSql, basicEducPara);
                DataTable localTB = _dbHelper.ArmyWebExecuteQuery(localDataSql, localDataPara);

                string basicEduc = string.Empty;
                string localCode = string.Empty;
                if (memberTB == null || memberTB.Rows.Count == 0)
                {
                    return Ok(new { Result = "No Memeber" });
                }
                if (basicEducTB != null && basicEducTB.Rows.Count != 0)
                {
                    basicEduc = basicEducTB.Rows[0]["PQPM基礎教育(32)"].ToString().Trim();
                }
                if (localTB != null && localTB.Rows.Count != 0)
                {
                    localCode = localTB.Rows[0]["locate"].ToString().Trim();
                }                

                // 時間格式處理
                string birthday = _codeToName.dateTimeTran(memberTB.Rows[0]["birthday"].ToString().Trim(), "yyy年MM月dd日", true);                           // 生日
                string salary_date = _codeToName.dateTimeTran(memberTB.Rows[0]["salary_date"].ToString().Trim(), "yyy年MM月dd日", true);                     // 任官日期
                string rank_date = _codeToName.dateTimeTran(memberTB.Rows[0]["rank_date"].ToString().Trim(), "yyy年MM月dd日", true);                         // 現階日期
                string pay_date = _codeToName.dateTimeTran(memberTB.Rows[0]["pay_date"].ToString().Trim(), "yyy年MM月dd日", true);                         // 任職日期
                string campaign_date = _codeToName.dateTimeTran(memberTB.Rows[0]["campaign_date"].ToString().Trim(), "yyy年MM月dd日", true);               // 入伍日期
                string update_date = _codeToName.dateTimeTran(memberTB.Rows[0]["update_date"].ToString().Trim(), "yyy年MM月dd日", true);                        // 異動日期
                string volun_officer_date = _codeToName.dateTimeTran(memberTB.Rows[0]["volun_officer_date"].ToString().Trim(), "yyy年MM月dd日", true);       // 轉服志願軍官日期
                string volun_sergeant_date = _codeToName.dateTimeTran(memberTB.Rows[0]["volun_sergeant_date"].ToString().Trim(), "yyy年MM月dd日", true);      // 轉服志願士官日期
                string volun_soldier_date = _codeToName.dateTimeTran(memberTB.Rows[0]["volun_soldier_date"].ToString().Trim(), "yyy年MM月dd日", true);       // 轉服志願士兵日期
                string again_campaign_date = _codeToName.dateTimeTran(memberTB.Rows[0]["again_campaign_date"].ToString().Trim(), "yyy年MM月dd日", true);     // 再入營日期
                string stop_volunteer_date = _codeToName.dateTimeTran(memberTB.Rows[0]["stop_volunteer_date"].ToString().Trim(), "yyy年MM月dd日", true);   // 廢止志願役日期
                string retire_date = _codeToName.dateTimeTran(memberTB.Rows[0]["retire_date"].ToString().Trim(), "yyy年MM月dd日", true);                   // 退伍日期

                PQPMRes basicMemData = new PQPMRes
                {
                    MemberName = memberTB.Rows[0]["member_name"].ToString().Trim(),

                    MemberId = memberTB.Rows[0]["member_id"].ToString().Trim(),

                    CampaignSerial = memberTB.Rows[0]["campaign_serial"].ToString().Trim(),

                    UnitCode = memberTB.Rows[0]["unit_code"].ToString().Trim(),

                    GroupCode = memberTB.Rows[0]["group_code"].ToString().Trim(),

                    SalaryDate = salary_date,

                    Birthday = birthday,

                    RankCode = memberTB.Rows[0]["rank_code"].ToString().Trim(),

                    CommonEducCode = memberTB.Rows[0]["common_educ_code"].ToString().Trim(),

                    LocalCode = localCode,

                    RankDate = rank_date,

                    BasicEdu = basicEduc, //基礎教育 class_code	基礎教育期別?  discipline_code	基礎教育科系?

                    NonEsCode = memberTB.Rows[0]["non_es_code"].ToString().Trim(),

                    SupplyRank = memberTB.Rows[0]["supply_rank"].ToString().Trim(),

                    MilitaryEducCode = memberTB.Rows[0]["military_educ_code"].ToString().Trim(),

                    EsRankCode = memberTB.Rows[0]["es_rank_code"].ToString().Trim(),

                    PayUnitCode = memberTB.Rows[0]["pay_unit_code"].ToString().Trim(),

                    BloodType = memberTB.Rows[0]["blood_type"].ToString().Trim(),

                    EsSkillCode = memberTB.Rows[0]["es_skill_code"].ToString().Trim(),

                    PayRemark = memberTB.Rows[0]["pay_remark"].ToString().Trim(),

                    IqScore = memberTB.Rows[0]["iq_score"].ToString().Trim(),

                    TitleCode = memberTB.Rows[0]["title_code"].ToString().Trim(),

                    BonusCode = memberTB.Rows[0]["bonus_code"].ToString().Trim(), // 勤務性加給  專業給付?

                    UnPromoteCode = memberTB.Rows[0]["un_promote_code"].ToString().Trim(),

                    PayDate = pay_date,

                    EsNumber = memberTB.Rows[0]["EsNumber"].ToString().Trim(), // 編制號

                    OriginalPay = memberTB.Rows[0]["original_pay"].ToString().Trim(),

                    CampaignCode = memberTB.Rows[0]["campaign_code"].ToString().Trim(),

                    MSkillCode = memberTB.Rows[0]["m_skill_code"].ToString().Trim(),

                    CornerCode = memberTB.Rows[0]["corner_code"].ToString().Trim(),

                    ServiceCode = memberTB.Rows[0]["service_code"].ToString().Trim(),

                    WorkStatus = memberTB.Rows[0]["work_status"].ToString().Trim(),

                    RecampaignMonth = memberTB.Rows[0]["recampaign_month"].ToString().Trim(),

                    UpdateDate = update_date,

                    TransCode = memberTB.Rows[0]["trans_code"].ToString().Trim(),

                    MainBonus = memberTB.Rows[0]["main_bonus"].ToString().Trim(),

                    VolunOfficerDate = volun_officer_date,

                    VolunSergeantDate = volun_sergeant_date,

                    VolunSoldierDate = volun_soldier_date,

                    AgainCampaignDate = again_campaign_date,

                    StopVolunteerDate = stop_volunteer_date,

                    LocalMark = memberTB.Rows[0]["local_mark"].ToString().Trim(),

                    RetireDate = retire_date,
                };
                return Ok(new { Result = "Success", basicMemData });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【retirePQPM Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【retirePQPM Fail】" + ex.Message);
            }
        }

        //relayPQPM
        [HttpGet]
        [ActionName("relayPQPM")]
        public IHttpActionResult relayPQPM(string memberId)
        {
            try
            {
                /*
                string memberDataSql = @"
                            with 
	                            temptable as (
		                            select 
			                            ve.member_id,ve.educ_code,ec.educ_name,es.school_desc,ve.school_code,ve.discipline_code,group_id= ROW_NUMBER() over (partition by ve.member_id order by study_date)
		                            from 
			                            Army.dbo.v_education as ve
		                            left join 
			                            Army.dbo.educ_code as ec on ec.educ_code = ve.educ_code
		                            left join 
			                            Army.dbo.educ_school as es on es.school_code = ve.school_code
		                            where 
			                            0=0
			                            and ve.educ_code in ('H','N') 
			                    )
                            SELECT
                                vmr.*,
                                REPLACE(vepj.item_no + '' + vepj.column_code + '' + t1.group_code + '' + vepj.serial_code, ' ', '') AS EsNumber
								,tt.school_code++isnull(tt.discipline_code,'') 'PQPM基礎教育(32)'
                            FROM
                                Army.dbo.v_member_relay AS vmr
                            LEFT JOIN 
                                Army.dbo.v_es_person_join AS vepj ON vepj.member_id = vmr.member_id
                            LEFT JOIN 
                                Army.dbo.tgroup AS t1 ON t1.group_code = vepj.group_code
							right join 
								temptable as tt on tt.member_id = vmr.member_id
                            WHERE
                                vmr.member_id = @memberId and tt.group_id= @groupId";
                */

                string memberDataSql = @"
                            SELECT
                                vmr.*,
                                REPLACE(vepj.item_no + '' + vepj.column_code + '' + t1.group_code + '' + vepj.serial_code, ' ', '') AS EsNumber								
                            FROM
                                Army.v_member_relay AS vmr
                            LEFT JOIN 
                                Army.dbo.v_es_person_join AS vepj ON vepj.member_id = vmr.member_id
                            LEFT JOIN 
                                Army.dbo.tgroup AS t1 ON t1.group_code = vepj.group_code							
                            WHERE
                                vmr.member_id = @memberId ";

                string basciEducSql = @"
                                with 
	                            temptable as (
		                             select 
			                            ve.member_id,ve.educ_code,ec.educ_name,es.school_desc,ve.school_code,ve.discipline_code,group_id= ROW_NUMBER() over (partition by ve.member_id order by study_date)
		                            from 
			                            Army.dbo.v_education as ve
		                            left join 
			                            Army.dbo.educ_code as ec on ec.educ_code = ve.educ_code
		                            left join 
			                            Army.dbo.educ_school as es on es.school_code = ve.school_code
		                            where 
			                            0=0
			                            and ve.educ_code in ('H','N')
			                    )
								select tt.school_code++isnull(tt.discipline_code,'') 'PQPM基礎教育(32)'
								from temptable as tt
								where tt.member_id = @memberId and tt.group_id = @groupId";

                string localDataSql = @"
                            SELECT
                                *
                            FROM
                                Army.dbo.v_address
                            WHERE
                                member_id = @memberId";



                SqlParameter[] memberDataPara = {
                    new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId }
                };
                SqlParameter[] basicEducPara = {
                    new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId },
                    new SqlParameter("@groupId", SqlDbType.VarChar) {Value = "1" }
                };
                SqlParameter[] localDataPara = { new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId } };


                DataTable memberTB = _dbHelper.ArmyWebExecuteQuery(memberDataSql, memberDataPara);
                DataTable basicEducTB = _dbHelper.ArmyWebExecuteQuery(basciEducSql, basicEducPara);
                DataTable localTB = _dbHelper.ArmyWebExecuteQuery(localDataSql, localDataPara);

                string basicEduc = string.Empty;
                string localCode = string.Empty;
                if (memberTB == null || memberTB.Rows.Count == 0)
                {
                    return Ok(new { Result = "No Memeber" });
                }
                if (basicEducTB != null && basicEducTB.Rows.Count != 0)
                {
                    basicEduc = basicEducTB.Rows[0]["PQPM基礎教育(32)"].ToString().Trim();
                }
                if (localTB != null && localTB.Rows.Count != 0)
                {
                    localCode = localTB.Rows[0]["locate"].ToString().Trim();
                }


                // 時間格式處理
                string birthday = _codeToName.dateTimeTran(memberTB.Rows[0]["birthday"].ToString().Trim(), "yyy年MM月dd日", true);                           // 生日
                string salary_date = _codeToName.dateTimeTran(memberTB.Rows[0]["salary_date"].ToString().Trim(), "yyy年MM月dd日", true);                     // 任官日期
                string rank_date = _codeToName.dateTimeTran(memberTB.Rows[0]["rank_date"].ToString().Trim(), "yyy年MM月dd日", true);                         // 現階日期
                string pay_date = _codeToName.dateTimeTran(memberTB.Rows[0]["pay_date"].ToString().Trim(), "yyy年MM月dd日", true);                         // 任職日期
                string campaign_date = _codeToName.dateTimeTran(memberTB.Rows[0]["campaign_date"].ToString().Trim(), "yyy年MM月dd日", true);               // 入伍日期
                string update_date = _codeToName.dateTimeTran(memberTB.Rows[0]["update_date"].ToString().Trim(), "yyy年MM月dd日", true);                         //異動日期
                string volun_officer_date = _codeToName.dateTimeTran(memberTB.Rows[0]["volun_officer_date"].ToString().Trim(), "yyy年MM月dd日", true);       // 轉服志願軍官日期
                string volun_sergeant_date = _codeToName.dateTimeTran(memberTB.Rows[0]["volun_sergeant_date"].ToString().Trim(), "yyy年MM月dd日", true);      // 轉服志願士官日期
                string volun_soldier_date = _codeToName.dateTimeTran(memberTB.Rows[0]["volun_soldier_date"].ToString().Trim(), "yyy年MM月dd日", true);       // 轉服志願士兵日期
                string again_campaign_date = _codeToName.dateTimeTran(memberTB.Rows[0]["again_campaign_date"].ToString().Trim(), "yyy年MM月dd日", true);     // 再入營日期
                string stop_volunteer_date = _codeToName.dateTimeTran(memberTB.Rows[0]["stop_volunteer_date"].ToString().Trim(), "yyy年MM月dd日", true);   // 廢止志願役日期
                string retire_date = _codeToName.dateTimeTran(memberTB.Rows[0]["retire_date"].ToString().Trim(), "yyy年MM月dd日", true);                   // 退伍日期

                PQPMRes basicMemData = new PQPMRes
                {
                    MemberName = memberTB.Rows[0]["member_name"].ToString().Trim(),

                    MemberId = memberTB.Rows[0]["member_id"].ToString().Trim(),

                    CampaignSerial = memberTB.Rows[0]["campaign_serial"].ToString().Trim(),

                    UnitCode = memberTB.Rows[0]["unit_code"].ToString().Trim(),

                    GroupCode = memberTB.Rows[0]["group_code"].ToString().Trim(),

                    SalaryDate = salary_date,

                    Birthday = birthday,

                    RankCode = memberTB.Rows[0]["rank_code"].ToString().Trim(),

                    CommonEducCode = memberTB.Rows[0]["common_educ_code"].ToString().Trim(),

                    LocalCode = localCode,

                    RankDate = rank_date,

                    BasicEdu = basicEduc, //基礎教育 class_code	基礎教育期別?  discipline_code	基礎教育科系?

                    NonEsCode = memberTB.Rows[0]["non_es_code"].ToString().Trim(),

                    SupplyRank = memberTB.Rows[0]["supply_rank"].ToString().Trim(),

                    MilitaryEducCode = memberTB.Rows[0]["military_educ_code"].ToString().Trim(),

                    EsRankCode = memberTB.Rows[0]["es_rank_code"].ToString().Trim(),

                    PayUnitCode = memberTB.Rows[0]["pay_unit_code"].ToString().Trim(),

                    BloodType = memberTB.Rows[0]["blood_type"].ToString().Trim(),

                    EsSkillCode = memberTB.Rows[0]["es_skill_code"].ToString().Trim(),

                    PayRemark = memberTB.Rows[0]["pay_remark"].ToString().Trim(),

                    IqScore = memberTB.Rows[0]["iq_score"].ToString().Trim(),

                    TitleCode = memberTB.Rows[0]["title_code"].ToString().Trim(),

                    BonusCode = memberTB.Rows[0]["bonus_code"].ToString().Trim(), // 勤務性加給  專業給付?

                    UnPromoteCode = memberTB.Rows[0]["un_promote_code"].ToString().Trim(),

                    PayDate = pay_date,

                    EsNumber = memberTB.Rows[0]["EsNumber"].ToString().Trim(), // 編制號

                    OriginalPay = memberTB.Rows[0]["original_pay"].ToString().Trim(),

                    CampaignCode = memberTB.Rows[0]["campaign_code"].ToString().Trim(),

                    MSkillCode = memberTB.Rows[0]["m_skill_code"].ToString().Trim(),

                    CornerCode = memberTB.Rows[0]["corner_code"].ToString().Trim(),

                    ServiceCode = memberTB.Rows[0]["service_code"].ToString().Trim(),

                    WorkStatus = memberTB.Rows[0]["work_status"].ToString().Trim(),

                    RecampaignMonth = memberTB.Rows[0]["recampaign_month"].ToString().Trim(),

                    UpdateDate = update_date,

                    TransCode = memberTB.Rows[0]["trans_code"].ToString().Trim(),

                    MainBonus = memberTB.Rows[0]["main_bonus"].ToString().Trim(),

                    VolunOfficerDate = volun_officer_date,

                    VolunSergeantDate = volun_sergeant_date,

                    VolunSoldierDate = volun_soldier_date,

                    AgainCampaignDate = again_campaign_date,

                    StopVolunteerDate = stop_volunteer_date,

                    LocalMark = memberTB.Rows[0]["local_mark"].ToString().Trim(),

                    RetireDate = retire_date,
                };
                return Ok(new { Result = "Success", basicMemData });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【relayPQPM Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【relayPQPM Fail】" + ex.Message);
            }
        }

        //教育資料
        [HttpGet]
        [ActionName("memberEducation")]
        public IHttpActionResult memberEducation(string memberId)
        {
            List<EducationRes> eduList = new List<EducationRes>();
            string eduSql = @"
                            SELECT 
                                LTRIM(RTRIM(country_code)) as country_code, LTRIM(RTRIM(school_code)) as school_code, 
                                LTRIM(RTRIM(discipline_code)) as discipline_code, LTRIM(RTRIM(class_code)) as class_code, 
                                LTRIM(RTRIM(period_no)) as period_no, LTRIM(RTRIM(educ_code)) as educ_code, 
                                study_date, LTRIM(RTRIM(graduate_date)) as graduate_date, 
                                LTRIM(RTRIM(classmate_amt)) as classmate_amt, FORMAT(CAST(graduate_score AS FLOAT) / 100, 'N2') as graduate_score, 
                                LTRIM(RTRIM(graduate_rank)) as graduate_rank, FORMAT(CAST(thesis_score AS FLOAT) / 100, 'N2') as thesis_score
                            FROM 
                                Army.dbo.v_education 
                            WHERE 
                                member_id = @memberId
                            ORDER BY
                                study_date DESC";

            
            SqlParameter[] eduParameters = new SqlParameter[]
            {
                new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId }
            };

            try
            {
                DataTable eduTB = _dbHelper.ArmyWebExecuteQuery(eduSql, eduParameters);

                if (eduTB != null && eduTB.Rows.Count > 0)
                {
                    
                    foreach(DataRow row in eduTB.Rows)
                    {
                        EducationRes eduRes = new EducationRes() 
                        {
                            CountryCode = _codeToName.countryName(row["country_code"].ToString()),

                            SchoolCode = _codeToName.schoolName(row["school_code"].ToString()),

                            DisciplineCode = _codeToName.disciplineName(row["discipline_code"].ToString()),

                            ClassCode  = _codeToName.className(row["class_code"].ToString()),

                            PeriodNo  = row["period_no"].ToString(),

                            EducCode = _codeToName.educName(row["educ_code"].ToString()),

                            StudyDate  = _codeToName.dateTimeTran(row["study_date"].ToString(), "yyy年MM月dd日", true),

                            GraduateDate = _codeToName.dateTimeTran(row["graduate_date"].ToString(), "yyy年MM月dd日", true),

                            ClassmateAmt = row["classmate_amt"].ToString(),

                            GraduateScore = row["graduate_score"].ToString(),

                            GraduateRank = row["graduate_rank"].ToString(),

                            ThesisScore = row["thesis_score"].ToString()
                        };

                        eduList.Add(eduRes);                       
                    }
                    
                    return Ok(new { Result = "Success", eduList });
                }
                else
                {
                    return Ok(new { Result = "Fail", eduList });
                }
            }
            catch (Exception ex)
            {
                // 如果出現異常，返回錯誤信息
                WriteLog.Log(String.Format("【memberEducation Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【memberEducation Fail】" + ex.Message);
            }
        }

        //退伍教育資料
        [HttpGet]
        [ActionName("memberRetireEducation")]
        public IHttpActionResult memberRetireEducation(string memberId)
        {
            List<EducationRes> eduList = new List<EducationRes>();
            string eduSql = @"
                            SELECT 
                                LTRIM(RTRIM(country_code)) as country_code, LTRIM(RTRIM(school_code)) as school_code, 
                                LTRIM(RTRIM(discipline_code)) as discipline_code, LTRIM(RTRIM(class_code)) as class_code, 
                                LTRIM(RTRIM(period_no)) as period_no, LTRIM(RTRIM(educ_code)) as educ_code, 
                                study_date, LTRIM(RTRIM(graduate_date)) as graduate_date, 
                                LTRIM(RTRIM(classmate_amt)) as classmate_amt, FORMAT(CAST(graduate_score AS FLOAT) / 100, 'N2') as graduate_score, 
                                LTRIM(RTRIM(graduate_rank)) as graduate_rank, FORMAT(CAST(thesis_score AS FLOAT) / 100, 'N2') as thesis_score
                            FROM 
                                Army.dbo.v_education_retire
                            WHERE 
                                member_id = @memberId
                            ORDER BY
                                study_date DESC";


            SqlParameter[] eduParameters = new SqlParameter[]
            {
                new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId }
            };

            try
            {
                DataTable eduTB = _dbHelper.ArmyWebExecuteQuery(eduSql, eduParameters);

                if (eduTB != null && eduTB.Rows.Count > 0)
                {
                    
                    foreach (DataRow row in eduTB.Rows)
                    {
                        EducationRes eduRes = new EducationRes()
                        {
                            CountryCode = _codeToName.countryName(row["country_code"].ToString()),

                            SchoolCode = _codeToName.schoolName(row["school_code"].ToString()),

                            DisciplineCode = _codeToName.disciplineName(row["discipline_code"].ToString()),

                            ClassCode  = _codeToName.className(row["class_code"].ToString()),

                            PeriodNo  = row["period_no"].ToString(),

                            EducCode = _codeToName.educName(row["educ_code"].ToString()),

                            StudyDate  = _codeToName.dateTimeTran(row["study_date"].ToString(), "yyy年MM月dd日", true),

                            GraduateDate = _codeToName.dateTimeTran(row["graduate_date"].ToString(), "yyy年MM月dd日", true),

                            ClassmateAmt = row["classmate_amt"].ToString(),

                            GraduateScore = row["graduate_score"].ToString(),

                            GraduateRank = row["graduate_rank"].ToString(),

                            ThesisScore = row["thesis_score"].ToString()
                        };

                        eduList.Add(eduRes);
                    }

                    return Ok(new { Result = "Success", eduList });
                }
                else
                {
                    return Ok(new { Result = "Fail", eduList });
                }
            }
            catch (Exception ex)
            {
                // 如果出現異常，返回錯誤信息
                WriteLog.Log(String.Format("【memberRetireEducation Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【memberRetireEducation Fail】" + ex.Message);
            }
        }

        // 獎懲資料
        [HttpGet]
        [ActionName("memberEncourage")]
        public IHttpActionResult memberEncourage(string memberId)
        {
            List<EncourageRes> encourageList = new List<EncourageRes>();
            
            string encourageSql = @"DECLARE @true bit = 1, @false bit = 0
                                    SELECT
	                                    @true 'active',
	                                    vmu2.unit_code 'enc_unit_code',
	                                    rtrim(vmu2.unit_title) 'enc_unit',
	                                    rtrim(r.rank_title) 'rank',
	                                    dbo.DateToString(en.doc_date, 2) 'doc_date',
	                                    en.doc_no,
                                    case 
	                                    when en.enc_reason_code ='A' then '撤職'
	                                    when en.enc_reason_code = 'H' then '罰薪'+rtrim(en.enc_metal_level)+rtrim(en.enc_star_level)+'%'
	                                    when en.enc_reason_code in ('J') then '檢束'+rtrim(en.enc_metal_level)+rtrim(en.enc_star_level)+'日'
	                                    when en.enc_reason_code in ('K') then rtrim(em1.enc_metal_desc)+rtrim(en.enc_metal_level)+rtrim(en.enc_star_level)+'月'
	                                    when en.enc_reason_code in ('L') then '禁閉'+rtrim(en.enc_metal_level)+rtrim(en.enc_star_level)+'日'
	                                    when en.enc_reason_code in ('M') then '悔過'+rtrim(en.enc_metal_level)+rtrim(en.enc_star_level)+'日'
	                                    when en.enc_reason_code in ('P') then '罰勤'+rtrim(en.enc_metal_level)+rtrim(en.enc_star_level)+'日'
	                                    when en.enc_reason_code in ('Q') then '禁足'+rtrim(en.enc_metal_level)+rtrim(en.enc_star_level)+'日'
	                                    when en.enc_reason_code in ('S') and en.enc_metal_code in ('13') then '公務人員懲戒法降級'+rtrim(en.enc_metal_level)+rtrim(en.enc_star_level)+'年'
	                                    when en.enc_reason_code in ('T') and en.enc_metal_level in ('2') then '罰站'+'1'+rtrim(en.enc_metal_level)+rtrim(en.enc_star_level)+'分鐘'
	                                    when en.enc_reason_code in ('T') then '罰站'+rtrim(en.enc_metal_level)+rtrim(en.enc_star_level)+'分鐘'
	                                    when en.enc_reason_code ='X' then '事蹟存記'
	                                    when en.enc_reason_code ='Y' then '不良事蹟存記'
	                                    when en.enc_reason_code ='Z' then '言詞申誡'
	                                    when en.enc_reason_code='9' AND rtrim(en.enc_star_level)='A'
	                                        THEN '獎金'+rtrim(en.enc_metal_code)+rtrim(en.enc_metal_level)+'0000元'
	                                    when en.enc_reason_code='9' AND rtrim(en.enc_star_level)='B'
	                                        THEN '獎金'+rtrim(en.enc_metal_code)+rtrim(en.enc_metal_level)+'000元'
	                                    when en.enc_reason_code='9'
	                                        THEN '獎金'+rtrim(en.enc_metal_code)+rtrim(en.enc_metal_level)+rtrim(en.enc_star_level)+'元'
	                                    ELSE em1.enc_metal_desc
                                    END 'enc_metal_desc',
	                                    rtrim(eg.enc_group_desc) 'enc_group',
	                                    en.doc_item,
	                                    en.enc_point_ident,
	                                    replace(dbo.DateToString(en.enc_cancel_date, 2), '-11/01/01', '') 'enc_cancel_date',
	                                    en.enc_cancel_doc,
	                                    rtrim(vmu1.unit_title) 'unit',
	                                    en.doc_ch 'enc_doc_ch',
	                                    en.enc_reason_ch 'enc_reason_ch'
                                    FROM [Army].[dbo].[v_encourage] en
	                                    LEFT JOIN [Army].[dbo].[enco_metal] as em1 on em1.enc_reason_code+em1.enc_metal_code+em1.enc_metal_level+em1.enc_star_level = en.enc_reason_code+en.enc_metal_code+en.enc_metal_level+en.enc_star_level
	                                    LEFT JOIN [Army].[dbo].[v_mu_unit] as vmu1 on vmu1.unit_code = en.unit_code
	                                    LEFT JOIN [Army].[dbo].[v_mu_unit] as vmu2 on vmu2.unit_code = en.enc_unit_code
	                                    LEFT JOIN [Army].[dbo].[rank] as r on r.rank_code = en.rank_code
	                                    LEFT JOIN [Army].[dbo].[enco_group] as eg on eg.enc_group_code = en.enc_group
	                                    LEFT JOIN [OrderSystem].[dbo].[Unit] as ou on ou.UnitCode = vmu1.unit_code
                                    WHERE member_id = @memberId
                                    ORDER BY doc_date DESC";
            
            /*
            string encourageSql = @"
                            SELECT 
                                LTRIM(RTRIM(unit_code)) as unit_code, LTRIM(RTRIM(enc_unit_code)) as enc_unit_code, 
                                LTRIM(RTRIM(rank_code)) as rank_code, doc_date, 
                                LTRIM(RTRIM(doc_no)) as doc_no, LTRIM(RTRIM(enc_reason_code)) as enc_reason_code, 
                                LTRIM(RTRIM(enc_group)) as enc_group, LTRIM(RTRIM(doc_item)) as doc_item, 
                                LTRIM(RTRIM(enc_point_ident)) as enc_point_ident, LTRIM(RTRIM(enc_cancel_date)) as enc_cancel_date, 
                                LTRIM(RTRIM(enc_cancel_doc)) as enc_cancel_doc, LTRIM(RTRIM(doc_ch)) as doc_ch, 
                                LTRIM(RTRIM(enc_reason_ch)) as enc_reason_ch
                            FROM 
                                Army.dbo.v_encourage 
                            WHERE 
                                member_id = @memberId
                            ORDER BY
                                doc_date DESC";
            */

            // 創建一個SqlParameter的實例來防止SQL注入
            SqlParameter[] encouragePara = new SqlParameter[]
            {
                new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId }
            };

            try
            {                
                DataTable encourageTB = _dbHelper.ArmyWebExecuteQuery(encourageSql, encouragePara);

                if (encourageTB != null && encourageTB.Rows.Count > 0)
                {                    
                    foreach(DataRow row in encourageTB.Rows)
                    {
                        /*
                        EncourageRes encourageRes = new EncourageRes() 
                        {
                            UnitCode = row["unit_code"].ToString(),

                            EncUnitCode  = _codeToName.unitName(row["enc_unit_code"].ToString(), false),

                            RankCode = _codeToName.rankName(row["rank_code"].ToString(), false),

                            DocDate  = _codeToName.dateTimeTran(row["doc_date"].ToString(), "yyy/MM/dd", true),

                            DocNo = row["doc_no"].ToString(),

                            EncReasonCode = _codeToName.metalName(row["enc_reason_code"].ToString(), false),

                            EncGroup = _codeToName.encoGroupName(row["enc_group"].ToString(), false),

                            DocItem = row["doc_item"].ToString(),

                            EncPointIdent = row["enc_point_ident"].ToString(),

                            EncCancelDate  = _codeToName.dateTimeTran(row["enc_cancel_date"].ToString(), "yyy/MM/dd", true),

                            EncCancelDoc = row["enc_cancel_doc"].ToString(),

                            UnitName = _codeToName.unitName(row["unit_code"].ToString(), false),

                            DocCh = row["doc_ch"].ToString(),

                            EncReasonCh = row["enc_reason_ch"].ToString(),
                        };
                        */
                        
                        EncourageRes encourageRes = new EncourageRes()
                        {
                            UnitCode = row["enc_unit_code"].ToString(),

                            EncUnitCode = row["enc_unit"].ToString(),

                            RankCode = row["rank"].ToString(),

                            DocDate = row["doc_date"].ToString(),

                            DocNo = row["doc_no"].ToString(),

                            EncReasonCode = row["enc_metal_desc"].ToString(),

                            EncGroup = row["enc_group"].ToString(),

                            DocItem = row["doc_item"].ToString(),

                            EncPointIdent = row["enc_point_ident"].ToString(),

                            EncCancelDate = row["enc_cancel_date"].ToString(),

                            EncCancelDoc = row["enc_cancel_doc"].ToString(),

                            UnitName = row["unit"].ToString(),

                            DocCh = row["enc_doc_ch"].ToString(),

                            EncReasonCh = row["enc_reason_ch"].ToString(),
                        };
                        
                        encourageList.Add(encourageRes);
                    }                    
                    return Ok(new { Result = "Success", encourageList });
                }
                else
                {
                    return Ok(new { Result = "Fail", encourageList });
                }
            }
            catch (Exception ex)
            {
                // 如果出現異常，返回錯誤信息
                WriteLog.Log(String.Format("【memberEncourage Fail】" + ex.Message.ToString()));
                return BadRequest("【memberEncourage Fail】" + ex.Message);
            }
        }

        // 獎懲統計
        [HttpGet]
        [ActionName("encourageStatistics")]
        public IHttpActionResult encourageStatistics(string memberId)
        {
            List<EncourageRes> encourageList = new List<EncourageRes>();
            string encourageSql = @"SELECT                           
                                    CASE 
                                        WHEN ve.enc_reason_code='1' THEN '勳章'
                                        WHEN ve.enc_reason_code IN ('2','3') THEN '獎章'
                                        WHEN ve.enc_reason_code ='2' AND ve.enc_metal_code=91 THEN '褒狀'
                                        WHEN ve.enc_reason_code ='5' AND LEFT(ve.enc_metal_code,1) IN ('8','9') THEN '獎狀'
                                        WHEN ve.enc_reason_code='6' THEN '大功'
                                        WHEN ve.enc_reason_code='7' THEN '記功'
                                        WHEN ve.enc_reason_code='8' THEN '嘉獎'
                                        WHEN ve.enc_reason_code='9' THEN '獎金'
                                        WHEN ve.enc_reason_code='E' THEN '大過'
                                        WHEN ve.enc_reason_code='F' THEN '記過'
                                        WHEN ve.enc_reason_code='G' THEN '申誡'
                                    ELSE '其他' END AS encType,
                                    COUNT(*) AS count_per_code
                                FROM
                                    Army.dbo.v_encourage AS ve
                                WHERE
                                    member_id = @memberId
                                GROUP BY
                                    CASE 
                                        WHEN ve.enc_reason_code='1' THEN '勳章'
                                        WHEN ve.enc_reason_code IN ('2','3') THEN '獎章'
                                        WHEN ve.enc_reason_code ='2' AND ve.enc_metal_code=91 THEN '褒狀'
                                        WHEN ve.enc_reason_code ='5' AND LEFT(ve.enc_metal_code,1) IN ('8','9') THEN '獎狀'
                                        WHEN ve.enc_reason_code='6' THEN '大功'
                                        WHEN ve.enc_reason_code='7' THEN '記功'
                                        WHEN ve.enc_reason_code='8' THEN '嘉獎'
                                        WHEN ve.enc_reason_code='9' THEN '獎金'
                                        WHEN ve.enc_reason_code='E' THEN '大過'
                                        WHEN ve.enc_reason_code='F' THEN '記過'
                                        WHEN ve.enc_reason_code='G' THEN '申誡'
                                    ELSE '其他'
                                    END;";

            // 創建一個SqlParameter的實例來防止SQL注入
            SqlParameter[] encouragePara = new SqlParameter[]
            {
                new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId }
            };

            try
            {
                DataTable encourageTB = _dbHelper.ArmyWebExecuteQuery(encourageSql, encouragePara);

                if (encourageTB != null && encourageTB.Rows.Count > 0)
                {                   
                    return Ok(new { Result = "Success", encourageTB });
                }
                else
                {
                    return Ok(new { Result = "Fail", encourageTB });
                }
            }
            catch (Exception ex)
            {
                // 如果出現異常，返回錯誤信息
                WriteLog.Log(String.Format("【encourageStatistics Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【encourageStatistics Fail】" + ex.Message);
            }
        }

        // 獎懲退伍資料
        [HttpGet]
        [ActionName("memberRetireEncourage")]
        public IHttpActionResult memberRetireEncourage(string memberId)
        {
            List<EncourageRes> encourageList = new List<EncourageRes>();
            
            string encourageSql = @"DECLARE @true bit = 1, @false bit = 0
                                    SELECT
	                                    @false 'active',
	                                    vmu2.unit_code 'enc_unit_code',
	                                    rtrim(vmu2.unit_title) 'enc_unit',
	                                    rtrim(r.rank_title) 'rank',
	                                    dbo.DateToString(enr.doc_date, 2) 'doc_date',
	                                    enr.doc_no,
                                    case 
	                                    when enr.enc_reason_code ='A' then '撤職'	                                    
	                                    when enr.enc_reason_code = 'H' then '罰薪'+rtrim(enr.enc_metal_level)+rtrim(enr.enc_star_level)+'%'	                                    
	                                    when enr.enc_reason_code in ('J') then '檢束'+rtrim(enr.enc_metal_level)+rtrim(enr.enc_star_level)+'日'
	                                    when enr.enc_reason_code in ('K') then rtrim(em1.enc_metal_desc)+rtrim(enr.enc_metal_level)+rtrim(enr.enc_star_level)+'月'
	                                    when enr.enc_reason_code in ('L') then '禁閉'+rtrim(enr.enc_metal_level)+rtrim(enr.enc_star_level)+'日'
	                                    when enr.enc_reason_code in ('M') then '悔過'+rtrim(enr.enc_metal_level)+rtrim(enr.enc_star_level)+'日'	                                    
	                                    when enr.enc_reason_code in ('P') then '罰勤'+rtrim(enr.enc_metal_level)+rtrim(enr.enc_star_level)+'日'
	                                    when enr.enc_reason_code in ('Q') then '禁足'+rtrim(enr.enc_metal_level)+rtrim(enr.enc_star_level)+'日'	                                    
	                                    when enr.enc_reason_code in ('S') and enr.enc_metal_code in ('13') then '公務人員懲戒法降級'+rtrim(enr.enc_metal_level)+rtrim(enr.enc_star_level)+'年'
	                                    when enr.enc_reason_code in ('T') then '罰站'+rtrim(enr.enc_metal_level)+rtrim(enr.enc_star_level)+'分鐘'	                                   
	                                    when enr.enc_reason_code ='X' then '事蹟存記'
	                                    when enr.enc_reason_code ='Y' then '不良事蹟存記'
	                                    when enr.enc_reason_code ='Z' then '言詞申誡'	
	                                    when enr.enc_reason_code='9' AND rtrim(enr.enc_star_level)='A'
	                                        THEN '獎金'+rtrim(enr.enc_metal_code)+rtrim(enr.enc_metal_level)+'0000元'
	                                    when enr.enc_reason_code='9' AND rtrim(enr.enc_star_level)='B'
	                                        THEN '獎金'+rtrim(enr.enc_metal_code)+rtrim(enr.enc_metal_level)+'000元'
	                                    when enr.enc_reason_code='9'
	                                        THEN '獎金'+rtrim(enr.enc_metal_code)+rtrim(enr.enc_metal_level)+rtrim(enr.enc_star_level)+'元'
	                                    ELSE em1.enc_metal_desc
                                    END 'enc_metal_desc',
	                                    rtrim(eg.enc_group_desc) 'enc_group',
	                                    enr.doc_item,
	                                    enr.enc_point_ident,
	                                    replace(dbo.DateToString(enr.enc_cancel_date, 2), '-11/01/01', '') 'enc_cancel_date',
	                                    enr.enc_cancel_doc,
	                                    rtrim(vmu1.unit_title) 'unit',
	                                    enr.doc_ch 'enc_doc_ch',
	                                    enr.enc_reason_ch 'enc_reason_ch'
                                    FROM [Army].[dbo].[v_encourage_retire] enr
	                                    LEFT JOIN [Army].[dbo].[enco_metal] as em1 on em1.enc_reason_code+em1.enc_metal_code+em1.enc_metal_level+em1.enc_star_level = enr.enc_reason_code+enr.enc_metal_code+enr.enc_metal_level+enr.enc_star_level
	                                    LEFT JOIN [Army].[dbo].[v_mu_unit] as vmu1 on vmu1.unit_code = enr.unit_code
	                                    LEFT JOIN [Army].[dbo].[v_mu_unit] as vmu2 on vmu2.unit_code = enr.enc_unit_code
	                                    LEFT JOIN [Army].[dbo].[rank] as r on r.rank_code = enr.rank_code
	                                    LEFT JOIN [Army].[dbo].[enco_group] as eg on eg.enc_group_code = enr.enc_group
	                                    LEFT JOIN [OrderSystem].[dbo].[Unit] as ou on ou.UnitCode = vmu1.unit_code
                                    WHERE member_id = @MemberID
                                    ORDER BY doc_date DESC";
            
            /*
            string encourageSql = @"
                            SELECT                                 
                                LTRIM(RTRIM(unit_code)) as unit_code, LTRIM(RTRIM(enc_unit_code)) as enc_unit_code, 
                                LTRIM(RTRIM(rank_code)) as rank_code, doc_date, 
                                LTRIM(RTRIM(doc_no)) as doc_no, LTRIM(RTRIM(enc_reason_code)) as enc_reason_code, 
                                LTRIM(RTRIM(enc_group)) as enc_group, LTRIM(RTRIM(doc_item)) as doc_item, 
                                LTRIM(RTRIM(enc_point_ident)) as enc_point_ident, LTRIM(RTRIM(enc_cancel_date)) as enc_cancel_date, 
                                LTRIM(RTRIM(enc_cancel_doc)) as enc_cancel_doc, LTRIM(RTRIM(doc_ch)) as doc_ch, 
                                LTRIM(RTRIM(enc_reason_ch)) as enc_reason_ch
                            FROM 
                                Army.dbo.v_encourage_retire
                            WHERE 
                                member_id = @memberId
                            ORDER BY
                                doc_date DESC";
            */

            // 創建一個SqlParameter的實例來防止SQL注入
            SqlParameter[] encouragePara = new SqlParameter[]
            {
                new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId }
            };

            try
            {
                DataTable encourageTB = _dbHelper.ArmyWebExecuteQuery(encourageSql, encouragePara);

                if (encourageTB != null && encourageTB.Rows.Count > 0)
                {
                    foreach (DataRow row in encourageTB.Rows)
                    {
                        /*
                        EncourageRes encourageRes = new EncourageRes()
                        {
                            UnitCode = row["unit_code"].ToString(),

                            EncUnitCode = _codeToName.unitName(row["enc_unit_code"].ToString(), false),

                            RankCode = _codeToName.rankName(row["rank_code"].ToString(), false),

                            DocDate = _codeToName.dateTimeTran(row["doc_date"].ToString(), "yyy/MM/dd", true),

                            DocNo = row["doc_no"].ToString(),

                            EncReasonCode = _codeToName.metalName(row["enc_reason_code"].ToString(), false),

                            EncGroup = _codeToName.encoGroupName(row["enc_group"].ToString(), false),

                            DocItem = row["doc_item"].ToString(),

                            EncPointIdent = row["enc_point_ident"].ToString(),

                            EncCancelDate = _codeToName.dateTimeTran(row["enc_cancel_date"].ToString(), "yyy/MM/dd", true),

                            EncCancelDoc = row["enc_cancel_doc"].ToString(),

                            UnitName = _codeToName.unitName(row["unit_code"].ToString(), false),

                            DocCh = row["doc_ch"].ToString(),

                            EncReasonCh = row["enc_reason_ch"].ToString(),
                        };
                        */

                        
                        EncourageRes encourageRes = new EncourageRes()
                        {
                            UnitCode = row["enc_unit_code"].ToString(),

                            EncUnitCode = row["enc_unit"].ToString(),

                            RankCode = row["rank"].ToString(),

                            DocDate = row["doc_date"].ToString(),

                            DocNo = row["doc_no"].ToString(),

                            EncReasonCode = row["enc_metal_desc"].ToString(),

                            EncGroup = row["enc_group"].ToString(),

                            DocItem = row["doc_item"].ToString(),

                            EncPointIdent = row["enc_point_ident"].ToString(),

                            EncCancelDate = row["enc_cancel_date"].ToString(),

                            EncCancelDoc = row["enc_cancel_doc"].ToString(),

                            UnitName = row["unit"].ToString(),

                            DocCh = row["enc_doc_ch"].ToString(),

                            EncReasonCh = row["enc_reason_ch"].ToString(),
                        };
                        
                        encourageList.Add(encourageRes);
                    }
                    return Ok(new { Result = "Success", encourageList });
                }
                else
                {
                    return Ok(new { Result = "Fail", encourageList });
                }
            }
            catch (Exception ex)
            {
                // 如果出現異常，返回錯誤信息
                WriteLog.Log(String.Format("【memberRetireEncourage Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【memberRetireEncourage Fail】" + ex.Message);
            }
        }

        // 退伍人員獎懲統計
        [HttpGet]
        [ActionName("RetireEncourageStatistics")]
        public IHttpActionResult RetireEncourageStatistics(string memberId)
        {
            List<EncourageRes> encourageList = new List<EncourageRes>();
            string encourageSql = @"SELECT                           
                                    CASE 
                                        WHEN ve.enc_reason_code='1' THEN '勳章'
                                        WHEN ve.enc_reason_code IN ('2','3') THEN '獎章'
                                        WHEN ve.enc_reason_code ='2' AND ve.enc_metal_code=91 THEN '褒狀'
                                        WHEN ve.enc_reason_code ='5' AND LEFT(ve.enc_metal_code,1) IN ('8','9') THEN '獎狀'
                                        WHEN ve.enc_reason_code='6' THEN '大功'
                                        WHEN ve.enc_reason_code='7' THEN '記功'
                                        WHEN ve.enc_reason_code='8' THEN '嘉獎'
                                        WHEN ve.enc_reason_code='9' THEN '獎金'
                                        WHEN ve.enc_reason_code='E' THEN '大過'
                                        WHEN ve.enc_reason_code='F' THEN '記過'
                                        WHEN ve.enc_reason_code='G' THEN '申誡'
                                    ELSE '其他' END AS encType,
                                    COUNT(*) AS count_per_code
                                FROM
                                    Army.dbo.v_encourage_retire AS ve
                                WHERE
                                    member_id = @memberId
                                GROUP BY
                                    CASE 
                                        WHEN ve.enc_reason_code='1' THEN '勳章'
                                        WHEN ve.enc_reason_code IN ('2','3') THEN '獎章'
                                        WHEN ve.enc_reason_code ='2' AND ve.enc_metal_code=91 THEN '褒狀'
                                        WHEN ve.enc_reason_code ='5' AND LEFT(ve.enc_metal_code,1) IN ('8','9') THEN '獎狀'
                                        WHEN ve.enc_reason_code='6' THEN '大功'
                                        WHEN ve.enc_reason_code='7' THEN '記功'
                                        WHEN ve.enc_reason_code='8' THEN '嘉獎'
                                        WHEN ve.enc_reason_code='9' THEN '獎金'
                                        WHEN ve.enc_reason_code='E' THEN '大過'
                                        WHEN ve.enc_reason_code='F' THEN '記過'
                                        WHEN ve.enc_reason_code='G' THEN '申誡'
                                    ELSE '其他'
                                    END;";

            // 創建一個SqlParameter的實例來防止SQL注入
            SqlParameter[] encouragePara = new SqlParameter[]
            {
                new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId }
            };

            try
            {
                DataTable encourageTB = _dbHelper.ArmyWebExecuteQuery(encourageSql, encouragePara);

                if (encourageTB != null && encourageTB.Rows.Count > 0)
                {
                    return Ok(new { Result = "Success", encourageTB });
                }
                else
                {
                    return Ok(new { Result = "Fail", encourageTB });
                }
            }
            catch (Exception ex)
            {
                // 如果出現異常，返回錯誤信息
                WriteLog.Log(String.Format("【RetireEncourageStatistics Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【RetireEncourageStatistics Fail】" + ex.Message);
            }
        }

        // 專長
        [HttpGet]
        [ActionName("memberSkill")]
        public IHttpActionResult memberSkill(string memberId)
        {
            List<SkillRes> skillList = new List<SkillRes>();
            string skillSql = @"
                            SELECT 
                                LTRIM(RTRIM(es_skill_code)) as es_skill_code, LTRIM(RTRIM(command_skill_code)) as command_skill_code, 
                                LTRIM(RTRIM(com_rank_code)) as com_rank_code, LTRIM(RTRIM(skill1_code)) as skill1_code, 
                                LTRIM(RTRIM(skill1_rank_code)) as skill1_rank_code, LTRIM(RTRIM(skill2_code)) as skill2_code, 
                                LTRIM(RTRIM(skill2_rank_code)) as skill2_rank_code, LTRIM(RTRIM(skill3_code)) as skill3_code, 
                                LTRIM(RTRIM(skill3_rank_code)) as skill3_rank_code, LTRIM(RTRIM(unit_code)) as unit_code, 
                                LTRIM(RTRIM(doc_ch)) as doc_ch, LTRIM(RTRIM(doc_date)) as doc_date, 
                                effect_date, LTRIM(RTRIM(trans_code)) as trans_code, 
                                LTRIM(RTRIM(trans_date)) as trans_date, LTRIM(RTRIM(get_type)) as get_type
                            FROM 
                                Army.dbo.v_skill_profession
                            WHERE 
                                member_id = @memberId
                            ORDER BY
                                effect_date DESC";

            // 創建一個SqlParameter的實例來防止SQL注入
            SqlParameter[] skillPara = new SqlParameter[]
            {
                new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId }
            };

            try
            {
                // 調用之前定義的方法執行查詢，返回一個DataTable
                DataTable skillTB = _dbHelper.ArmyWebExecuteQuery(skillSql, skillPara);

                if (skillTB != null && skillTB.Rows.Count > 0)
                {
                    skillTB.Columns.Add("rank_code");
                    
                    foreach(DataRow row in skillTB.Rows)
                    {
                        SkillRes skillRes = new SkillRes() 
                        {
                            EsSkillCode = _codeToName.skillName(row["es_skill_code"].ToString()),

                            RankCode = _codeToName.rankName(row["skill3_rank_code"].ToString()),

                            CommandSkillCode = _codeToName.skillName(row["command_skill_code"].ToString()),

                            ComRankCode = _codeToName.rankName(row["com_rank_code"].ToString()),

                            Skill1Code = _codeToName.skillName(row["skill1_code"].ToString()),

                            Skill1RankCode = _codeToName.rankName(row["skill1_rank_code"].ToString()),

                            Skill2Code = _codeToName.skillName(row["skill2_code"].ToString()),

                            Skill2RankCode = _codeToName.rankName(row["skill2_rank_code"].ToString()),

                            Skill3Code = _codeToName.skillName(row["skill3_code"].ToString()),

                            Skill3RankCode = _codeToName.rankName(row["skill3_rank_code"].ToString()),

                            UnitCode = _codeToName.unitName(row["unit_code"].ToString()),

                            DocCh = row["doc_ch"].ToString(),

                            DocDate = _codeToName.dateTimeTran(row["doc_date"].ToString(), "yyy年MM月dd日", true),

                            EffectDate = _codeToName.dateTimeTran(row["effect_date"].ToString(), "yyy年MM月dd日", true),

                            TransCode = _codeToName.transName(row["trans_code"].ToString()),

                            TransDate = _codeToName.dateTimeTran(row["trans_date"].ToString(), "yyy年MM月dd日", true),

                            GetTypeA = _codeToName.skillTypeName(row["get_type"].ToString())
                        };

                        skillList.Add(skillRes);
                    }
                    return Ok(new { Result = "Success", skillList });
                }
                else
                {
                    return Ok(new { Result = "Fail", skillList });
                }
            }
            catch (Exception ex)
            {
                // 如果出現異常，返回錯誤信息
                WriteLog.Log(String.Format("【memberSkill Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【memberSkill Fail】" + ex.Message);
            }
        }

        // 俸級晉支
        [HttpGet]
        [ActionName("RiseRankSupply")]
        public IHttpActionResult RiseRankSupply(string memberId)
        {
            List<SupplyRes> supList = new List<SupplyRes>();
            string supplySql = @"
                            SELECT 
                                LTRIM(RTRIM(old_rank_code)) as old_rank_code, LTRIM(RTRIM(old_supply_rank)) as old_supply_rank, 
                                LTRIM(RTRIM(new_rank_code)) as new_rank_code, LTRIM(RTRIM(new_supply_rank)) as new_supply_rank, 
                                LTRIM(RTRIM(approve_unit)) as approve_unit, LTRIM(RTRIM(effect_date)) as effect_date, 
                                doc_date, LTRIM(RTRIM(doc_no)) as doc_no, 
                                LTRIM(RTRIM(doc_ch)) as doc_ch
                            FROM 
                                Army.dbo.v_rise_rank_supply
                            WHERE 
                                member_id = @memberId
                            ORDER BY
                                doc_date DESC";

            // 創建一個SqlParameter的實例來防止SQL注入
            SqlParameter[] supplyPara = new SqlParameter[]
            {
                new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId }
            };

            try
            {
                DataTable supplyTB = _dbHelper.ArmyWebExecuteQuery(supplySql, supplyPara);

                if (supplyTB != null && supplyTB.Rows.Count > 0)
                {                            
                    foreach (DataRow row in supplyTB.Rows) 
                    {
                        SupplyRes supRes = new SupplyRes() 
                        {
                            OldRankCode = row["old_rank_code"].ToString(),
                            OldSupplyRank = row["old_supply_rank"].ToString(),
                            NewRankCode = row["new_rank_code"].ToString(),
                            NewSupplyRank = row["new_supply_rank"].ToString(),
                            ApproveUnit = row["approve_unit"].ToString().Trim(),
                            EffectDate = _codeToName.dateTimeTran(row["effect_date"].ToString(), "yyy年MM月dd日", true),
                            DocDate = _codeToName.dateTimeTran(row["doc_date"].ToString(), "yyy年MM月dd日", true),
                            DocNo = row["doc_no"].ToString(),
                            DocCh = row["doc_ch"].ToString()
                        };
                        supList.Add(supRes);
                    }
                    return Ok(new { Result = "Success", supList });
                }
                else
                {
                    return Ok(new { Result = "Fail", supList });
                }
            }
            catch (Exception ex)
            {
                // 如果出現異常，返回錯誤信息
                WriteLog.Log(String.Format("【RiseRankSupply Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【RiseRankSupply Fail】" + ex.Message);
            }
        }

        //役期管制
        [HttpGet]
        [ActionName("ControlRetiredate")]
        public IHttpActionResult ControlRetiredate(string memberId)
        {
            List<RetiredateRes> retList = new List<RetiredateRes>();
            string retiredateSql = @"
                                    SELECT 
                                        LTRIM(RTRIM(control_code)) as control_code, LTRIM(RTRIM(subtract_day)) as subtract_day, 
                                        LTRIM(RTRIM(approve_unit)) as approve_unit, apv_start_date, 
                                        LTRIM(RTRIM(doc_ch)) as doc_ch, LTRIM(RTRIM(doc_no)) as doc_no, 
                                        LTRIM(RTRIM(doc_date)) as doc_date, LTRIM(RTRIM(apv_enc_date)) as apv_enc_date, 
                                        LTRIM(RTRIM(control_date)) as control_date
                                    FROM 
                                        Army.dbo.v_control_retiredate
                                    WHERE 
                                        member_id = @memberId
                                    ORDER BY
                                        apv_start_date DESC";

            // 創建一個SqlParameter的實例來防止SQL注入
            SqlParameter[] retiredatePara = new SqlParameter[]
            {
                new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId }
            };

            try
            {
                
                DataTable retiredateTB = _dbHelper.ArmyWebExecuteQuery(retiredateSql, retiredatePara);

                if (retiredateTB != null && retiredateTB.Rows.Count > 0)
                {
                    
                    foreach (DataRow row in retiredateTB.Rows)
                    {
                        RetiredateRes retRes = new RetiredateRes() 
                        {
                            ControlCode = _codeToName.controlName(row["control_code"].ToString()),
                            SubtractDay = row["subtract_day"].ToString(),
                            ApproveUnit = _codeToName.unitName(row["approve_unit"].ToString()),
                            ApvStartDate =  _codeToName.dateTimeTran(row["apv_start_date"].ToString(), "yyy年MM月dd日", true),
                            DocCh = row["doc_ch"].ToString(),
                            DocNo = row["doc_no"].ToString(),
                            DocDate = _codeToName.dateTimeTran(row["doc_date"].ToString(), "yyy年MM月dd日", true),
                            ApvEncDate = _codeToName.dateTimeTran(row["apv_enc_date"].ToString(), "yyy年MM月dd日", true),
                            ControlDate = _codeToName.dateTimeTran(row["control_date"].ToString(), "yyy年MM月dd日", true)
                        };

                        retList.Add(retRes);
                    }
                    return Ok(new { Result = "Success", retList });
                }
                else
                {
                    return Ok(new { Result = "Fail", retList });
                }
            }
            catch (Exception ex)
            {
                // 如果出現異常，返回錯誤信息
                WriteLog.Log(String.Format("【ControlRetiredate Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【ControlRetiredate Fail】" + ex.Message);
            }
        }

        //測驗
        [HttpGet]
        [ActionName("memberExam")]
        public IHttpActionResult memberExam(string memberId)
        {
            List<ExamRes> examList = new List<ExamRes>();
            string examSql = @"
                            SELECT 
                                LTRIM(RTRIM(exam_code)) as exam_code, LTRIM(RTRIM(exam_date)) as exam_date, 
                                LTRIM(RTRIM(score)) as score, LTRIM(RTRIM(exam_level)) as exam_level, 
                                LTRIM(RTRIM(approve_unit)) as approve_unit, doc_date, 
                                LTRIM(RTRIM(doc_no)) as doc_no, LTRIM(RTRIM(doc_ch)) as doc_ch
                            FROM 
                                Army.dbo.v_exam
                            WHERE 
                                member_id = @memberId
                            ORDER BY
                                doc_date DESC";

            // 創建一個SqlParameter的實例來防止SQL注入
            SqlParameter[] examPara = new SqlParameter[]
            {
                new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId }
            };

            try
            {                
                DataTable examTB = _dbHelper.ArmyWebExecuteQuery(examSql, examPara);

                if (examTB != null && examTB.Rows.Count > 0)
                {                    
                    foreach (DataRow row in examTB.Rows) 
                    {
                        ExamRes examRes = new ExamRes() 
                        {
                            ExamCode = row["exam_code"].ToString(),
                            ExamDate = _codeToName.dateTimeTran(row["exam_date"].ToString(), "yyy年MM月dd日", true),
                            Score = row["Score"].ToString(),
                            ExamLevel = row["exam_level"].ToString(),
                            ApproveUnit = row["approve_unit"].ToString(),
                            DocDate = _codeToName.dateTimeTran(row["doc_date"].ToString(), "yyy年MM月dd日", true),
                            DocNo = row["doc_no"].ToString(),
                            DocCh = row["doc_ch"].ToString(),
                        };
                        examList.Add(examRes);
                    }
                    return Ok(new { Result = "Success", examList });
                }
                else
                {
                    return Ok(new { Result = "Fail", examList });
                }
            }
            catch (Exception ex)
            {
                // 如果出現異常，返回錯誤信息
                WriteLog.Log(String.Format("【memberExam Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【memberExam Fail】" + ex.Message);
            }
        }

        // 任官令
        [HttpGet]
        [ActionName("memberAppointment")]
        public IHttpActionResult memberAppointment(string memberId)
        {
            List<AppointmentRes> appList = new List<AppointmentRes>(); 
            string appointmentSql = @"
                            SELECT 
                                LTRIM(RTRIM(class_code)) as class_code, LTRIM(RTRIM(appointment_date)) as appointment_date, 
                                LTRIM(RTRIM(number)) as number, LTRIM(RTRIM(new_service_code)) as new_service_code, 
                                LTRIM(RTRIM(new_rank_code)) as new_rank_code, LTRIM(RTRIM(new_group_code)) as new_group_code,
                                LTRIM(RTRIM(old_service_code)) as old_service_code, LTRIM(RTRIM(old_rank_code)) as old_rank_code, 
                                LTRIM(RTRIM(old_group_code)) as old_group_code, LTRIM(RTRIM(unit_code)) as unit_code, 
                                effect_date
                            FROM 
                                Army.dbo.v_appointment
                            WHERE 
                                member_id = @memberId
                            ORDER BY
                                effect_date DESC";

            // 創建一個SqlParameter的實例來防止SQL注入
            SqlParameter[] appointmentPara = new SqlParameter[]
            {
                new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId }
            };

            try
            {
                // 調用之前定義的方法執行查詢，返回一個DataTable
                DataTable appointmentTB = _dbHelper.ArmyWebExecuteQuery(appointmentSql, appointmentPara);

                if (appointmentTB != null && appointmentTB.Rows.Count > 0)
                {
                    
                    foreach (DataRow row in appointmentTB.Rows)
                    {
                        
                        AppointmentRes appointmentRes = new AppointmentRes() 
                        {
                            ClassCode = _codeToName.appointmentName(row["class_code"].ToString()),
                            AppointmentDate = _codeToName.dateTimeTran(row["appointment_date"].ToString(), "yyy年MM月dd日", true),
                            NumberA = row["number"].ToString(),
                            NewServiceCode = _codeToName.serviceName(row["new_service_code"].ToString()),
                            NewRankCode = _codeToName.rankName(row["new_rank_code"].ToString()),
                            NewGroupCode = _codeToName.groupName(row["new_group_code"].ToString()),
                            OldServiceCode = _codeToName.serviceName(row["old_service_code"].ToString()),
                            OldRankCode = _codeToName.rankName(row["old_rank_code"].ToString()),
                            OldGroupCode = _codeToName.groupName(row["old_group_code"].ToString()),
                            UnitCode = row["unit_code"].ToString(),
                            EffectDate = _codeToName.dateTimeTran(row["effect_date"].ToString(), "yyy年MM月dd日", true)
                        };
                        appList.Add(appointmentRes);
                    }
                    return Ok(new { Result = "Success", appList });
                }
                else
                {
                    return Ok(new { Result = "Fail", appList });
                }
            }
            catch (Exception ex)
            {
                // 如果出現異常，返回錯誤信息
                WriteLog.Log(String.Format("【memberAppointment Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【memberAppointment Fail】" + ex.Message);
            }
        }

        // 進修
        [HttpGet]
        [ActionName("educationControl")]
        public IHttpActionResult educationControl(string memberId)
        {
            List<EduControlRes> eduList = new List<EduControlRes>();
            string eduControlSql = @"
                            SELECT 
                                LTRIM(RTRIM(control_code)) as control_code, LTRIM(RTRIM(country_code)) as country_code, 
                                LTRIM(RTRIM(school_code)) as school_code, LTRIM(RTRIM(discipline_code)) as discipline_code, 
                                LTRIM(RTRIM(class_code)) as class_code, LTRIM(RTRIM(educ_code)) as educ_code, 
                                study_date, LTRIM(RTRIM(graduate_date)) as graduate_date, 
                                LTRIM(RTRIM(period_no)) as period_no, LTRIM(RTRIM(year_class)) as year_class, 
                                LTRIM(RTRIM(control_status)) as control_status, LTRIM(RTRIM(doc_date)) as doc_date, 
                                LTRIM(RTRIM(doc_no)) as doc_no, LTRIM(RTRIM(doc_ch)) as doc_ch, 
                                LTRIM(RTRIM(leave_date)) as leave_date, LTRIM(RTRIM(approve_unit)) as approve_unit  
                            FROM 
                                Army.dbo.v_education_control
                            WHERE 
                                member_id = @memberId
                            ORDER BY
                                study_date DESC";

            // 創建一個SqlParameter的實例來防止SQL注入
            SqlParameter[] eduControlPara = new SqlParameter[]
            {
                new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId }
            };

            try
            {                
                DataTable eduControlTB = _dbHelper.ArmyWebExecuteQuery(eduControlSql, eduControlPara);

                if (eduControlTB != null && eduControlTB.Rows.Count > 0)
                {
                    
                    foreach (DataRow row in eduControlTB.Rows)
                    {
                        EduControlRes eduControlRes = new EduControlRes()
                        {
                            ControlCode = _codeToName.eduControlName(row["control_code"].ToString()),
                            CountryCode = _codeToName.countryName(row["country_code"].ToString()),
                            SchoolCode = _codeToName.schoolName(row["school_code"].ToString()),
                            DisciplineCode = _codeToName.disciplineName(row["discipline_code"].ToString()),
                            ClassCode = _codeToName.className(row["class_code"].ToString()),
                            EducCode = _codeToName.educName(row["educ_code"].ToString()),
                            StudyDate = _codeToName.dateTimeTran(row["study_date"].ToString(), "yyy年MM月dd日", true),
                            GraduateDate = _codeToName.dateTimeTran(row["graduate_date"].ToString(), "yyy年MM月dd日", true),
                            PeriodNo = row["period_no"].ToString(),
                            YearClass = row["year_class"].ToString(),
                            ControlStatus = row["control_status"].ToString(),
                            DocDate = _codeToName.dateTimeTran(row["doc_date"].ToString(), "yyy年MM月dd日", true),
                            DocNo = row["doc_no"].ToString(),
                            DocCh = row["doc_ch"].ToString(),
                            LeaveDate = _codeToName.dateTimeTran(row["leave_date"].ToString(), "yyy年MM月dd日", true),
                            ApproveUnit = _codeToName.unitName(row["approve_unit"].ToString())
                        };
                        eduList.Add(eduControlRes);
                    }
                    return Ok(new { Result = "Success", eduList });
                }
                else
                {
                    return Ok(new { Result = "Fail", eduList });
                }
            }
            catch (Exception ex)
            {
                // 如果出現異常，返回錯誤信息
                WriteLog.Log(String.Format("【educationControl Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【educationControl Fail】" + ex.Message);
            }
        }

        // 考試證照
        [HttpGet]
        [ActionName("memberCertificate")]
        public IHttpActionResult memberCertificate(string memberId)
        {
            List<CertificateRes> certList = new List<CertificateRes>();
            string certificateSql = @"
                            SELECT 
                                LTRIM(RTRIM(certificate_sort)) as certificate_sort, LTRIM(RTRIM(certificate_job_code)) as certificate_job_code, 
                                LTRIM(RTRIM(certificate_grade_code)) as certificate_grade_code, get_date, 
                                LTRIM(RTRIM(certificate_no)) as certificate_no, LTRIM(RTRIM(approve_unit)) as approve_unit, 
                                LTRIM(RTRIM(doc_date)) as doc_date, LTRIM(RTRIM(doc_no)) as doc_no, 
                                LTRIM(RTRIM(doc_ch)) as doc_ch
                            FROM 
                                Army.dbo.v_certificate
                            WHERE 
                                member_id = @memberId
                            ORDER BY
                                get_date DESC";

            // 創建一個SqlParameter的實例來防止SQL注入
            SqlParameter[] certificatePara = new SqlParameter[]
            {
                new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId }
            };

            try
            {
                
                DataTable certificateTB = _dbHelper.ArmyWebExecuteQuery(certificateSql, certificatePara);

                if (certificateTB != null && certificateTB.Rows.Count > 0)
                {
                    foreach (DataRow row in certificateTB.Rows)
                    {
                        CertificateRes certificateRes = new CertificateRes() 
                        {
                            CertificateSort = _codeToName.certSortName(row["certificate_sort"].ToString()),
                            CertificateJobCode = _codeToName.certJobName(row["certificate_job_code"].ToString()),
                            CertificateGradeCode = _codeToName.certGradeName(row["certificate_grade_code"].ToString()),
                            GetDate = _codeToName.dateTimeTran(row["get_date"].ToString(), "yyy年MM月dd日", true),
                            CertificateNo = row["certificate_no"].ToString(),
                            ApproveUnit = _codeToName.unitName(row["approve_unit"].ToString()),
                            DocDate = _codeToName.dateTimeTran(row["doc_date"].ToString(), "yyy年MM月dd日", true),
                            DocNo = row["doc_no"].ToString(),
                            DocCh = row["doc_ch"].ToString()
                        };
                        certList.Add(certificateRes);
                    }
                    return Ok(new { Result = "Success", certList });
                }
                else
                {
                    return Ok(new { Result = "Fail", certList });
                }
            }
            catch (Exception ex)
            {
                // 如果出現異常，返回錯誤信息
                WriteLog.Log(String.Format("【memberCertificate Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【memberCertificate Fail】" + ex.Message);
            }
        }

        //著作
        [HttpGet]
        [ActionName("memberWritings")]
        public IHttpActionResult memberWritings(string memberId)
        {
            List<WritingRes> writeList = new List<WritingRes>();
            string writingSql = @"
                            SELECT 
                                LTRIM(RTRIM(title_heading)) as title_heading, LTRIM(RTRIM(publisher)) as publisher, 
                                LTRIM(RTRIM(job_code)) as job_code, LTRIM(RTRIM(writings_code)) as writings_code, 
                                press_date, LTRIM(RTRIM(approve_unit)) as approve_unit, 
                                LTRIM(RTRIM(doc_date)) as doc_date, LTRIM(RTRIM(doc_no)) as doc_no, 
                                LTRIM(RTRIM(doc_ch)) as doc_ch
                            FROM 
                                Army.dbo.v_writings
                            WHERE 
                                member_id = @memberId
                            ORDER BY
                                press_date DESC";

            // 創建一個SqlParameter的實例來防止SQL注入
            SqlParameter[] writingsPara = new SqlParameter[]
            {
                new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId }
            };

            try
            {                
                DataTable writingsTB = _dbHelper.ArmyWebExecuteQuery(writingSql, writingsPara);

                if (writingsTB != null && writingsTB.Rows.Count > 0)
                {
                    
                    foreach (DataRow row in writingsTB.Rows)
                    {
                        WritingRes writingRes = new WritingRes() 
                        {
                            TitleHeading = row["title_heading"].ToString(),
                            Publisher = row["publisher"].ToString(),
                            JobCode = row["job_code"].ToString(),
                            WritingsCode = row["writings_code"].ToString(),
                            PressDate = _codeToName.dateTimeTran(row["press_date"].ToString(), "yyy年MM月dd日", true),
                            ApproveUnit = row["approve_unit"].ToString(),
                            DocDate = _codeToName.dateTimeTran(row["doc_date"].ToString(), "yyy年MM月dd日", true),
                            DocNo = row["doc_no"].ToString(),
                            DocCh = row["doc_ch"].ToString()
                        };
                        writeList.Add(writingRes);
                    }
                    return Ok(new { Result = "Success", writeList });
                }
                else
                {
                    return Ok(new { Result = "Fail", writeList });
                }
            }
            catch (Exception ex)
            {
                // 如果出現異常，返回錯誤信息
                WriteLog.Log(String.Format("【memberWritings Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【memberWritings Fail】" + ex.Message);
            }
        }

        //購買年資
        [HttpGet]
        [ActionName("buyExperience")]
        public IHttpActionResult buyExperience(string memberId)
        {
            List<BuyRes> buyList = new List<BuyRes>();
            string buySql = @"
                            SELECT 
                                LTRIM(RTRIM(start_effect_date)) as start_effect_date, LTRIM(RTRIM(end_effect_date)) as end_effect_date, 
                                LTRIM(RTRIM(approve_doc_date)) as approve_doc_date, LTRIM(RTRIM(approve_doc_no)) as approve_doc_no, 
                                doc_date, LTRIM(RTRIM(doc_no)) as doc_no, 
                                LTRIM(RTRIM(update_date)) as update_date
                            FROM 
                                Army.dbo.v_buy_experience
                            WHERE 
                                member_id = @memberId
                            ORDER BY
                                doc_date DESC";

            // 創建一個SqlParameter的實例來防止SQL注入
            SqlParameter[] buyPara = new SqlParameter[]
            {
                new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId }
            };

            try
            {
                DataTable buyTB = _dbHelper.ArmyWebExecuteQuery(buySql, buyPara);

                if (buyTB != null && buyTB.Rows.Count > 0)
                {
                    foreach (DataRow row in buyTB.Rows)
                    {
                        BuyRes buyRes = new BuyRes() 
                        {
                            StartEffectDate = row["start_effect_date"].ToString(),
                            EndEffectDate = row["end_effect_date"].ToString(),
                            ApproveDocDate = row["approve_doc_date"].ToString(),
                            ApproveDocNo = row["approve_doc_no"].ToString(),
                            DocDate = row["doc_date"].ToString(),
                            DocNo = row["doc_no"].ToString(),
                            UpdateDate = _codeToName.dateTimeTran(row["update_date"].ToString(), "yyy年MM月dd日", true)
                        };
                        buyList.Add(buyRes);
                    }
                    return Ok(new { Result = "Success", buyList });
                }
                else
                {
                    return Ok(new { Result = "Fail", buyList });
                }
            }
            catch (Exception ex)
            {
                // 如果出現異常，返回錯誤信息
                WriteLog.Log(String.Format("【buyExperience Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【buyExperience Fail】" + ex.Message);
            }
        }

        //出國記錄
        [HttpGet]
        [ActionName("exitCountry")]
        public IHttpActionResult exitCountry(string memberId)
        {
            List<ExitRes> exitList = new List<ExitRes>();
            string exitSql = @"
                            SELECT 
                                LTRIM(RTRIM(approve_unit)) as approve_unit, LTRIM(RTRIM(belong_unit)) as belong_unit, 
                                doc_date, LTRIM(RTRIM(doc_no)) as doc_no, 
                                LTRIM(RTRIM(apv_out_date)) as apv_out_date, LTRIM(RTRIM(apv_back_date)) as apv_back_date,
                                LTRIM(RTRIM(goal)) as goal, LTRIM(RTRIM(re_approve_date)) as re_approve_date, 
                                LTRIM(RTRIM(re_approve_mk)) as re_approve_mk, LTRIM(RTRIM(tranout_mk)) as tranout_mk, 
                                LTRIM(RTRIM(reject_mk)) as reject_mk
                            FROM 
                                Army.dbo.v_member_exit_country
                            WHERE 
                                member_id = @memberId
                            ORDER BY
                                doc_date DESC";

            // 創建一個SqlParameter的實例來防止SQL注入
            SqlParameter[] exitPara = new SqlParameter[]
            {
                new SqlParameter("@memberId", SqlDbType.VarChar) { Value = memberId }
            };

            try
            {
                DataTable exitTB = _dbHelper.ArmyWebExecuteQuery(exitSql, exitPara);

                if (exitTB != null && exitTB.Rows.Count > 0)
                {
                    foreach (DataRow row in exitTB.Rows)
                    {
                        string re_approve_mk = string.Empty;
                        string tranout_mk = string.Empty;
                        string reject_mk = string.Empty;
                        if(row["re_approve_mk"].ToString().Trim() == "1")
                        {
                            re_approve_mk = "已審核";
                        }
                        else
                        {
                            re_approve_mk = "未審核";
                        }

                        if (row["tranout_mk"].ToString().Trim() == "1")
                        {
                            tranout_mk = "已傳輸移民署";
                        }
                        else
                        {
                            tranout_mk = "未傳輸移民署";
                        }

                        if (row["reject_mk"].ToString().Trim() == "1")
                        {
                            reject_mk = "已註銷";
                        }
                        else
                        {
                            reject_mk = "未註銷";
                        }

                        
                        ExitRes exitRes = new ExitRes()
                        {
                            ApproveUnit = _codeToName.unitName(row["approve_unit"].ToString(),false),
                            BelongUnit = _codeToName.unitName(row["belong_unit"].ToString(),false),
                            DocDate = _codeToName.stringToDate(row["doc_date"].ToString()),
                            DocNo = row["doc_no"].ToString(),
                            ApvOutDate = _codeToName.stringToDate(row["apv_out_date"].ToString()),
                            ApvBackDate = _codeToName.stringToDate(row["apv_back_date"].ToString()),
                            Goal = row["goal"].ToString(),
                            ReApproveDate = _codeToName.stringToDate(row["re_approve_date"].ToString()),
                            ReApproveMk = re_approve_mk,
                            TranoutMk = tranout_mk,
                            RejectMk = reject_mk,
                        };
                        exitList.Add(exitRes);
                    }
                    return Ok(new { Result = "Success", exitList });
                }
                else
                {
                    return Ok(new { Result = "Fail", exitList });
                }
            }
            catch (Exception ex)
            {
                // 如果出現異常，返回錯誤信息
                WriteLog.Log(String.Format("【exitCountry Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【exitCountry Fail】" + ex.Message);
            }
        }

    }
}