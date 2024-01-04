using ArmyAPI.Models;
using ArmyAPI.Services;
//using iTextSharp.text;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.UI.WebControls;
using System.Windows.Media.Media3D;

namespace ArmyAPI.Controllers
{
    public class UploadController : ApiController
    {
        private readonly DbHelper _dbHelper;
        private readonly MakeReport _makeReport;
        private readonly CodetoName _codetoName;

        public UploadController()
        {
            _dbHelper = new DbHelper();
            _makeReport = new MakeReport();
            _codetoName = new CodetoName();
        }

        [HttpPost]
        [ActionName("UploadAndProcessFile")]
        public async Task<IHttpActionResult> UploadAndProcessFile(string uploadYear, string uploadMonth)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest("Unsupported media type");
            }

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            //民國轉西元
            CultureInfo culture = new CultureInfo("zh-TW");
            culture.DateTimeFormat.Calendar = new TaiwanCalendar();

            //若當月份有資料則刪除原資料
            string checkMonthSql = @"IF EXISTS (SELECT TOP (1) * FROM ArmyWeb.dbo.unit_specific_error WHERE upload_year = @uploadYear AND upload_month = @uploadMonth) 
                                     BEGIN    
                                            DELETE FROM  ArmyWeb.dbo.unit_specific_error    
                                            WHERE upload_year = @uploadYear AND upload_month = @uploadMonth
                                     END";
            SqlParameter[] checkMonthPara = 
            { 
                new SqlParameter ("@uploadYear", SqlDbType.VarChar){ Value = uploadYear },
                new SqlParameter ("@uploadMonth", SqlDbType.VarChar){ Value = uploadMonth}
            };

            bool checkMonthResult = _dbHelper.ArmyWebUpdate(checkMonthSql, checkMonthPara);
                
