using ArmyAPI.Services;
using ArmyAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Web.Http;
using OfficeOpenXml;
using System.IO;
using System.Net.Http;
using Microsoft.Ajax.Utilities;
using System.Data.SqlClient;

namespace ArmyAPI.Controllers
{
    public class HierarchicalController : ApiController
    {
        private readonly DbHelper _dbHelper;
        private readonly personnelDbSV _personnelDbSV;
        private readonly ChangeHierarchical _ChangeHierarchical;
        private readonly MakeReport _makeReport;

        public HierarchicalController()
        {
            _dbHelper = new DbHelper();
            _personnelDbSV = new personnelDbSV();
            _ChangeHierarchical = new ChangeHierarchical();
            _makeReport = new MakeReport();
        }

        // Post api/Hierarchical
        // 階級換敘 - 傳值
        [HttpPost]
        [ActionName("changeHierarchicalData")]
        public async Task<IHttpActionResult> changeHierarchicalData([FromBody] List<string> idNumber)
        {
            try
            {
                List<HierarchicalRes> hierarchicalList = new List<HierarchicalRes>();
                string getMemberSql = @"SELECT v.member_id, v.member_name, v.rank_code, r.rank_title, v.supply_rank 
                                        FROM Army.dbo.v_member_data AS v JOIN Army.dbo.rank AS r ON v.rank_code = r.rank_code 
                                        WHERE v.member_id in (";
                for(int i = 0; i < idNumber.Count; i++)
                {
                    if (i == 0)
                    {
                        getMemberSql += "'" + idNumber[i] + "'";
                    }
                    else
                    {
                        getMemberSql += ",'" + idNumber[i] + "'";
                    }
                }
                getMemberSql += ") ORDER BY CASE";

                int SortingWeight = 1;
                foreach (string memberId in idNumber)
                {
                    getMemberSql += " WHEN v.member_id = '" + memberId + "' THEN " + SortingWeight;
                    SortingWeight++;
                }
                getMemberSql += @" ELSE 999
                                  END;";

                DataTable getMemberTb = _dbHelper.ArmyWebExecuteQuery(getMemberSql);

                if(getMemberTb == null || getMemberTb.Rows.Count == 0)
                {
                    return Ok(new { Result = "No Member" });
                }
                
                foreach(DataRow row in getMemberTb.Rows)
                {
                    string rankCode = row["rank_code"].ToString();
                    string supplyRank = row["supply_rank"].ToString();
                    HierarchicalRes newHierarchical = _ChangeHierarchical.getNewHierarchical(rankCode, supplyRank);
                    newHierarchical.MemberId = row["member_id"].ToString();
                    newHierarchical.MemberName = row["member_name"].ToString();
                    int oldPoint = int.Parse(newHierarchical.OldSupplyPoint);
                    int newPoint = int.Parse(newHierarchical.NewSupplyPoint);
                    if(oldPoint >= newPoint)
                    {
                        newHierarchical.Massage = "轉後前俸點超過轉換後階級的最高點數";
                    }
                    else
                    {
                        newHierarchical.Massage = "該轉換後階級為自動晉支後的轉換結果";
                    }
                    hierarchicalList.Add(newHierarchical);
                }
                return Ok(new { Result = "Success",hierarchicalList });
            }
            catch (Exception ex)
            {
                return BadRequest("【changeHierarchical Fail】" + ex.ToString());
            }
            
        }


        // Post api/Hierarchical
        // 階級換敘 - 傳值
        [HttpPost]
        [ActionName("changeHierarchicalFile")]
        public async Task<IHttpActionResult> changeHierarchicalFile()
        {
            try
            {
                List<HierarchicalRes> hierarchicalList = new List<HierarchicalRes>();
                List<string> idNumber = new List<string>();
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return BadRequest("Invalid request, expecting multipart file upload");
                }

                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                // 取得上傳的文件
                foreach (var file in provider.Contents)
                {
                    List<string> Data = new List<string>();                    
                    var buffer = await file.ReadAsByteArrayAsync();
                    var filename = file.Headers.ContentDisposition.FileName.Trim('\"');                    
                    var fileExtension = Path.GetExtension(filename).ToLower();

                    // 將文件保存到 MemoryStream
                    using (var stream = new MemoryStream(buffer))
                    {
                        
                        switch (fileExtension)
                        {
                            case ".txt":
                                Data = _makeReport.txtReadLines(stream);
                                break;
                            case ".xlsx":
                                Data = _makeReport.excelReadLines(stream);
                                break;
                            default:
                                return Ok(new { Result = "不支援的檔案格式", hierarchicalList });
                        }
                    }    
                        string getMemberSql = @"SELECT v.member_id, v.member_name, v.rank_code, r.rank_title, v.supply_rank 
                                        FROM Army.dbo.v_member_data AS v JOIN Army.dbo.rank AS r ON v.rank_code = r.rank_code 
                                        WHERE v.member_id in (";
                        string orderSql = string.Empty;

                        int SortingWeight = 1;
                        for (int row = 0; row < Data.Count; row++)
                        {                            
                            if (row != 0)
                            {
                                getMemberSql += ",";
                            }                            
                            getMemberSql += "'" + Data[row] + "'";
                            orderSql += " WHEN v.member_id = '" + Data[row] + "' THEN " + SortingWeight;
                            SortingWeight++;

                        }
                        getMemberSql += ") ORDER BY CASE";
                        getMemberSql += orderSql;
                        getMemberSql += @" ELSE 999
                                  END;";
                        
                        DataTable getMemberTb = _dbHelper.ArmyWebExecuteQuery(getMemberSql);

                        if (getMemberTb == null || getMemberTb.Rows.Count == 0)
                        {
                            return Ok(new { Result = "No Member" });
                        }

                        foreach (DataRow row in getMemberTb.Rows)
                        {
                            string rankCode = row["rank_code"].ToString();
                            string supplyRank = row["supply_rank"].ToString();
                            HierarchicalRes newHierarchical = _ChangeHierarchical.getNewHierarchical(rankCode, supplyRank);
                            newHierarchical.MemberId = row["member_id"].ToString();
                            newHierarchical.MemberName = row["member_name"].ToString();
                            int oldPoint = int.Parse(newHierarchical.OldSupplyPoint);
                            int newPoint = int.Parse(newHierarchical.NewSupplyPoint);
                            if (oldPoint >= newPoint)
                            {
                                newHierarchical.Massage = "轉後前俸點超過轉換後階級的最高點數";
                            }
                            else
                            {
                                newHierarchical.Massage = "該轉換後階級為自動晉支後的轉換結果";
                            }
                            hierarchicalList.Add(newHierarchical);
                        }
                    }                
                return Ok(new { Result = "Success", hierarchicalList });
            }
            catch (Exception ex)
            {
                return BadRequest("【changeHierarchical Fail】" + ex.ToString());
            }

        }

        [HttpGet]
        [ActionName("getHierarchicalData")]
        public async Task<IHttpActionResult> getHierarchicalData() 
        {
            try 
            {
                string hierarDataSql = @"SELECT 
                                            adh.old_supply_point, adh.old_supply_rank, rk1.rank_title 'old_rank_title', adh.new_supply_point, adh.new_supply_rank, rk2.rank_title 'new_rank_title' 
                                         FROM 
                                            ArmyWeb.dbo.hierarchical as adh
                                         LEFT JOIN
                                            Army.dbo.rank as rk1 on rk1.rank_code = adh.old_rank_code
                                         LEFT JOIN
                                            Army.dbo.rank as rk2 on rk2.rank_code = adh.new_rank_code
                                         ORDER BY
                                            CAST(adh.old_rank_code AS int),
											CAST(adh.old_supply_rank AS int),
											CAST(adh.old_supply_point AS int)";

                DataTable hierarchicalTB = _dbHelper.ArmyWebExecuteQuery(hierarDataSql);
                if( hierarchicalTB != null && hierarchicalTB.Rows.Count != 0) 
                {
                    return Ok(new { Result = "Success", hierarchicalTB });
                    
                }
                else
                {
                    return Ok(new { Result = "Fail", hierarchicalTB });
                }
               
            }
            catch (Exception ex) 
            {
                WriteLog.Log(String.Format("【getHierarchicalData Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【getHierarchicalData Fail】" + ex.Message);
            }
        }

        [HttpPost]
        [ActionName("editHierarchicalData")]
        public async Task<IHttpActionResult> editHierarchicalData([FromBody] EditHierarchicalReq editData)
        {
            try
            {
                // step 2. 根據查詢到的table_name刪除代碼表資料
                string delCodeDataSql = @"DELETE FROM
                                            ArmyWeb.dbo.hierarchical";

                bool delResult = _dbHelper.ArmyWebUpdate(delCodeDataSql);


                // step 3. 將codeData_List的資料匯入資料表
                string inCodeDataSql = @"INSERT INTO 
                                            ArmyWeb.dbo.hierarchical
                                        VALUES ";
                for (int i = 0; i < editData.DataList.Count; i++)
                {
                    if (i != 0)
                    {
                        inCodeDataSql += ",";
                    }
                    inCodeDataSql += "('" + editData.DataList[i].OldSupplyPoint + "','" + editData.DataList[i].OldSupplyRank + "','"
                                          + editData.DataList[i].OldRankCode + "','" + editData.DataList[i].NewSupplyPoint + "','"
                                          + editData.DataList[i].NewSupplyRank + "','" + editData.DataList[i].NewRankCode + "')";
                }
                bool inResult = _dbHelper.ArmyWebUpdate(inCodeDataSql);

                if (!inResult)
                {
                    return Ok(new { Result = "更新失敗" });
                }

                return Ok(new { Result = "Success" });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【editHierarchicalData Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【editHierarchicalData Fail】" + ex.Message);
            }
        }


    }
}