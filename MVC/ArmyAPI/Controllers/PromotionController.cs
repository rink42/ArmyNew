using ArmyAPI.Models;
using ArmyAPI.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using OfficeOpenXml;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace ArmyAPI.Controllers
{
    public class PromotionController : ApiController
	{
        private readonly DbHelper _dbHelper;
        private readonly personnelDbSV _personnelDbSV;
        private readonly MakeReport _makeReport;

        public PromotionController()
        {
            _dbHelper = new DbHelper();
            _personnelDbSV = new personnelDbSV();
            _makeReport = new MakeReport();
        }
        
        
        // 套印報表 - 未產製區 - 匯入晉任名冊
        [HttpPost]
        [ActionName("addRegister")]
        public async Task<IHttpActionResult> addRegister(string formType)
        {
            
            bool addRegisters = true;

            //設置民國轉西元
            CultureInfo culture = new CultureInfo("zh-TW");
            culture.DateTimeFormat.Calendar = new TaiwanCalendar();

            //設置西元轉民國
            CultureInfo Tocalendar = new CultureInfo("zh-TW");
            Tocalendar.DateTimeFormat.Calendar = new TaiwanCalendar();

            DateTime Calendar = new DateTime();
            string AD = string.Empty;

            List<string> Formcell = new List<string>(new string[8]);

            var soldierDataList = new List<RegisterRes>();

            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return BadRequest("Invalid request, expecting multipart file upload");
                }

                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                formType = formType.Trim('\"');

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

                            for (int row = 2; row <= rowCount; row++)
                            {
                                for (int cell = 2; cell <= cellCount; cell++)
                                {
                                    //根據第1行的項目擷取所需的資料
                                    switch (worksheet.Cells[1, cell].Text)
                                    {
                                        case "一級單位":
                                            Formcell[0] = worksheet.Cells[row, cell].Text;
                                            break;
                                        case "單位":
                                            Formcell[1] = worksheet.Cells[row, cell].Text;
                                            break;
                                        case "姓名":
                                            Formcell[2] = worksheet.Cells[row, cell].Text;
                                            break;
                                        case "兵籍號碼":
                                            Formcell[3] = worksheet.Cells[row, cell].Text;
                                            break;
                                        case "身分證字號":
                                            Formcell[3] = worksheet.Cells[row, cell].Text;
                                            break;
                                        case "官科":
                                            Formcell[4] = worksheet.Cells[row, cell].Text;
                                            break;
                                        case "兵科":
                                            Formcell[4] = worksheet.Cells[row, cell].Text;
                                            break;
                                        case "階級":
                                            Formcell[5] = worksheet.Cells[row, cell].Text;
                                            break;
                                        case "晉任階級":
                                            Formcell[6] = worksheet.Cells[row, cell].Text;
                                            break;
                                        case "專長":
                                            Formcell[6] = worksheet.Cells[row, cell].Text;
                                            break;
                                        case "生效時間":
                                            Formcell[7] = worksheet.Cells[row, cell].Text;
                                            break;
                                        default:
                                            break;
                                    }
                                }

                                // 格式化資料存入資料庫
                                Register solderDataToDB = new Register
                                {
                                    PrimaryUnit = Formcell[0],                  //一級單位

                                    CurrentPosition = Formcell[1],              //現職單位

                                    Name = Formcell[2],                         //姓名

                                    IdNumber = Formcell[3],                     //兵籍號碼

                                    Branch = "陸軍",                            //軍種

                                    Rank = Formcell[4],                         //官科

                                    OldRankCode = Formcell[5],                  //原任官階

                                    NewRankCode = Formcell[6],                  //晉任官階

                                    EffectDate = null,                          //生效日期

                                    FormType = formType                           //初任 or 晉任
                                };


                                if (Formcell[7] != null)
                                {
                                    Calendar = DateTime.Parse(Formcell[7], culture);        //民國轉西元
                                    AD = Calendar.ToString("yyy/MM/dd", Tocalendar);        //西元轉民國
                                    solderDataToDB.EffectDate = Calendar;
                                }

                                //存入資料庫
                                string solderSql = "INSERT INTO register(primary_unit, current_position, name, id_number, branch, rank, old_rank_code, new_rank_code, effect_date, form_type) " +
                                        "VALUES (@primaryUnit, @currentPosition, @Name, @idNumber, @Branch, @Rank, @oldRankCode, @newRankCode, @effectDate, @formType)";
                                SqlParameter[] parameters =
                                {
                                    new SqlParameter("@primaryUnit", SqlDbType.VarChar) { Value =  (object)solderDataToDB.PrimaryUnit ?? DBNull.Value},
                                    new SqlParameter("@currentPosition", SqlDbType.VarChar) { Value = (object)solderDataToDB.CurrentPosition ?? DBNull.Value},
                                    new SqlParameter("@Name", SqlDbType.VarChar) { Value =  (object)solderDataToDB.Name ?? DBNull.Value},
                                    new SqlParameter("@idNumber", SqlDbType.VarChar) { Value =  (object)solderDataToDB.IdNumber},
                                    new SqlParameter("@Branch", SqlDbType.VarChar) { Value =  (object)solderDataToDB.Branch ?? DBNull.Value},
                                    new SqlParameter("@Rank", SqlDbType.VarChar) { Value =  (object)solderDataToDB.Rank ?? DBNull.Value},
                                    new SqlParameter("@oldRankCode", SqlDbType.VarChar) { Value =  (object)solderDataToDB.OldRankCode ?? DBNull.Value},
                                    new SqlParameter("@newRankCode", SqlDbType.VarChar) { Value =  (object)solderDataToDB.NewRankCode ?? DBNull.Value},
                                    new SqlParameter("@effectDate", SqlDbType.SmallDateTime) { Value =  (object)solderDataToDB.EffectDate ?? DBNull.Value},
                                    new SqlParameter("@formType", SqlDbType.VarChar) { Value =  (object)solderDataToDB.FormType ?? DBNull.Value}
                                };

                                addRegisters = _dbHelper.ArmyWebUpdate(solderSql, parameters);


                                //檢查身分證和人民有無誤植
                                MemRes checkMemberResult = _personnelDbSV.checkMember(solderDataToDB.IdNumber, solderDataToDB.Name);

                                //格式化傳回前端的資料
                                RegisterRes solderData = new RegisterRes
                                {
                                    PrimaryUnit = Formcell[0],                  //一級單位

                                    CurrentPosition = Formcell[1],              //現職單位

                                    Name = Formcell[2],                         //姓名

                                    IdNumber = Formcell[3],                     //兵籍號碼

                                    Branch = "陸軍",                            //軍種

                                    Rank = Formcell[4],                         //官科

                                    OldRankCode = Formcell[5],                  //原任官階

                                    NewRankCode = Formcell[6],                  //晉任官階

                                    EffectDate = Formcell[7],                   //生效日期

                                    FormType = formType,                          //初任 or 晉任

                                    CheckMemberResult = checkMemberResult.Result,  //兵籍號碼和姓名驗證

                                    InsertResult = addRegisters
                                };
                                soldierDataList.Add(solderData);
                            }
                        }
                    }
                }
                return Ok(new { Result = "Success", soldierDataList });
            }
            catch (Exception ex)
            {
                return BadRequest("【addRegister Fail】" + ex.Message);
            }
        }


        //套印報表 - 未產製區 - 名冊搜尋
        [HttpGet]
        [ActionName("getRegister")]
        public async Task<IHttpActionResult> getRegister(string formType, string effectDate)
        {
            formType = formType.Trim('\"');

            try
            {

                //設置民國轉西元
                CultureInfo culture = new CultureInfo("zh-TW");
                culture.DateTimeFormat.Calendar = new TaiwanCalendar();

                //設置西元轉民國
                CultureInfo Tocalendar = new CultureInfo("zh-TW");
                Tocalendar.DateTimeFormat.Calendar = new TaiwanCalendar();

                var soldierDataList = new List<RegisterRes>();

                string effectDateAD = string.Empty;

                string selRegistersSql = "SELECT * FROM register WHERE form_type = @formType";

                if(effectDate != null)
                {
                    selRegistersSql += " and CONVERT(VARCHAR(25), effect_date, 126) like @effectDate";
                    effectDateAD = DateTime.Parse(effectDate, culture).ToString("yyyy-MM-dd") + "%" ;
                }

                SqlParameter[] parameters = {
                    new SqlParameter("@formType", SqlDbType.VarChar) { Value =  (object)formType},
                    new SqlParameter("@effectDate", SqlDbType.VarChar) {Value = effectDateAD}
                };
                

                var registers = _dbHelper.ArmyWebExecuteQuery(selRegistersSql, parameters);

                if (registers == null || registers.Rows.Count == 0)
                {
                    return Ok(new { Result = "Not Found" });
                }

               
                foreach (DataRow row in registers.Rows)
                {
                    DateTime Calendar = new DateTime();
                    string AD = string.Empty;
                    MemRes checkMemberResult = new MemRes();
                    checkMemberResult.Result = "True";

                    if (row["name"].ToString() != null || row["id_number"].ToString() != null)
                    {
                        checkMemberResult = _personnelDbSV.checkMember(row["id_number"].ToString(), row["name"].ToString());
                    }

                    if (row["effect_date"].ToString() != null && row["effect_date"].ToString() != "")
                    {
                        Calendar = DateTime.Parse(row["effect_date"].ToString());
                        AD = Calendar.ToString("yyy.MM.dd", Tocalendar);
                    }

                   RegisterRes solderData = new RegisterRes
                   {
                        PrimaryUnit = row["primary_unit"].ToString(),                 //一級單位

                        CurrentPosition = row["current_position"].ToString(),         //現職單位

                        Name = row["name"].ToString(),                                //姓名

                        IdNumber = row["id_number"].ToString(),                       //兵籍號碼

                        Branch = row["branch"].ToString(),                            //軍種

                        Rank = row["rank"].ToString(),                                //官科

                        OldRankCode = row["old_rank_code"].ToString(),                //原任官階

                        NewRankCode = row["new_rank_code"].ToString(),                //晉任官階

                        EffectDate = AD,                                                            //生效日期

                        FormType = row["form_type"].ToString(),                       //初任 or 晉任

                        CheckMemberResult = checkMemberResult.Result,                               //兵籍號碼和姓名驗證

                        InsertResult = true                                                         //不用管
                   };

                    soldierDataList.Add(solderData);
                }

                return Ok(new { Result = "Success", soldierDataList });
            }
            catch (Exception ex)
            {
                return BadRequest("【getRegister Fail】" + ex.ToString());
            }
        }

        
        // 套印報表 - 未產製區 - 名冊刪除
        [HttpDelete]
        [ActionName("deleteRegister")]
        public async Task<IHttpActionResult> deleteRegister(string idNumber, string formType)
        {
            formType = formType.Trim('\"');
            idNumber = idNumber.Trim('\"');
            try
            {
                string delRegistersSql = "DELETE FROM ArmyWeb.dbo.register WHERE id_number = @idNumber and form_type = @formType";

                SqlParameter[] parameters = {
                    new SqlParameter("@idNumber", SqlDbType.VarChar) { Value =  (object)idNumber},
                    new SqlParameter("@formType", SqlDbType.VarChar) { Value =  (object)formType}
                };

                bool Result = _dbHelper.ArmyWebUpdate(delRegistersSql, parameters);

                if (Result)
                {
                    return Ok(new { Result = "Success", IdNumber = idNumber});
                }
                else
                {
                    return Ok(new { Result = "Delete Fail", IdNumber = idNumber});
                }
            }
            catch (Exception ex)
            {
                return BadRequest("【deleteRegister Fail】" + ex.ToString());
            }
        }


        //套印報表 - 未產製區 - 名冊修改
        [HttpPut]
        [ActionName("updateRegister")]
        public async Task<IHttpActionResult> updateRegister([FromBody] RegisterReq updateData)
        {
            try
            {
                CultureInfo Tocalendar = new CultureInfo("zh-TW");
                Tocalendar.DateTimeFormat.Calendar = new TaiwanCalendar();
                //DateTime Calendar = new DateTime();

                if (updateData.EffectDate != null)
                {
                    updateData.EffectDate = DateTime.Parse(updateData.EffectDate.ToString(), Tocalendar);
                }

                string upRegisterSql = @"UPDATE register SET primary_unit= @primary_unit, current_position = @current_position, 
                                        name = @name, id_number = @id_number, branch = @branch, rank = @rank, old_rank_code = @old_rank_code, 
                                        new_rank_code = @new_rank_code, effect_date = @effect_date, form_type = @form_type 
                                        WHERE id_number = @ID  and form_type = @Form";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@primary_unit", SqlDbType.VarChar) { Value =  (object)updateData.PrimaryUnit ?? DBNull.Value},
                    new SqlParameter("@current_position", SqlDbType.VarChar) { Value = (object)updateData.CurrentPosition ?? DBNull.Value},
                    new SqlParameter("@name", SqlDbType.VarChar) { Value =  (object)updateData.Name ?? DBNull.Value},
                    new SqlParameter("@id_number", SqlDbType.VarChar) { Value =  (object)updateData.IdNumber},
                    new SqlParameter("@branch", SqlDbType.VarChar) { Value =  (object)updateData.Branch ?? DBNull.Value},
                    new SqlParameter("@rank", SqlDbType.VarChar) { Value =  (object)updateData.Rank ?? DBNull.Value},
                    new SqlParameter("@old_rank_code", SqlDbType.VarChar) { Value =  (object)updateData.OldRankCode ?? DBNull.Value},
                    new SqlParameter("@new_rank_code", SqlDbType.VarChar) { Value =  (object)updateData.NewRankCode ?? DBNull.Value},
                    new SqlParameter("@effect_date", SqlDbType.SmallDateTime) { Value =  (object)updateData.EffectDate ?? DBNull.Value},
                    new SqlParameter("@form_type", SqlDbType.VarChar) { Value =  (object)updateData.FormType},
                    new SqlParameter("@ID", SqlDbType.VarChar) { Value =  (object)updateData.OriginalId},
                    new SqlParameter("@Form", SqlDbType.VarChar) { Value =  (object)updateData.OriginalForm}
                };

                var Result = _dbHelper.ArmyWebUpdate(upRegisterSql, parameters);

                if (Result)
                {
                    return Ok(new { Result = "Success" });
                }
                else
                {
                    return Ok(new { Result = "Update Fail" });
                }

            }
            catch (Exception ex)
            {
                return BadRequest("【updateRegister Fail】" + ex.ToString());
            }
        }


        //套印報表 - 未產製區 - 產製名冊
        [HttpPost]
        [ActionName("addCaseRegister")]
        public async Task<IHttpActionResult> addCaseRegister([FromBody]SaveCaseReq caseData)
        {
            //設置民國轉西元
            CultureInfo culture = new CultureInfo("zh-TW");
            culture.DateTimeFormat.Calendar = new TaiwanCalendar();

            //設置西元轉民國
            CultureInfo Tocalendar = new CultureInfo("zh-TW");
            Tocalendar.DateTimeFormat.Calendar = new TaiwanCalendar();

            var soldierDataList = new List<SaveCaseRes>();
            List<CaseExcelReq> excelDataList = new List<CaseExcelReq>();
           
            string formType = string.Empty;
            string dateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

            try
            {
                // 1.新增案件人員名單到 case_register
                foreach (var idNumber in caseData.IdNumber)
                {
                    
                    string selectSql = "SELECT * FROM register WHERE id_number = @id_number";
                    SqlParameter[] selectParameters = { new SqlParameter("@id_number", idNumber) };

                    DataTable dt = _dbHelper.ArmyWebExecuteQuery(selectSql, selectParameters);
                    
                    if(dt == null || dt.Rows.Count == 0)
                    {
                        return Ok(new { Result = "No Member Found", CaseId = dateTime, Pdf = string.Empty, Excel = string.Empty, soldierDataList });
                    }
                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        if (row["effect_date"] != DBNull.Value || row["effect_date"].ToString() != "")
                        {
                            row["effect_date"] = DateTime.Parse(row["effect_date"].ToString());
                        }

                        string insertSql = "INSERT INTO case_register(case_id, case_name, primary_unit, current_position, name, id_number, branch, rank, old_rank_code, new_rank_code, effect_date, form_type) " +
                                        "VALUES (@caseId, @caseName, @primaryUnit, @currentPosition, @Name, @idNumber, @Branch, @Rank, @oldRankCode, @newRankCode, @effectDate, @formType)";

                        SqlParameter[] insertParameters =
                        {
                            new SqlParameter("@caseId", SqlDbType.VarChar){ Value = dateTime},
                            new SqlParameter("@caseName", SqlDbType.VarChar){ Value = caseData.CaseName},
                            new SqlParameter("@primaryUnit", SqlDbType.VarChar) { Value =  (object) row["primary_unit"] ?? DBNull.Value},
                            new SqlParameter("@currentPosition", SqlDbType.VarChar) { Value = (object)row["current_position"] ?? DBNull.Value},
                            new SqlParameter("@Name", SqlDbType.VarChar) { Value =  (object)row["name"] ?? DBNull.Value},
                            new SqlParameter("@idNumber", SqlDbType.VarChar) { Value =  (object)row["id_number"]},
                            new SqlParameter("@Branch", SqlDbType.VarChar) { Value =  (object)row["branch"] ?? DBNull.Value},
                            new SqlParameter("@Rank", SqlDbType.VarChar) { Value =  (object)row["rank"] ?? DBNull.Value},
                            new SqlParameter("@oldRankCode", SqlDbType.VarChar) { Value =  (object)row["old_rank_code"] ?? DBNull.Value},
                            new SqlParameter("@newRankCode", SqlDbType.VarChar) { Value =  (object)row["new_rank_code"] ?? DBNull.Value},
                            new SqlParameter("@effectDate", SqlDbType.SmallDateTime) { Value =  (object)row["effect_date"] ?? DBNull.Value},
                            new SqlParameter("@formType", SqlDbType.VarChar) { Value =  (object)row["form_type"]}
                        };
                        
                        bool insertCaseResult = _dbHelper.ArmyWebUpdate(insertSql, insertParameters);
                        
                        bool delRegisterResult = false;
                        
                        // 2. 刪除未產製列表中的人員
                        if (insertCaseResult)
                        {
                            string delRegiater = "DELETE FROM register WHERE id_number = @idNumber";
                            SqlParameter[] delSqlParameter = { new SqlParameter("@idNumber",SqlDbType.VarChar) { Value = row["id_number"] } };
                            delRegisterResult = _dbHelper.ArmyWebUpdate(delRegiater, delSqlParameter);

                        }

                        SaveCaseRes Result = new SaveCaseRes
                        {
                            InsertResult = insertCaseResult,

                            DelResult = delRegisterResult,

                            CaseId = dateTime,

                            CaseName = caseData.CaseName,

                            MemberId = row["id_number"].ToString()
                        };

                        soldierDataList.Add(Result);

                        // 3. 建立excel數據
                        string newEffectDate = string.Empty;
                        string Year = string.Empty;
                        string Month = string.Empty;
                        string Day = string.Empty;
                        if (row["effect_date"].ToString() != "")
                        {
                            DateTime Calendar = DateTime.Parse(row["effect_date"].ToString());
                            newEffectDate = Calendar.ToString("yyy.MM.dd", Tocalendar);
                            Year = Calendar.ToString("yyy", Tocalendar);
                            Month = Calendar.ToString("MM", Tocalendar);
                            Day = Calendar.ToString("dd", Tocalendar);
                        }
                      
                        formType = row["form_type"].ToString();
                        CaseExcelReq excelData = new CaseExcelReq 
                        {
                            PrimaryUnit = row["primary_unit"].ToString(),

                            CurrentPosition = row["current_position"].ToString(),

                            Name = row["name"].ToString(),

                            IdNumber = row["id_number"].ToString(),

                            Branch = row["branch"].ToString(),

                            Rank = row["rank"].ToString(),

                            OldRankCode = row["old_rank_code"].ToString(),

                            NewRankCode = row["new_rank_code"].ToString(),

                            EffectDate = newEffectDate,

                            Year = Year,

                            Month = Month,

                            Day = Day,

                            FormType = row["form_type"].ToString(),

                            CaseId = caseData.CaseName
                        };

                        excelDataList.Add(excelData);
                    }
                }

                // 4. 建立 PDF 和 Excel 報表
                
                string pdfDataSql = "SELECT * FROM case_register WHERE case_id = @caseId AND case_name = @caseName";
                SqlParameter[] pdfDataParameter =
                {
                    new SqlParameter("@caseId",SqlDbType.VarChar){Value = dateTime},
                    new SqlParameter("@caseName",SqlDbType.VarChar){Value = caseData.CaseName}
                };
                DataTable pdfDataTb = _dbHelper.ArmyWebExecuteQuery(pdfDataSql, pdfDataParameter);
                if (pdfDataTb == null || pdfDataTb.Rows.Count == 0)
                {
                    return Ok(new { Result = "No Data", CaseId = dateTime, Pdf = string.Empty, Excel = string.Empty, soldierDataList });
                }

                int tbRow = pdfDataTb.Rows.Count % 15;
                if (tbRow != 0)
                {
                    for (int i = 0; i < 15 - tbRow; i++)
                    {
                        DataRow row = pdfDataTb.NewRow();
                        pdfDataTb.Rows.Add(row);
                    }
                }

                string pdfName = "~/Report/" + dateTime + "_" + caseData.CaseName + ".pdf";
                string excelName = "~/Report/" + dateTime + "_" + caseData.CaseName + ".xlsx";
                string pdfOutputPath = System.Web.Hosting.HostingEnvironment.MapPath(pdfName);
                string excelOutputPath = System.Web.Hosting.HostingEnvironment.MapPath(excelName);
                string urlPath = Request.RequestUri.GetLeftPart(UriPartial.Authority) + $"/{ConfigurationManager.AppSettings.Get("ApiPath")}/Report/";
                string pdfHttpPath = string.Empty;
                string excelHttpPath = string.Empty;
                bool pdfResult = true;
                bool excelResult = true;

                if (formType == "初任")
                {
                    excelResult = _makeReport.exportFirstToExcel(excelDataList, excelOutputPath);
                    pdfResult = _makeReport.exportFirstToPDF(pdfDataTb, pdfOutputPath);
                }
                else
                {
                    excelResult = _makeReport.exportPromotionToExcel(excelDataList, excelOutputPath);
                    pdfResult = _makeReport.exportPromotionToPDF(pdfDataTb, pdfOutputPath);
                }

                if (excelResult || pdfResult)
                {
                    pdfName = dateTime + "_" + caseData.CaseName + ".pdf";
                    excelName = dateTime + "_" + caseData.CaseName + ".xlsx";

                    pdfHttpPath = urlPath + pdfName;
                    excelHttpPath = urlPath + excelName;
                }
               

                // 5. 新增一個新的案件 case_list
                string caselistSql = "INSERT INTO case_list(case_id, case_name, create_member, member_id, host_url, pdf_name, excel_name, create_date)" +
                                       "VALUES (@case_id, @case_name, @create_member, @member_id, @host_url, @pdf_name, @excel_name, @create_date)";

                SqlParameter[] caseParameters =
                {
                        new SqlParameter("@case_id", SqlDbType.VarChar){ Value = dateTime},
                        new SqlParameter("@case_name", SqlDbType.VarChar){ Value = caseData.CaseName},
                        new SqlParameter("@create_member", SqlDbType.VarChar){ Value = caseData.CreateMember},
                        new SqlParameter("@member_id", SqlDbType.VarChar){ Value = caseData.CreateMemberId},
                        new SqlParameter("@host_url", SqlDbType.VarChar){ Value = urlPath},
                        new SqlParameter("@pdf_name", SqlDbType.VarChar){ Value = pdfName},
                        new SqlParameter("@excel_name", SqlDbType.VarChar){ Value = excelName},
                        new SqlParameter("@create_date", SqlDbType.SmallDateTime){ Value = DateTime.Now}
                };

                bool insertCaseList = _dbHelper.ArmyWebUpdate(caselistSql, caseParameters);
                if (!insertCaseList)
                {
                    return Ok(new { Result = "Insert Case List Fail", CaseId = dateTime, Pdf = pdfHttpPath, Excel = excelHttpPath, soldierDataList });
                }


                return Ok(new { Result = "Success", CaseId = dateTime, Pdf = pdfHttpPath, Excel = excelHttpPath, soldierDataList });
            }
            catch (Exception ex)
            {
                return BadRequest("【addCaseRegister Fail】" + ex.ToString());
            }
        }


        //套印報表 - 未產製區 - 落款編輯
        [HttpPut]
        [ActionName("updateSignature")]
        public async Task<IHttpActionResult> updateSignature([FromBody] UpdateSignatureReq updateData)
        {
            try
            {

                string upRegisterSql = @"UPDATE signature SET name = @Name, rank_title = @jobTitle 
                                          WHERE name = @orignName";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@Name", SqlDbType.VarChar) { Value =  (object)updateData.Name ?? DBNull.Value},
                    new SqlParameter("@jobTitle", SqlDbType.VarChar) { Value = (object)updateData.JobTitle ?? DBNull.Value},
                    new SqlParameter("@orignName", SqlDbType.VarChar) { Value =  (object)updateData.OrignName ?? DBNull.Value},
                };

                var Result = _dbHelper.ArmyWebUpdate(upRegisterSql, parameters);

                if (!Result)
                {
                    return Ok(new { Result = "Update Fail" });
                }

                return Ok(new { Result = "Success" });
            }
            catch (Exception ex)
            {
                return BadRequest("【updateSignature Fail】" + ex.ToString());
            }
        }


        // 套印報表 - 未產製區 - 落款資訊
        [HttpGet]
        [ActionName("getSignature")]
        public async Task<IHttpActionResult> getSignature()
        {
            try
            {
                DataTable signatureTb = new DataTable();
                string signatureSql = "SELECT * FROM signature";
                signatureTb = _dbHelper.ArmyWebExecuteQuery(signatureSql);
                if (signatureTb == null || signatureTb.Rows.Count == 0)
                {
                    return Ok(new {Result = "No Signature", signatureTb});
                }

                return Ok(new { Result = "Success", signatureTb });
            }
            catch (Exception ex)
            {
                return BadRequest("【getSignature Fail】" + ex.ToString());
            }
        }

    }
}
