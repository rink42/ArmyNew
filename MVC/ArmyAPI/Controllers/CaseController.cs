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
                caseName = "%" + caseName + "%"; 
                var caseList = new List<CaseListRes>();
                string createDateTime2 = string.Empty;
                string searchCaseSql = "SELECT * FROM case_list";

                if (caseName != null && createDate != null)
                {
                    searchCaseSql += " WHERE case_name like @caseName and CONVERT(VARCHAR(25), create_date, 126) like @createDate";
                    createDateTime2 = DateTime.Parse(createDate).ToString("yyyy-MM-dd") + "%";
                }
                else if (caseName != null && createDate == null)
                {
                    searchCaseSql += " WHERE case_name like @caseName ";
                }
                else if (caseName == null && createDate != null)
                {
                    searchCaseSql += " WHERE CONVERT(VARCHAR(25), create_date, 126) like @createDate ";
                    createDateTime2 = "%" + DateTime.Parse(createDate).ToString("yyyy-MM-dd") + "%";
                }


                SqlParameter[] parameters =
                {
                    new SqlParameter("@caseName", SqlDbType.VarChar) { Value =  (object)caseName ?? DBNull.Value},
                    new SqlParameter("@createDate", SqlDbType.VarChar) { Value = (object)createDateTime2 ?? DBNull.Value}
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

                        CaseName = row["case_name"].ToString(),

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
                return BadRequest("【Fail】" + ex.ToString());
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
                string getCaseId = "%" + caseId + "%";
                string getIdNumber = "%" + idNumber + "%";
                string getName = "%" + Name + "%";
                string searchCaseSql = "SELECT * FROM case_register WHERE case_id like @caseId";


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
                    new SqlParameter("@caseId", SqlDbType.VarChar) { Value =  (object)getCaseId ?? DBNull.Value},
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

                        CaseName = row["case_name"].ToString().Trim(),

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
                return BadRequest("【Fail】" + ex.ToString());
            }
        }


        [HttpPost]
        [ActionName("reprintCaseRegister")]
        public async Task<IHttpActionResult> reprintCaseRegister([FromBody]ReprintCaseReq reprintData)
        {
            try
            {   //設置民國轉西元
                CultureInfo culture = new CultureInfo("zh-TW");
                culture.DateTimeFormat.Calendar = new TaiwanCalendar();

                //設置西元轉民國
                CultureInfo Tocalendar = new CultureInfo("zh-TW");
                Tocalendar.DateTimeFormat.Calendar = new TaiwanCalendar();

                List<SaveCaseRes> soldierDataList = new List<SaveCaseRes>();
                List<CaseExcelReq> excelDataList = new List<CaseExcelReq>();
                
                string formType = string.Empty;
                string dateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

                // 1. 查詢case_register表中的資料
                string selCaseSql = "SELECT * FROM case_register WHERE case_id = @caseId and id_number in ('" + reprintData.IdNumber[0] + "'";
                foreach (string id in reprintData.IdNumber)
                {
                    selCaseSql += ",'" + id + "'";
                }
                selCaseSql += ")";

                SqlParameter[] Parameters = { new SqlParameter("@caseId", SqlDbType.VarChar) { Value = reprintData.CaseId } };

                DataTable caseRegister = _dbHelper.ArmyWebExecuteQuery(selCaseSql, Parameters);

                if (caseRegister == null || caseRegister.Rows.Count == 0)
                {
                    return Ok(new { Result = "Member Not Found" , CaseId = dateTime, Pdf = string.Empty, Excel = string.Empty, soldierDataList });
                }

                // 2. 新增補印人員到case_register
                foreach (DataRow row in caseRegister.Rows)
                {
                    if (row["effect_date"] != DBNull.Value || row["effect_date"].ToString() != "")
                    {
                        row["effect_date"] = DateTime.Parse(row["effect_date"].ToString());
                    }

                    string insertSql = "INSERT INTO case_register(case_id, case_name, primary_unit, current_position, name, id_number, branch, rank, old_rank_code, new_rank_code, effect_date, form_type) " +
                                        "VALUES (@caseId, @caseName, @primaryUnit, @currentPosition, @Name, @idNumber, @Branch, @Rank, @oldRankCode, @newRankCode, @effectDate, @formType)";

                    SqlParameter[] insertParameters =
                    {
                        new SqlParameter("@caseId", SqlDbType.VarChar) { Value = dateTime},
                        new SqlParameter("@caseName", SqlDbType.VarChar) { Value = reprintData.NewCaseId},
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

                        CaseName = reprintData.NewCaseId,

                        MemberId = row["id_number"].ToString()
                    };

                    soldierDataList.Add(insertResult);

                    //3. 建立excel資料
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

                        CaseId = reprintData.NewCaseId
                    };

                    excelDataList.Add(excelData);
                }

                // 4. 建立Pdf和Excel檔案
                string pdfDataSql = "SELECT * FROM case_register WHERE case_id = @CaseId AND case_name = @CaseName";
                SqlParameter[] pdfDataParameter =
                {
                    new SqlParameter("@CaseId",SqlDbType.VarChar){Value = dateTime},
                    new SqlParameter("@CaseName",SqlDbType.VarChar){Value = reprintData.NewCaseId}
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

                string pdfName = "~/Report/" + dateTime + "_" + reprintData.NewCaseId + ".pdf";
                string excelName = "~/Report/" + dateTime + "_" + reprintData.NewCaseId + ".xlsx";
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

                if (excelResult)
                {                    
                    excelName = dateTime + "_" + reprintData.NewCaseId + ".xlsx";
                    excelHttpPath = urlPath + excelName;                   
                }

                if (pdfResult)
                {
                    pdfName = dateTime + "_" + reprintData.NewCaseId + ".pdf";
                    pdfHttpPath = urlPath + pdfName;
                }

                // 5. 插入新的資料到case_list
                string updateCaseListSql = @"INSERT INTO case_list(case_id, case_name, create_member, member_id, host_url, pdf_name, excel_name, create_date) 
                                              VALUES (@newCaseId, @newCaseName, @createMember, @memberId, @hostUrl, @pdfName, @excelName, @createDate);";

                SqlParameter[] updateCaseListParams = {
                        new SqlParameter("@newCaseId", SqlDbType.VarChar){ Value = dateTime },
                        new SqlParameter("@newCaseName", SqlDbType.VarChar){ Value = reprintData.NewCaseId },
                        new SqlParameter("@createMember", SqlDbType.VarChar){ Value = reprintData.CreateMember },
                        new SqlParameter("@memberId", SqlDbType.VarChar){ Value = reprintData.CreateMemberId },
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
                // 處理異常
                return BadRequest("【Fail】" + ex.ToString());
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
                
                if(caseRegTable == null || caseRegTable.Rows.Count == 0)
                {
                    return Ok(new { Result = "Case Member Not Found" , delCaseReg = false, delCase = false, insertRegList });
                }

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
                
                // 3. 刪除case_register和case_list中的資料
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

                // 4. 刪除 PDF 和 Excel
                string pdfName = "~/Report/" + caseId + "_" + caseRegTable.Rows[0]["case_name"].ToString().Trim() + ".pdf";
                string excelName = "~/Report/" + caseId + "_" + caseRegTable.Rows[0]["case_name"].ToString().Trim() + ".xlsx";
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
                return Ok(new { Result = "Success" , delCaseReg = delCaseRegResult, delCase = delCaseResult, insertRegList});
            }
            catch (Exception ex)
            {
                // 處理異常，您可以根據需要進一步詳細地處理
                return BadRequest("Fail" + ex.ToString());
            }
        }


    }
}