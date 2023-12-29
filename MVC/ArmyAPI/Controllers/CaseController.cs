using ArmyAPI.Models;
using ArmyAPI.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using static log4net.Appender.RollingFileAppender;

namespace ArmyAPI.Controllers
{
    public class CaseController : ApiController
    {
        private readonly DbHelper _dbHelper;
        private readonly personnelDbSV _personnelDbSV;
        private readonly MakeReport _makeReport;

        public CaseController()
        {
            _dbHelper = new DbHelper();
            _personnelDbSV = new personnelDbSV();
            _makeReport = new MakeReport();
        }

        [HttpGet]
        [ActionName("searchCase")]
        public async Task<IHttpActionResult> searchCase(string caseName, string createDate)
        {
            try
            {
                var caseList = new List<CaseListRes>();
                string newCaseName = "%" + caseName + "%";
                string newCreateDateTime = "%" + createDate + "%";
                string searchCaseSql = @"with 
                                         temptable as (
	                                        SELECT 
		                                        case_id, '士任令(' + form_type + ')字第' + RIGHT('000' + CAST(name_count AS VARCHAR(3)), 3) + '號' as caseName 
	                                        FROM 
		                                        ArmyWeb.dbo.case_list	                                       
                                         )
                                         SELECT 
                                            cl.*, tt.caseName '任官令名稱'
                                         FROM 
                                            ArmyWeb.dbo.case_list as cl
                                         LEFT JOIN 
                                            temptable as tt on tt.case_id = cl.case_id";

                if (caseName != null && createDate != null)
                {
                    searchCaseSql += " WHERE tt.caseName like @caseName and CONVERT(VARCHAR(25), cl.create_date, 126) like @createDate";
                }
                else if (caseName != null && createDate == null)
                {
                    searchCaseSql += " WHERE tt.caseName like @caseName";
                }
                else if (caseName == null && createDate != null)
                {
                    searchCaseSql += " WHERE CONVERT(VARCHAR(25), cl.create_date, 126) like @createDate";
                }


                SqlParameter[] parameters =
                {
                    new SqlParameter("@caseName", SqlDbType.VarChar) { Value =  (object)newCaseName ?? DBNull.Value},
                    new SqlParameter("@createDate", SqlDbType.VarChar) { Value = (object)newCreateDateTime ?? DBNull.Value}
                };

                DataTable caseTable = _dbHelper.ArmyWebExecuteQuery(searchCaseSql, parameters);

                if (caseTable == null || caseTable.Rows.Count == 0)
                {
                    return Ok(new { Result = "Case Not Found" ,caseList});
                }

                

                foreach (DataRow row in caseTable.Rows)
                {
                    string CreateDateStr = DateTime.Parse(row["create_date"].ToString()).ToString("yyyy-MM-dd");
                    CaseListRes caseData = new CaseListRes
                    {
                        CaseId = row["case_id"].ToString(),

                        CaseName = row["任官令名稱"].ToString(),

                        CreateMember = row["create_member"].ToString(),

                        MemberId = row["member_id"].ToString(),

                        HostUrl = row["host_url"].ToString(),

                        PdfName = row["pdf_name"].ToString(),

                        ExcelName = row["excel_name"].ToString(),

                        CreateDate = CreateDateStr
                    };
                    caseList.Add(caseData);
                }

                return Ok(new {Result = "Success", caseList });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【searchCase Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【searchCase Fail】" + ex.ToString());
            }
            
        }