            //讀取匯入的檔案
            foreach (var file in provider.Contents)
            {
                var filename = file.Headers.ContentDisposition.FileName.Trim('\"');
                var buffer = await file.ReadAsByteArrayAsync();
                var fileContent = System.Text.Encoding.Default.GetString(buffer);
                
                // 處理文件內容
                try
                {
                    var lines = fileContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    
                    for (int j = 0; j < lines.Length; j++)
                    {
                        // 解析每一行的數據，根據您的資料格式
                        if(j == 0)
                        {
                            continue;
                        }
                        var fields = lines[j].Split('\t');
                        
                        
                        // 創建SQL命令來插入數據
                        string uploadSql = @"INSERT INTO 
                                                ArmyWeb.dbo.unit_specific_error  
                                            VALUES (";

                        // 設置參數值
                        SqlParameter[] uploadPara = new SqlParameter[30];
                        for (int i = 0; i < fields.Length + 2; i++)
                        {                            
                            if(i != 0)
                            {
                                uploadSql += ", ";
                            }
                            uploadSql += "@value" + i;
                            
                            switch (i)
                            {
                                case 28:
                                    uploadPara[i] = new SqlParameter("@value" + i, SqlDbType.VarChar) { Value = uploadYear };
                                    break;
                                case 29:
                                    uploadPara[i] = new SqlParameter("@value" + i, SqlDbType.VarChar) { Value = uploadMonth };                                   
                                    break;
                                default:
                                    uploadPara[i] = new SqlParameter("@value" + i, SqlDbType.VarChar) { Value = (object)fields[i].Trim() ?? DBNull.Value };
                                    break;
                            }                             
                        }
                        uploadSql += ")";
                       
                        
                        bool uploadResult = _dbHelper.ArmyWebUpdate(uploadSql, uploadPara);
                        
                    }
                    
                    return Ok(new { Result = "Success" });
                }
                catch (Exception ex)
                {
                    WriteLog.Log(String.Format("【UploadAndProcessFile Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                    return BadRequest("【UploadAndProcessFile Fail】");                    
                }
            }

            return BadRequest("No file uploaded.");
        }


        [HttpPost]
        [ActionName("RetireFileCheck")]
        public async Task<IHttpActionResult> RetireFileCheck()
        {
            //檢查是否有檔案匯入
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest("Unsupported media type");
            }

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

           
            List<RetireFileCheckRes> checkList = new List<RetireFileCheckRes>();

            try
            {
                //讀取匯入的檔案
                foreach (var file in provider.Contents)
                {                    
                    var buffer = await file.ReadAsByteArrayAsync();
                    var fileContent = System.Text.Encoding.Default.GetString(buffer);

                    using (var Excelstream = new MemoryStream(buffer))
                    {
                        List<Dictionary<string, object>> excelData = new List<Dictionary<string, object>>();

                        using (var package = new ExcelPackage(Excelstream)) // 假定ERPlusReader接受Stream
                        {
                            var worksheet = package.Workbook.Worksheets[0];
                            var rowCount = worksheet.Dimension.Rows;
                            var cellCount = worksheet.Dimension.Columns;

                            // 處理文件內容
                            for(int row = 2; row <= rowCount; row++)
                            {
                                string id = worksheet.Cells[row, 1].Text;

                                RetireFileCheckRes retireMem = new RetireFileCheckRes()
                                {
                                    Id = id,

                                    Distinction = worksheet.Cells[row, 2].Text,

                                    Analyze = worksheet.Cells[row, 3].Text
                                };

                                
                                //根據身分證搜尋姓名和單位
                                string checkMemberSql = @"SELECT
                                                            vmd.member_name, LTRIM(RTRIM(vmu.unit_title)) as unit_title
                                                          FROM
                                                            Army.dbo.v_member_data as vmd
                                                          LEFT JOIN
                                                            Army.dbo.v_mu_unit as vmu on vmu.unit_code = vmd.unit_code
                                                          WHERE
                                                            vmd.member_id = @idNumber";
                                SqlParameter[] checkMemberPara = { new SqlParameter("@idNumber", SqlDbType.VarChar) {Value = id } };
                                DataTable checkMemberTB = _dbHelper.ArmyWebExecuteQuery(checkMemberSql, checkMemberPara);
                                if(checkMemberTB != null && checkMemberTB.Rows.Count != 0)
                                {
                                    retireMem.Name = checkMemberTB.Rows[0]["member_name"].ToString();
                                    retireMem.Unit = checkMemberTB.Rows[0]["unit_title"].ToString();
                                }
                                else
                                {
                                    retireMem.Remark += " 未在現員檔內";
                                }
                                checkList.Add(retireMem);
                            }
                        } 
                    }
                }

                //檢查是否有重複的資料
                List<RetireFileCheckRes> sortList = checkList.OrderBy(id => id.Id).ToList();
                for (int i = 0; i < sortList.Count; i++)
                {
                    if (i == 0)
                    {
                        if (sortList[i].Id == sortList[i + 1].Id)
                        {
                            sortList[i].Remark += " 重複匯入";
                        }
                    }
                    else if (i == sortList.Count - 1)
                    {
                        if (sortList[i].Id == sortList[i - 1].Id)
                        {
                            sortList[i].Remark += " 重複匯入";
                        }
                    }
                    else
                    {
                        if (sortList[i].Id == sortList[i + 1].Id || sortList[i].Id == sortList[i - 1].Id)
                        {
                            sortList[i].Remark += " 重複匯入";
                        }
                    }
                }

                return Ok(new { Result = "Success", checkList = sortList });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【RetireFileCheck Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【RetireFileCheck Fail】");
            }
        }

        [HttpPost]
        [ActionName("RetireReasonInsert")]
        public async Task<IHttpActionResult> RetireReasonInsert(RetireReasonReq retireData)
        {
            try
            {
                foreach(var member in retireData.Member) 
                {
                    string insertSql = @"IF EXISTS (SELECT member_id FROM ArmyWeb.dbo.retire_reason WHERE member_id = @memberId)
                                        BEGIN
                                            DELETE FROM 
                                                ArmyWeb.dbo.retire_reason
                                            WHERE
                                                member_id = @memberId;

                                            INSERT INTO
                                                ArmyWeb.dbo.retire_reason
                                            VALUES
                                                (@memberName, @memberId, @unitTitle, @reasonDistinction, @reasonAnalyze);
                                        END
                                        ELSE
                                        BEGIN
                                            INSERT INTO
                                                ArmyWeb.dbo.retire_reason
                                            VALUES
                                                (@memberName, @memberId, @unitTitle, @reasonDistinction, @reasonAnalyze);
                                        END;";
                    SqlParameter[] insertPara =
                    {
                        new SqlParameter("@memberName", SqlDbType.VarChar){Value = (object)member.Name ?? DBNull.Value},
                        new SqlParameter("@memberId", SqlDbType.VarChar){Value = member.Id},
                        new SqlParameter("@unitTitle", SqlDbType.VarChar){Value = (object)member.Unit ?? DBNull.Value},
                        new SqlParameter("@reasonDistinction", SqlDbType.VarChar){Value = (object)member.Distinction ?? DBNull.Value},
                        new SqlParameter("@reasonAnalyze", SqlDbType.VarChar){Value = (object)member.Analyze ?? DBNull.Value},
                    };

                    bool insertResult = _dbHelper.ArmyWebUpdate(insertSql, insertPara);

                    if (!insertResult)
                    {
                        return Ok(new { Result = "Fail Insert" });
                    }
                }
                return Ok(new { Result = "success" });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【RetireReasonInsert Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【RetireReasonInsert Fail】");
            }
            
        }
    }
}
