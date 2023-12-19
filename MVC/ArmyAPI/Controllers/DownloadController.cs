using ArmyAPI.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
//using System.Web.Mvc;

namespace ArmyAPI.Controllers
{
    public class DownloadController : ApiController
    {
        private readonly DbHelper _dbHelper;
        private readonly MakeReport _makeReport;

        public DownloadController()
        {
            _dbHelper = new DbHelper();
            _makeReport = new MakeReport();
        }

        [HttpGet]
        [ActionName("getDownloadCount")]
        public IHttpActionResult getDownloadCount()
        {
            try
            {
                string loadCountSql = @"SELECT 
                                            memb_action, 
                                            COUNT(*) AS action_count
                                        FROM 
                                            dbo.report_record
                                        GROUP BY 
                                            memb_action
                                        ORDER BY 
                                            action_count DESC";

                DataTable loadCountTB = _dbHelper.ArmyWebExecuteQuery(loadCountSql);

                if(loadCountTB != null && loadCountTB.Rows.Count != 0)
                {
                    return Ok(new { Result = "Success", loadCountTB });
                }
                else
                {
                    return Ok(new {Result = "Fail",  loadCountTB});
                }
            }
            catch (Exception ex) 
            {
                WriteLog.Log(String.Format("【getDownloadCount Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【getDownloadCount Fail】" + ex.Message);
                
            }
        }

        [HttpGet]
        [ActionName("searchDownloadCount")]
        public IHttpActionResult searchDownloadCount(string keyWord)
        {
            try
            {
                string loadCountSql = @"SELECT 
                                            memb_action, 
                                            COUNT(*) AS action_count
                                        FROM 
                                            dbo.report_record
                                        WHERE
                                            memb_action like @KeyWord
                                        GROUP BY 
                                            memb_action
                                        ORDER BY 
                                            action_count DESC";

                SqlParameter[] loadCountPara = { new SqlParameter("@KeyWord", SqlDbType.NVarChar) { Value = "%" + keyWord + "%" } };

                DataTable loadCountTB = _dbHelper.ArmyWebExecuteQuery(loadCountSql, loadCountPara);

                if (loadCountTB != null && loadCountTB.Rows.Count != 0)
                {
                    return Ok(new { Result = "Success", loadCountTB });
                }
                else
                {
                    return Ok(new { Result = "Fail", loadCountTB });
                }
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【searchDownloadCount Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【searchDownloadCount Fail】" + ex.Message);

            }
        }

        [HttpGet]
        [ActionName("getMemberCount")]
        public IHttpActionResult getMemberCount()
        {
            try
            {
                string membCountSql = @"SELECT 
                                            rr.memb_id,
                                            vmd.member_name,
                                            rr.memb_action,
                                            COUNT(*) AS action_count
                                        FROM 
                                            ArmyWeb.dbo.report_record as rr
                                        LEFT JOIN
                                            Army.dbo.v_member_data as vmd ON vmd.member_id = rr.memb_id                                            
                                        GROUP BY 
                                            rr.memb_id,
                                            rr.memb_action,
                                            vmd.member_name
                                        ORDER BY 
                                            action_count DESC";               

                DataTable membCountTB = _dbHelper.ArmyWebExecuteQuery(membCountSql);

                if (membCountTB != null && membCountTB.Rows.Count != 0)
                {
                    return Ok(new { Result = "Success", membCountTB });
                }
                else
                {
                    return Ok(new { Result = "Fail", membCountTB });
                }
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【getMemberCount Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【getMemberCount Fail】" + ex.Message);
            }
        }

        [HttpGet]
        [ActionName("searchMemberCount")]
        public IHttpActionResult searchMemberCount(string keyWord)
        {
            try
            {
                string membCountSql = @"SELECT 
                                            rr.memb_id,
                                            vmd.member_name,
                                            rr.memb_action,
                                            COUNT(*) AS action_count
                                        FROM 
                                            ArmyWeb.dbo.report_record as rr
                                        LEFT JOIN
                                            Army.dbo.v_member_data as vmd ON vmd.member_id = rr.memb_id
                                        WHERE
                                            concat(rr.memb_id, vmd.member_name, memb_action) like @keyWord
                                        GROUP BY 
                                            rr.memb_id,
                                            rr.memb_action,
                                            vmd.member_name
                                        ORDER BY 
                                            action_count DESC";

                SqlParameter[] membCountPara = { new SqlParameter("@KeyWord", SqlDbType.NVarChar) { Value = "%" + keyWord + "%" } };

                DataTable membCountTB = _dbHelper.ArmyWebExecuteQuery(membCountSql, membCountPara);

                if (membCountTB != null && membCountTB.Rows.Count != 0)
                {
                    return Ok(new { Result = "Success", membCountTB });
                }
                else
                {
                    return Ok(new { Result = "Fail", membCountTB });
                }
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【searchMemberCount Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【searchMemberCount Fail】" + ex.Message);
            }
        }

    }
}