        [HttpGet]
        [ActionName("searchCaseRegister")]
        public async Task<IHttpActionResult> searchCaseRegister(string caseId, string idNumber, string Name)
        {
            //設置西元轉民國
            CultureInfo culture = new CultureInfo("zh-TW");
            culture.DateTimeFormat.Calendar = new TaiwanCalendar();
            string AD = string.Empty;
            var caseRegisterList = new List<CaseRegisterRes>();
            
            try
            {                
                string getIdNumber = "%" + idNumber + "%";
                string getName = "%" + Name + "%";
                string searchCaseSql = @"SELECT 
                                            cr.*,'士任令(' + cl.form_type + ')字第' + RIGHT('000' + CAST(cl.name_count AS VARCHAR(3)), 3) + '號' as '任官令名稱'
                                         FROM 
                                            ArmyWeb.dbo.case_register as cr
                                         LEFT JOIN
											ArmyWeb.dbo.case_list as cl on cl.case_id = cr.case_id
                                         WHERE 
                                            cr.case_id = @caseId";


                if (idNumber != null)
                {
                    searchCaseSql += " and id_number like @idNumber";
                }

                if (Name != null)
                {
                    searchCaseSql += " and name like @Name";

                }


                SqlParameter[] parameters =
                {
                    new SqlParameter("@caseId", SqlDbType.VarChar) { Value =  (object)caseId ?? DBNull.Value},
                    new SqlParameter("@idNumber", SqlDbType.VarChar) { Value = (object)getIdNumber ?? DBNull.Value},
                    new SqlParameter("@Name", SqlDbType.VarChar) {Value = (object)getName ?? DBNull.Value}
                };

                DataTable caseRegisterTable = _dbHelper.ArmyWebExecuteQuery(searchCaseSql, parameters);

                if (caseRegisterTable == null)
                {
                    return Ok(new { Result = "Case Register Not Found" , caseRegisterList });
                }

                

                foreach(DataRow row in caseRegisterTable.Rows)
                {
                    if (row["effect_date"].ToString() != "")
                    {
                        DateTime Calendar = DateTime.Parse(row["effect_date"].ToString());
                        AD = Calendar.ToString("yyy.MM.dd", culture);        //西元轉民國
                    }
                    
                    CaseRegisterRes caseData = new CaseRegisterRes
                    {
                        CaseId = row["case_id"].ToString().Trim(),

                        CaseName = row["任官令名稱"].ToString().Trim(),

                        PrimaryUnit = row["primary_unit"].ToString().Trim(),

                        CurrentPosition = row["current_position"].ToString().Trim(),

                        Name = row["name"].ToString().Trim(),

                        IdNumber = row["id_number"].ToString().Trim(),

                        Branch = row["branch"].ToString().Trim(),

                        Rank = row["rank"].ToString().Trim(),

                        OldRankCode = row["old_rank_code"].ToString().Trim(),

                        NewRankCode = row["new_rank_code"].ToString().Trim(),

                        EffectDate = AD,

                        FormType = row["form_type"].ToString().Trim()
                    };
                    caseRegisterList.Add(caseData);
                }

                return Ok(new { Result = "Success", caseRegisterList });
            }
            catch(Exception ex) 
            {
                WriteLog.Log(String.Format("【searchCaseRegister Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【searchCaseRegister Fail】" + ex.ToString());
            }
        }


        [HttpPost]
        [ActionName("reprintCaseRegister")]
        public async Task<IHttpActionResult> reprintCaseRegister([FromBody]ReprintCaseReq reprintData)
        {
            //設置民國轉西元
            CultureInfo culture = new CultureInfo("zh-TW");
            culture.DateTimeFormat.Calendar = new TaiwanCalendar();

            //設置西元轉民國
            CultureInfo Tocalendar = new CultureInfo("zh-TW");
            Tocalendar.DateTimeFormat.Calendar = new TaiwanCalendar();

            List<SaveCaseRes> soldierDataList = new List<SaveCaseRes>();
            List<GeneralReq> generalReq = new List<GeneralReq>();
            List<CaseExcelReq> excelDataList = new List<CaseExcelReq>();

            DateTime date = DateTime.Now;

            string dateTime = date.ToString("yyyyMMddHHmmss");
            string caseName = string.Empty;
            string formType = string.Empty;
            string pdfHttpPath = string.Empty;
            string excelHttpPath = string.Empty;
            string pdfName = string.Empty;
            string excelName = string.Empty;
            string pdfOutputPath = string.Empty;
            string excelOutputPath = string.Empty;
            string urlPath = string.Empty;

            bool pdfResult = true;
            bool excelResult = true;

            int nameCount = 0;
            try
            {
                // 1.查詢case_list 任官令名稱
                string nameCountSql = @"SELECT
                                            cl.name_count, '士任令(補)字第' + RIGHT('000' + CAST(cl.name_count AS VARCHAR(3)), 3) + '號' as '任官令名稱'
                                        FROM
                                            ArmyWeb.dbo.case_list as cl                            
                                        WHERE
                                            cl.case_id = @caseId";
                SqlParameter[] nameCountPara = { new SqlParameter("@caseId",SqlDbType.VarChar) {Value = reprintData.OldCaseId } };
                DataTable nameCountTB = _dbHelper.ArmyWebExecuteQuery(nameCountSql, nameCountPara);
                if(nameCountTB != null && nameCountTB.Rows.Count != 0)
                {
                    caseName = nameCountTB.Rows[0]["任官令名稱"].ToString();
                    nameCount = int.Parse(nameCountTB.Rows[0]["name_count"].ToString());
                }

                // 2. 查詢case_register表中的補印人員資料
                string selCaseSql = "SELECT * FROM case_register WHERE case_id = @caseId and id_number in ('";
                selCaseSql += string.Join("','", reprintData.IdNumber);
                selCaseSql += "')";

                SqlParameter[] Parameters = { new SqlParameter("@caseId", SqlDbType.VarChar) { Value = reprintData.OldCaseId } };

                DataTable caseRegister = _dbHelper.ArmyWebExecuteQuery(selCaseSql, Parameters);

                if (caseRegister == null || caseRegister.Rows.Count == 0)
                {
                    return Ok(new { Result = "Member Not Found" , CaseId = dateTime, Pdf = string.Empty, Excel = string.Empty, soldierDataList });
                }

                // 3. 新增補印人員到case_register
                foreach (DataRow row in caseRegister.Rows)
                {
                    if (row["effect_date"] != DBNull.Value || row["effect_date"].ToString() != "")
                    {
                        row["effect_date"] = DateTime.Parse(row["effect_date"].ToString());
                    }

                    string insertSql = "INSERT INTO case_register(case_id, primary_unit, current_position, name, id_number, branch, rank, old_rank_code, new_rank_code, effect_date, form_type) " +
                                        "VALUES (@caseId, @primaryUnit, @currentPosition, @Name, @idNumber, @Branch, @Rank, @oldRankCode, @newRankCode, @effectDate, @formType)";

                    SqlParameter[] insertParameters =
                    {
                        new SqlParameter("@caseId", SqlDbType.VarChar) { Value = dateTime},                        
                        new SqlParameter("@primaryUnit", SqlDbType.VarChar) { Value =  (object) row["primary_unit"] ?? DBNull.Value},
                        new SqlParameter("@currentPosition", SqlDbType.VarChar) { Value = (object)row["current_position"] ?? DBNull.Value},
                        new SqlParameter("@Name", SqlDbType.VarChar) { Value =  (object)row["name"] ?? DBNull.Value},
                        new SqlParameter("@idNumber", SqlDbType.VarChar) { Value =  (object)row["id_number"]},
                        new SqlParameter("@Branch", SqlDbType.VarChar) { Value =  (object)row["branch"] ?? DBNull.Value},
                        new SqlParameter("@Rank", SqlDbType.VarChar) { Value =  (object)row["rank"] ?? DBNull.Value},
                        new SqlParameter("@oldRankCode", SqlDbType.VarChar) { Value =  (object)row["old_rank_code"] ?? DBNull.Value},
                        new SqlParameter("@newRankCode", SqlDbType.VarChar) { Value =  (object)row["new_rank_code"] ?? DBNull.Value},
                        new SqlParameter("@effectDate", SqlDbType.SmallDateTime) { Value =  (object)row["effect_date"] ?? DBNull.Value},
                        new SqlParameter("@formType", SqlDbType.VarChar) { Value = "補"}
                    };

                    bool insertCaseResult = _dbHelper.ArmyWebUpdate(insertSql, insertParameters);

                    SaveCaseRes insertResult = new SaveCaseRes
                    {
                        InsertResult = insertCaseResult,

                        CaseId = dateTime,

                        CaseName = caseName,

                        MemberId = row["id_number"].ToString()
                    };

                    soldierDataList.Add(insertResult);

                    //4. 建立excel資料
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

                        FormType = "補",

                        CaseId = caseName
                    };

                    excelDataList.Add(excelData);

                    //5. 查詢職階級別
                    string generalSql = @"SELECT
                                                rank_code
                                              FROM
                                                Army.dbo.rank
                                              WHERE
                                                rank_title = @rankTitle";
                    SqlParameter[] generalPara = { new SqlParameter("@rankTitle", SqlDbType.VarChar) { Value = row["new_rank_code"].ToString() } };
                    DataTable generalTB = _dbHelper.ArmyWebExecuteQuery(generalSql, generalPara);
                    if (generalTB != null && generalTB.Rows.Count != 0)
                    {
                        int rank = int.Parse(generalTB.Rows[0]["rank_code"].ToString());
                        if (rank > 0 && rank <= 23)
                        {
                            GeneralReq generalRecord = new GeneralReq
                            {
                                GeneralId = row["id_number"].ToString(),

                                GeneralName = row["name"].ToString(),

                                GeneralRank = generalTB.Rows[0]["rank_code"].ToString()
                            };
                            generalReq.Add(generalRecord);
                        }
                    }

                }

                // 6. 建立Pdf和Excel檔案
                string pdfDataSql = "SELECT * FROM case_register WHERE case_id = @CaseId";
                SqlParameter[] pdfDataParameter =
                {
                    new SqlParameter("@CaseId",SqlDbType.VarChar){Value = dateTime},
                };
                DataTable pdfDataTb = _dbHelper.ArmyWebExecuteQuery(pdfDataSql, pdfDataParameter);
                if (pdfDataTb == null || pdfDataTb.Rows.Count == 0)
                {
                    return Ok(new { Result = "No Data", soldierDataList });
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

                pdfName = "~/Report/" + dateTime + "_" + caseName + ".pdf";
                excelName = "~/Report/" + dateTime + "_" + caseName + ".xlsx";
                pdfOutputPath = System.Web.Hosting.HostingEnvironment.MapPath(pdfName);
                excelOutputPath = System.Web.Hosting.HostingEnvironment.MapPath(excelName);
                urlPath = Request.RequestUri.GetLeftPart(UriPartial.Authority) + $"/{ConfigurationManager.AppSettings.Get("ApiPath")}/Report/";
                
                if (formType == "初任")
                {
                    excelResult = _makeReport.exportFirstToExcel(excelDataList, excelOutputPath);
                    pdfResult = _makeReport.exportFirstToPDF(pdfDataTb, pdfOutputPath, caseName);
                }
                else
                {
                    excelResult = _makeReport.exportPromotionToExcel(excelDataList, excelOutputPath);
                    pdfResult = _makeReport.exportPromotionToPDF(pdfDataTb, pdfOutputPath, caseName);
                }

                if (excelResult)
                {                    
                    excelName = dateTime + "_" + caseName + ".xlsx";
                    excelHttpPath = urlPath + excelName;                   
                }

                if (pdfResult)
                {
                    pdfName = dateTime + "_" + caseName + ".pdf";
                    pdfHttpPath = urlPath + pdfName;
                }

                _makeReport.checkGeneral(generalReq, reprintData.CreateMemberId, excelName, "補印任官令下載");

                // 7. 插入新的資料到case_list
                string updateCaseListSql = @"INSERT INTO case_list(case_id, name_count, create_member, member_id, form_type, host_url, pdf_name, excel_name, create_date) 
                                              VALUES (@newCaseId, @nameCount, @createMember, @memberId, @formType, @hostUrl, @pdfName, @excelName, @createDate);";

                SqlParameter[] updateCaseListParams = {
                        new SqlParameter("@newCaseId", SqlDbType.VarChar){ Value = dateTime },
                        new SqlParameter("@nameCount", SqlDbType.VarChar){ Value = nameCount },
                        new SqlParameter("@createMember", SqlDbType.VarChar){ Value = reprintData.CreateMember },
                        new SqlParameter("@memberId", SqlDbType.VarChar){ Value = reprintData.CreateMemberId },
                        new SqlParameter("@formType", SqlDbType.VarChar){ Value =  "補"},
                        new SqlParameter("@hostUrl", SqlDbType.VarChar){ Value = urlPath},
                        new SqlParameter("@pdfName", SqlDbType.VarChar){ Value = pdfName},
                        new SqlParameter("@excelName", SqlDbType.VarChar){ Value = excelName},
                        new SqlParameter("@createDate", SqlDbType.SmallDateTime){ Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                };

                bool addCaseResult = _dbHelper.ArmyWebUpdate(updateCaseListSql, updateCaseListParams);

                if (!addCaseResult)
                {
                    return Ok(new { Result = "Insert reprintCase List Fail", CaseId = dateTime, Pdf = pdfHttpPath, Excel = excelHttpPath, soldierDataList });
                }

                return Ok(new { Result = "Success", CaseId = dateTime, Pdf = pdfHttpPath, Excel = excelHttpPath, soldierDataList });
            }
            catch (Exception ex)
            {
                // 產製失敗刪除失敗的案件
                string deleteCaseRegisterQuery = "DELETE FROM case_register WHERE case_id = @caseId";
                SqlParameter[] delCaseRegPm = {
                    new SqlParameter("@caseId", SqlDbType.VarChar) { Value = dateTime }
                };
                bool delCaseRegResult = _dbHelper.ArmyWebUpdate(deleteCaseRegisterQuery, delCaseRegPm);

                string deleteCaseListQuery = "DELETE FROM case_list WHERE case_id = @caseId";
                SqlParameter[] delCasePm = {
                    new SqlParameter("@caseId", SqlDbType.VarChar) { Value = dateTime }
                };
                bool delCaseResult = _dbHelper.ArmyWebUpdate(deleteCaseListQuery, delCasePm);

                // 檢查 PDF 是否存在
                if (File.Exists(pdfOutputPath))
                {                     
                    File.Delete(pdfOutputPath);
                }

                // 檢查 Excel 是否存在
                if (File.Exists(excelOutputPath))
                {                    
                    File.Delete(excelOutputPath);
                }

                // 處理異常
                WriteLog.Log(String.Format("【reprintCaseRegister Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【reprintCaseRegister Fail】" + ex.ToString());
            }
        }


        [HttpDelete]
        [ActionName("DeleteCase")]
        public async Task<IHttpActionResult> DeleteCase(string caseId)
        {
            List<SaveRegRes> insertRegList = new List<SaveRegRes>();
            try
            {
                // 1. 從case_register搜尋資料
                string searchQuery = "SELECT * FROM case_register WHERE case_id = @caseId";
                SqlParameter[] searchParams = { 
                    new SqlParameter("@caseId", SqlDbType.VarChar) { Value = caseId }                    
                };
                DataTable caseRegTable = _dbHelper.ArmyWebExecuteQuery(searchQuery, searchParams);
                
                if(caseRegTable != null && caseRegTable.Rows.Count != 0)
                {
                    if (caseRegTable.Rows[0]["form_type"].ToString().Trim() != "補")
                    {
                        foreach (DataRow row in caseRegTable.Rows)
                        {
                            // 2. 將搜尋到的資料新增到register中
                            if (row["effect_date"] != DBNull.Value || row["effect_date"].ToString() != "")
                            {
                                row["effect_date"] = DateTime.Parse(row["effect_date"].ToString());
                            }

                            string solderSql = "INSERT INTO register(primary_unit, current_position, name, id_number, branch, rank, old_rank_code, new_rank_code, effect_date, form_type) " +
                                                "VALUES (@primaryUnit, @currentPosition, @Name, @idNumber, @Branch, @Rank, @oldRankCode, @newRankCode, @effectDate, @formType)";
                            SqlParameter[] parameters =
                            {
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

                            bool addRegisters = _dbHelper.ArmyWebUpdate(solderSql, parameters);



                            //檢查身分證和人民有無誤植
                            MemRes checkMemberResult = _personnelDbSV.checkMember(row["id_number"].ToString(), row["name"].ToString());

                            SaveRegRes inRegResult = new SaveRegRes
                            {
                                CheckMemberResult = checkMemberResult.Result,

                                InsertResult = addRegisters,

                                MemberId = row["id_number"].ToString(),

                                MemberName = row["name"].ToString()
                            };

                            insertRegList.Add(inRegResult);

                            if (!addRegisters)
                            {
                                return Ok(new { Result = "Insert Fail", delCaseReg = false, delCase = false, insertRegList });
                            }
                        }
                    }
                }

                // 3. 刪除 PDF 和 Excel
                string getFileSql = @"SELECT
                                        pdf_name, excel_name
                                      FROM
                                        ArmyWeb.dbo.case_list
                                      WHERE
                                        case_id = @caseId";
                SqlParameter[] getFilePara = { new SqlParameter("@caseId",SqlDbType.VarChar) {Value = caseId } };
                DataTable getFileTB = _dbHelper.ArmyWebExecuteQuery(getFileSql, getFilePara);
                if(getFileTB != null && getFileTB.Rows.Count != 0)
                {
                    string pdfName = "~/Report/" + getFileTB.Rows[0]["pdf_name"].ToString();
                    string excelName = "~/Report/" + getFileTB.Rows[0]["excel_name"].ToString();
                    string pdfOutputPath = System.Web.Hosting.HostingEnvironment.MapPath(pdfName);
                    string excelOutputPath = System.Web.Hosting.HostingEnvironment.MapPath(excelName);

                    // 檢查 PDF 是否存在
                    if (File.Exists(pdfOutputPath))
                    {
                        // 刪除PDF
                        File.Delete(pdfOutputPath);
                    }

                    // 檢查 Excel 是否存在
                    if (File.Exists(excelOutputPath))
                    {
                        // 刪除Excel
                        File.Delete(excelOutputPath);
                    }
                }

                // 4. 刪除case_register和case_list中的資料
                string deleteCaseRegisterQuery = "DELETE FROM case_register WHERE case_id = @caseId";
                SqlParameter[] delCaseRegPm = { 
                    new SqlParameter("@caseId", SqlDbType.VarChar) { Value = caseId }                  
                };
                bool delCaseRegResult = _dbHelper.ArmyWebUpdate(deleteCaseRegisterQuery, delCaseRegPm);

                string deleteCaseListQuery = "DELETE FROM case_list WHERE case_id = @caseId";
                SqlParameter[] delCasePm = {
                    new SqlParameter("@caseId", SqlDbType.VarChar) { Value = caseId }                    
                };
                bool delCaseResult = _dbHelper.ArmyWebUpdate(deleteCaseListQuery, delCasePm);


                return Ok(new { Result = "Success" , delCaseReg = delCaseRegResult, delCase = delCaseResult, insertRegList});
            }
            catch (Exception ex)
            {
                // 處理異常，您可以根據需要進一步詳細地處理
                WriteLog.Log(String.Format("【DeleteCase Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【DeleteCase Fail】" + ex.ToString());
            }
        }


    }
}