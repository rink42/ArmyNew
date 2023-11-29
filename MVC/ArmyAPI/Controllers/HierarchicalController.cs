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

namespace ArmyAPI.Controllers
{
    public class HierarchicalController : ApiController
    {
        private readonly DbHelper _dbHelper;
        private readonly personnelDbSV _personnelDbSV;
        private readonly ChangeHierarchical _ChangeHierarchical;

        public HierarchicalController()
        {
            _dbHelper = new DbHelper();
            _personnelDbSV = new personnelDbSV();
            _ChangeHierarchical = new ChangeHierarchical();
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
                                        FROM v_member_data AS v JOIN rank AS r ON v.rank_code = r.rank_code 
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
                getMemberSql += ")";

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

                if (!Request.Content.IsMimeMultipartContent())
                {
                    return BadRequest("Invalid request, expecting multipart file upload");
                }

                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                // 取得上傳的文件
                foreach (var file in provider.Contents)
                {
                    //var fileName = file.Headers.ContentDisposition.FileName.Trim('\"');
                    var buffer = await file.ReadAsByteArrayAsync();

                    // 將文件保存到 MemoryStream
                    using (var Excelstream = new MemoryStream(buffer))
                    {

                        List<Dictionary<string, object>> excelData = new List<Dictionary<string, object>>();
                        using (var package = new ExcelPackage(Excelstream)) // 假定ERPlusReader接受Stream
                        {
                            var worksheet = package.Workbook.Worksheets[0];
                            var rowCount = worksheet.Dimension.Rows;
                            var cellCount = worksheet.Dimension.Columns;

                            string getMemberSql = @"SELECT v.member_id, v.member_name, v.rank_code, r.rank_title, v.supply_rank 
                                        FROM v_member_data AS v JOIN rank AS r ON v.rank_code = r.rank_code 
                                        WHERE v.member_id in (";
                            for (int row = 1; row <= rowCount; row++)
                            {
                                if (row == 1)
                                {
                                    getMemberSql += "'" + worksheet.Cells[row, 1].Text + "'";
                                }
                                else
                                {
                                    getMemberSql += ",'" + worksheet.Cells[row, 1].Text + "'";
                                }
                            }
                            getMemberSql += ")";

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
                    }
                }
                return Ok(new { Result = "Success", hierarchicalList });
            }
            catch (Exception ex)
            {
                return BadRequest("【changeHierarchical Fail】" + ex.ToString());
            }

        }



    }
}