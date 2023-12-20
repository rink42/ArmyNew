using ArmyAPI.Services;
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
                   
                    return InternalServerError(ex);
                }
            }

            return BadRequest("No file uploaded.");
        }

        

    }
}
