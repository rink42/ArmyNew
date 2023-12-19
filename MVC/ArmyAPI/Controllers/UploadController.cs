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
        public async Task<IHttpActionResult> UploadAndProcessFile()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest("Unsupported media type");
            }

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            CultureInfo culture = new CultureInfo("zh-TW");
            culture.DateTimeFormat.Calendar = new TaiwanCalendar();

            string[] test;
            
            foreach (var file in provider.Contents)
            {
                var filename = file.Headers.ContentDisposition.FileName.Trim('\"');
                var buffer = await file.ReadAsByteArrayAsync();
                var fileContent = System.Text.Encoding.Default.GetString(buffer);
                //string fileContent = File.ReadAllText(buffer, Encoding.GetEncoding("Big5"));
                /*
                List<string> lines = new List<string>();
                using (var stream = new MemoryStream(buffer))
                {
                    
                    lines = _makeReport.txtReadLines(stream);
                       
                }
                */
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
                        test = fields;
                        
                        // 創建SQL命令來插入數據
                        string uploadSql = @"INSERT INTO 
                                                unit_specific_error  
                                            VALUES (";

                        // 設置參數值
                        SqlParameter[] uploadPara = new SqlParameter[28];
                        for (int i = 0; i < fields.Length; i++)
                        {                            
                            if(i != 0)
                            {
                                uploadSql += ", ";
                            }
                            uploadSql += "@value" + i;

                            switch (i)
                            {
                                case 5:
                                case 11:
                                case 21:
                                case 24:
                                    if (fields[i] != "0") 
                                    {                                        
                                        string Date = _codetoName.stringToDate(fields[i]);
                                        fields[i] = DateTime.Parse(Date.ToString(), culture).ToString("yyyy.MM.dd");
                                    }
                                    else
                                    {
                                        fields[i] = null;
                                    }
                                    break;
                                default:
                                    fields[i] = fields[i].Trim();
                                    break;
                            }
                            uploadPara[i] = new SqlParameter("@value" + i, SqlDbType.VarChar) { Value = (object)fields[i]?? DBNull.Value };
                        }
                        uploadSql += ")";
                        
                        bool uploadResult = _dbHelper.ArmyWebUpdate(uploadSql, uploadPara);
                        
                    }
                    
                    return Ok("File processed successfully.");
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
