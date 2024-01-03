using ArmyAPI.Models;
using ArmyAPI.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.SqlClient;
using System.Data;
using NPOI.HSSF.UserModel;


namespace ArmyAPI.Controllers
{
    public class FormCodeController : ApiController
    {
        private readonly DbHelper _dbHelper;
        private readonly personnelDbSV _personnelDbSV;

        public FormCodeController()
        {
            _dbHelper = new DbHelper();
            _personnelDbSV = new personnelDbSV();
        }


        // GET api/FormCode
        // 線傳代碼 - 傳輸表單查詢
        [HttpGet]
        [ActionName("searchTranscode")]
        public async Task<IHttpActionResult> searchTranscode(string keyWord)
        {
            try 
            {
                List<TransCodeRes> transCodeList = new List<TransCodeRes>();
                string newKeyWord = "%" + keyWord + "%";
                string transCodeSql = "SELECT LTRIM(RTRIM(trans_type)) as trans_type, LTRIM(RTRIM(trans_code)) as trans_code, LTRIM(RTRIM(memo)) as memo FROM Army.dbo.memb_trans_code WHERE concat(trans_type, trans_code, memo) like @keyWord";
                SqlParameter[] codeParameters = { new SqlParameter("@keyWord", SqlDbType.VarChar) { Value = (object)newKeyWord ?? DBNull.Value } };
                DataTable tranCodeTb = _dbHelper.ArmyWebExecuteQuery(transCodeSql, codeParameters);
                if (tranCodeTb == null || tranCodeTb.Rows.Count == 0)
                {
                    return Ok(new { Result = "Trans Code Not Found", transCodeList });
                }

                foreach (DataRow row in tranCodeTb.Rows)
                {
                    string codeTitle = string.Empty;
                    string codeTitleSql = "SELECT LTRIM(RTRIM(trans_title)) as trans_title FROM ArmyWeb.dbo.memb_trans_code_data WHERE trans_type = @transType";
                    SqlParameter[] titleParameters = { new SqlParameter("@transType", SqlDbType.VarChar) { Value = row["trans_type"].ToString() } };
                    DataTable codeTitleTb = _dbHelper.ArmyWebExecuteQuery(codeTitleSql, titleParameters);

                    if (codeTitleTb != null && codeTitleTb.Rows.Count > 0)
                    {
                        codeTitle = codeTitleTb.Rows[0]["trans_title"].ToString();
                    }

                    TransCodeRes transCode = new TransCodeRes
                    {
                        TransType = row["trans_type"].ToString(),

                        TransTitle = codeTitle,

                        TransCode = row["trans_code"].ToString(),

                        Memo = row["memo"].ToString()
                    };

                    transCodeList.Add(transCode);
                }
                return Ok(new { Result = "Success", transCodeList });
            }
            catch(Exception ex) 
            {
                WriteLog.Log(String.Format("【searchTranscode Fail】" + DateTime.Now.ToString() + " " + ex.Message));                
                return BadRequest("【searchTranscode Fail】");
            }
            
        }

        // GET api/FormCode
        // 線傳代碼 - 線傳各類代碼查詢
        [HttpGet]
        [ActionName("searchCodeTable")]
        public async Task<IHttpActionResult> searchCodeTable(string keyWord)
        {
            try
            {
                keyWord = "%" + keyWord + "%";
                List<CodeTableRes> codeDataList = new List<CodeTableRes>();
                string tableCatalog1 = "Army";
                string tableCatalog2 = "ArmyWeb";
                string tableManagerSql = "SELECT * FROM ArmyWeb.dbo.code_table_manage WHERE type = @type";
                SqlParameter[] oldParameter = { new SqlParameter("@type", SqlDbType.VarChar) { Value = "old"} };
                SqlParameter[] newParameter = { new SqlParameter("@type", SqlDbType.VarChar) { Value = "new"} };

                DataTable oldTableTB = _dbHelper.ArmyWebExecuteQuery(tableManagerSql, oldParameter);
                DataTable newTableTB = _dbHelper.ArmyWebExecuteQuery(tableManagerSql, newParameter);
                if (oldTableTB == null || oldTableTB.Rows.Count == 0)
                {
                    return Ok(new { Result = "Table Not Found" });
                }

                foreach (DataRow row in oldTableTB.Rows)
                {
                    string codeTableSql = "SELECT COLUMN_NAME FROM Army.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_CATALOG = @tableCatalog AND TABLE_NAME = @tableName";
                    SqlParameter[] codeParameter =
                    {
                        new SqlParameter("@tableCatalog",SqlDbType.VarChar){Value = tableCatalog1},
                        new SqlParameter("@tableName",SqlDbType.VarChar){Value= row["table_name"].ToString()}
                    };

                    
                    DataTable codeTableTB = _dbHelper.ArmyWebExecuteQuery(codeTableSql, codeParameter);
                    if (codeTableTB.Rows.Count != 0)
                    {
                        List<string> columnName = new List<string>();
                        string codeDataSql = "SELECT * FROM Army.dbo." + row["table_name"].ToString() + " WHERE concat(" + codeTableTB.Rows[0]["COLUMN_NAME"].ToString();
                        foreach (DataRow columnRow in codeTableTB.Rows)
                        {
                            codeDataSql += "," + columnRow["COLUMN_NAME"].ToString();
                            columnName.Add(columnRow["COLUMN_NAME"].ToString());
                        }
                        codeDataSql += ") like @keyWord";

                        
                        SqlParameter[] codeDataSqlParameter =
                        {
                            new SqlParameter("@keyWord",SqlDbType.VarChar){Value = keyWord}
                        };

                        DataTable codeDataTB = _dbHelper.ArmyWebExecuteQuery(codeDataSql, codeDataSqlParameter);
                        if (codeDataTB == null || codeDataTB.Rows.Count == 0)
                        {
                            CodeTableRes codeTableResult = new CodeTableRes
                            {
                                Result = "No Data",
                                Table = row["table_name"].ToString() + "-" + row["table_ch_name"].ToString(),
                                Columns = columnName,
                                codeTable = null
                            };
                            codeDataList.Add(codeTableResult);
                        }
                        else if (codeDataTB.Rows.Count > 20)
                        {
                            CodeTableRes codeTableResult = new CodeTableRes
                            {
                                Result = "Data Rows Count Over 20",
                                Table = row["table_name"].ToString() + "-" + row["table_ch_name"].ToString(),
                                Columns = columnName,
                                codeTable = null
                            };
                            codeDataList.Add(codeTableResult);
                        }
                        else
                        {
                            CodeTableRes codeTableResult = new CodeTableRes
                            {
                                Result = "Success",
                                Table = row["table_name"].ToString() + "-" + row["table_ch_name"].ToString(),
                                Columns = columnName,
                                codeTable = codeDataTB
                            };
                            codeDataList.Add(codeTableResult);
                        }
                    }
                }

                foreach (DataRow row in newTableTB.Rows)
                {
                    string codeTableSql = "SELECT COLUMN_NAME FROM ArmyWeb.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_CATALOG = @tableCatalog AND TABLE_NAME = @tableName";
                    SqlParameter[] codeParameter =
                    {
                        new SqlParameter("@tableCatalog",SqlDbType.VarChar){Value = tableCatalog2},
                        new SqlParameter("@tableName",SqlDbType.VarChar){Value= row["table_name"].ToString()}
                    };


                    DataTable codeTableTB = _dbHelper.ArmyWebExecuteQuery(codeTableSql, codeParameter);
                    if (codeTableTB.Rows.Count != 0)
                    {
                        List<string> columnName = new List<string>();
                        string codeDataSql = "SELECT * FROM ArmyWeb.dbo." + row["table_name"].ToString() + " WHERE concat(" + codeTableTB.Rows[0]["COLUMN_NAME"].ToString();
                        foreach (DataRow columnRow in codeTableTB.Rows)
                        {
                            codeDataSql += "," + columnRow["COLUMN_NAME"].ToString();
                            columnName.Add(columnRow["COLUMN_NAME"].ToString());
                        }
                        codeDataSql += ") like @keyWord";


                        SqlParameter[] codeDataSqlParameter =
                        {
                            new SqlParameter("@keyWord",SqlDbType.VarChar){Value = keyWord}
                        };

                        DataTable codeDataTB = _dbHelper.ArmyWebExecuteQuery(codeDataSql, codeDataSqlParameter);
                        if (codeDataTB == null || codeDataTB.Rows.Count == 0)
                        {
                            CodeTableRes codeTableResult = new CodeTableRes
                            {
                                Result = "No Data",
                                Table = row["table_name"].ToString() + "-" + row["table_ch_name"].ToString(),
                                Columns = columnName,
                                codeTable = null
                            };
                            codeDataList.Add(codeTableResult);
                        }
                        else if (codeDataTB.Rows.Count > 20)
                        {
                            CodeTableRes codeTableResult = new CodeTableRes
                            {
                                Result = "Data Rows Count Over 20",
                                Table = row["table_name"].ToString() + "-" + row["table_ch_name"].ToString(),
                                Columns = columnName,
                                codeTable = null
                            };
                            codeDataList.Add(codeTableResult);
                        }
                        else
                        {
                            CodeTableRes codeTableResult = new CodeTableRes
                            {
                                Result = "Success",
                                Table = row["table_name"].ToString() + "-" + row["table_ch_name"].ToString(),
                                Columns = columnName,
                                codeTable = codeDataTB
                            };
                            codeDataList.Add(codeTableResult);
                        }
                    }
                }
                return Ok(new { Result = "Success", codeDataList });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【searchCodeTable Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【searchCodeTable Fail】");
            }
            
        }

        // GET api/FormCode
        // 線傳代碼 - 線傳各類代碼查詢 - 建立代碼表
        [HttpPost]
        [ActionName("createCodeTable")]
        public async Task<IHttpActionResult> createCodeTable(string tableTitle, string chineseTitle)
        {
            List<int> Formcell = new List<int>(new int[2]);
            List<CreateCodeTbRes> codeList = new List<CreateCodeTbRes>();
            tableTitle = "s_" + tableTitle;

            try
            {
                // Step 1. 檢查有無檔案上傳
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return BadRequest("Invalid request, expecting multipart file upload");
                }

                // 檢查有無同名Table存在
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);
                string getTableSql = @"SELECT * FROM ArmyWeb.sys.tables WHERE name = @tableTitle";
                SqlParameter[] getTableParameter = { new SqlParameter("@tableTitle", SqlDbType.VarChar) { Value = tableTitle } };
                DataTable getTableTB = _dbHelper.ArmyWebExecuteQuery(getTableSql, getTableParameter);
                if (getTableTB == null||getTableTB.Rows.Count != 0) 
                {
                    return Ok(new { Result = "Table Already Exists", codeList });
                }

                // Step 2. 檢查Table是否存在，不存在則創建新Table
                string createTableSql = @"IF NOT EXISTS (SELECT * FROM ArmyWeb.sys.tables WHERE name = @tableTitle)
                                                BEGIN
                                                    CREATE TABLE ArmyWeb.dbo." + tableTitle + @"(code VARCHAR(20), memo NVARCHAR(100))
                                                END";
                SqlParameter[] createTbParameter = { new SqlParameter("@tableTitle", SqlDbType.VarChar) { Value = tableTitle } };
                bool createTable = _dbHelper.ArmyWebUpdate(createTableSql, createTbParameter);

                if (!createTable)
                {
                    return Ok(new {Result = "Fail Create Table", codeList});
                }

                // Step 3.檢查Table Manager是否存在此資料表名稱，不存在則新建 
                string manageSql = @"IF NOT EXISTS (SELECT * FROM ArmyWeb.dbo.code_table_manage WHERE table_name = @tableTitle)
                                        BEGIN
                                            INSERT INTO ArmyWeb.dbo.code_table_manage VALUES (@tableTitle, @chineseTitle, @Type)
                                        END";

                SqlParameter[] manageSqlParameter =
                {
                    new SqlParameter("@tableTitle", SqlDbType.VarChar){Value = tableTitle},
                    new SqlParameter("@chineseTitle", SqlDbType.VarChar){Value = chineseTitle},
                    new SqlParameter("@Type", SqlDbType.VarChar){Value = "new"}
                };

                bool manageResult = _dbHelper.ArmyWebUpdate(manageSql, manageSqlParameter);

                if (!manageResult)
                {
                    return Ok(new { Result = "Fail Insert Table Manage", codeList});
                }

                // Step 4. 取得上傳的文件
                foreach (var file in provider.Contents)
                {
                    //var fileName = file.Headers.ContentDisposition.FileName.Trim('\"');
                    var buffer = await file.ReadAsByteArrayAsync();

                    // 將文件保存到 MemoryStream
                    using (var Excelstream = new MemoryStream(buffer))
                    {

                        List<Dictionary<string, object>> excelData = new List<Dictionary<string, object>>();
                        using (var package = new HSSFWorkbook(Excelstream)) // 假定ERPlusReader接受Stream
                        {
                            var workSheet = package.GetSheetAt(0);
                            var lastRow = workSheet.LastRowNum;
                            var firstRow = workSheet.FirstRowNum;                           
                           
                            var lastCell = workSheet.GetRow(firstRow).LastCellNum;
                            var firstCell = workSheet.GetRow(firstRow).FirstCellNum;

                            // Step 5. 從excel取資料insert到新建的表中
                            for (int row = firstRow + 1; row <= lastRow; row++)
                            {
                                var rowSheet = workSheet.GetRow(row);
                                string insertCodeDataSql = "INSERT INTO ArmyWeb.dbo." + tableTitle + " VALUES (@code, @memo)";
                                int code = firstCell;
                                int memo = firstCell + 1;
                                SqlParameter[] codeDataParameter =  {
                                    new SqlParameter("@code",SqlDbType.VarChar){Value = rowSheet.GetCell(code).ToString()},
                                    new SqlParameter("@memo",SqlDbType.VarChar){Value = rowSheet.GetCell(memo).ToString()}
                                };

                                bool insertCodeDataResult = _dbHelper.ArmyWebUpdate(insertCodeDataSql, codeDataParameter);

                                CreateCodeTbRes createCodeRes = new CreateCodeTbRes
                                {
                                    Result = insertCodeDataResult,
                                    
                                    Code = rowSheet.GetCell(0).ToString()
                                };
                                codeList.Add(createCodeRes);
                            }
                        }
                    }
                }
                return Ok(new { Result = "Success", codeList });
            }
            catch (Exception ex) 
            {
                WriteLog.Log(String.Format("【createCodeTable Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【createCodeTable Fail】");
            }
        }

        // 線傳代碼 - 線傳各類代碼查詢 - 搜尋新代碼表
        [HttpGet]
        [ActionName("searchNewCodeTable")]
        public async Task<IHttpActionResult> searchNewCodeTable()
        {
            try
            {
                string newCodeSql = "SELECT table_ch_name FROM ArmyWeb.dbo.code_table_manage WHERE type = @Type";

                SqlParameter[] newCodeParameter = { new SqlParameter("@Type", SqlDbType.VarChar) { Value = "new" } };

                DataTable newCodeTb = _dbHelper.ArmyWebExecuteQuery(newCodeSql, newCodeParameter);

                if (newCodeTb == null || newCodeTb.Rows.Count == 0)
                {
                    return Ok(new { Result = "No New Table", newCodeTb });
                }

                return Ok(new { Result = "Success", newCodeTb });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【searchNewCodeTable Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【searchNewCodeTable Fail】");
            }
        }

        // 線傳代碼 - 線傳各類代碼查詢 - 刪除代碼表
        [HttpDelete]
        [ActionName("dropCodeTable")]
        public async Task<IHttpActionResult> dropCodeTable(string chineseName)
        {
            try
            {
                // step 1. 查詢中文代碼表名稱對應的table name
                DataTable codeDataTb = new DataTable();
                string tableNameSql = "SELECT table_name FROM ArmyWeb.dbo.code_table_manage WHERE type = @Type AND table_ch_name = @chineseName";
                SqlParameter[] tableNameParameter =
                {
                    new SqlParameter("@Type", SqlDbType.VarChar) { Value = "new" },
                    new SqlParameter("@chineseName", SqlDbType.VarChar) { Value =  chineseName}
                };

                DataTable tableNameTb = _dbHelper.ArmyWebExecuteQuery(tableNameSql, tableNameParameter);

                if (tableNameTb == null || tableNameTb.Rows.Count == 0)
                {
                    return Ok(new { Result = "There is no Table", codeDataTb });
                }


                //step 2. 刪除Table Manage中的資料表名稱
                string delManagerSql = "DELETE ArmyWeb.dbo.code_table_manage WHERE table_name = @tableName AND type = @Type";
                SqlParameter[] delManagerParameter =
                {
                    new SqlParameter("@tableName", SqlDbType.VarChar) { Value =  tableNameTb.Rows[0]["table_name"].ToString()},
                    new SqlParameter("@Type", SqlDbType.VarChar) { Value = "new" },
                };

                bool delManagerResult = _dbHelper.ArmyWebUpdate(delManagerSql, delManagerParameter);

                if (!delManagerResult)
                {
                    return Ok(new { Result = "Delete Manager Fail" });
                }


                //strp 3. 刪除Table
                string dropTbSql = @"IF EXISTS (SELECT * FROM ArmyWeb.sys.tables WHERE name = @tableTitle) 
                                    BEGIN
                                        DROP TABLE ArmyWeb.dbo." + tableNameTb.Rows[0]["table_name"].ToString() +
                                    @" END";
                SqlParameter[] dropTbParameter = { new SqlParameter("@tableTitle", SqlDbType.VarChar) { Value = tableNameTb.Rows[0]["table_name"].ToString() } };

                bool dropTbResult = _dbHelper.ArmyWebUpdate(dropTbSql, dropTbParameter);

                if (!dropTbResult)
                {
                    return Ok(new { Result = "Fail to Drop the Table" });
                }
                return Ok(new { Result = "Success" });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【dropCodeTable Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【dropCodeTable Fail】");
            }
        }

        // 線傳代碼 - 線傳各類代碼查詢 - 取得代碼表資料
        [HttpGet]
        [ActionName("getCodeData")]
        public async Task<IHttpActionResult> searchCodeData(string chineseName)
        {
            try
            {
                // step 1. 查詢中文代碼表名稱對應的table name
                DataTable codeDataTb = new DataTable();
                string tableNameSql = "SELECT table_name FROM ArmyWeb.dbo.code_table_manage WHERE type = @Type AND table_ch_name = @chineseName";
                SqlParameter[] tableNameParameter =
                {
                    new SqlParameter("@Type", SqlDbType.VarChar) { Value = "new" },
                    new SqlParameter("@chineseName", SqlDbType.VarChar) { Value =  chineseName}
                };

                DataTable tableNameTb = _dbHelper.ArmyWebExecuteQuery(tableNameSql, tableNameParameter);

                if(tableNameTb == null || tableNameTb.Rows.Count == 0)
                {
                    return Ok(new { Result = "There is no Table", codeDataTb });
                }

                // step 2. 根據查詢到的table_name取得代碼表資料
                string codeDataSql = @"SELECT code, memo FROM ArmyWeb.dbo." + tableNameTb.Rows[0]["table_name"].ToString();
                //SqlParameter[] codeDataParameter =
                //{
                //    new SqlParameter("@tableTitle", SqlDbType.VarChar) { Value =  tableNameTb.Rows[0]["table_name"].ToString()},
                //};

                codeDataTb = _dbHelper.ArmyWebExecuteQuery(codeDataSql);

                if(codeDataTb == null || codeDataTb.Rows.Count == 0)
                {
                    return Ok(new { Result = "There is no Data in Table", codeDataTb });
                }

            
                return Ok(new { Result = "Success", codeDataTb });

            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【searchCodeData Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【searchCodeData Fail】");
            }
        }

        // 線傳代碼 - 線傳各類代碼查詢 - 編輯代碼表資料
        [HttpPost]
        [ActionName("editCodeData")]
        public async Task<IHttpActionResult> editCodeData([FromBody] EditCodeReq codeData) 
        {
            // step 1. 查詢中文代碼表名稱對應的table name
            string tableNameSql = "SELECT table_name FROM ArmyWeb.dbo.code_table_manage Where type = @Type AND table_ch_name = @chineseName";
            SqlParameter[] tableNameParameter =
            {
                    new SqlParameter("@Type", SqlDbType.VarChar) { Value = "new" },
                    new SqlParameter("@chineseName", SqlDbType.VarChar) { Value =  codeData.ChineseTitle }
                };

            DataTable tableNameTb = _dbHelper.ArmyWebExecuteQuery(tableNameSql, tableNameParameter);

            if (tableNameTb == null || tableNameTb.Rows.Count == 0)
            {
                return Ok(new { Result = "There is no Table" });
            }

            // step 2. 查詢第一筆標題資料
            string titleSql = @"SELECT * FROM  ArmyWeb.dbo." + tableNameTb.Rows[0]["table_name"].ToString();


            DataTable titleTb = _dbHelper.ArmyWebExecuteQuery(titleSql);

            if (titleTb == null || titleTb.Rows.Count == 0)
            {
                return Ok(new { Result = "There is no Title" });
            }

            try
            {
                // step 3. 根據查詢到的table_name刪除代碼表資料
                string delCodeDataSql = @"DELETE FROM
                                        ArmyWeb.dbo." + tableNameTb.Rows[0]["table_name"].ToString();

                bool delResult = _dbHelper.ArmyWebUpdate(delCodeDataSql);               
               

                // step 4. 將標題資料和codeData_List的資料匯入資料表
                string inCodeDataSql = @"INSERT INTO ArmyWeb.dbo." + tableNameTb.Rows[0]["table_name"].ToString() + @"
                                        VALUES ('" + titleTb.Rows[0]["code"].ToString() + "','" + titleTb.Rows[0]["memo"].ToString() + "')";
                for (int i = 0; i < codeData.DataList.Count; i++)
                {                    
                    inCodeDataSql += ", ('" + codeData.DataList[i].Code + "','" + codeData.DataList[i].Memo + "')";
                }
                bool inResult = _dbHelper.ArmyWebUpdate(inCodeDataSql);

                if (!inResult)
                {
                    return Ok(new { Result = "更新失敗" });
                }

                return Ok(new { Result = "Success" });
            }
            catch(Exception ex)
            {
                // step 3. 根據查詢到的table_name刪除代碼表資料
                string delCodeDataSql = @"DELETE FROM
                                        ArmyWeb.dbo." + tableNameTb.Rows[0]["table_name"].ToString();

                bool delResult = _dbHelper.ArmyWebUpdate(delCodeDataSql);

                // step 4. 將標題資料和codeData_List的資料匯入資料表
                string inCodeDataSql = @"INSERT INTO ArmyWeb.dbo." + tableNameTb.Rows[0]["table_name"].ToString() + @"
                                        VALUES ";
                for (int i = 0; i < titleTb.Rows.Count; i++)
                {
                    if (i != 0)
                    {
                        inCodeDataSql += ",";
                    }
                    inCodeDataSql += "('" + titleTb.Rows[i]["code"].ToString() + "','" + titleTb.Rows[i]["memo"].ToString() + "')";
                }
                bool inResult = _dbHelper.ArmyWebUpdate(inCodeDataSql);

                
                WriteLog.Log(String.Format("【editCodeData Fail】" + DateTime.Now.ToString() + " " + ex.Message));               
                return BadRequest("【editCodeData Fail】");
            }
        }

        // [棄用]
        // 線傳代碼 - 線傳各類代碼查詢 - 更新代碼表資料
        [HttpPut]
        [ActionName("updateCodeData")]
        public async Task<IHttpActionResult> updateCodeData([FromBody] UpdateCodeReq codeData)
        {
            try
            {
                // step 1. 查詢中文代碼表名稱對應的table name
                string tableNameSql = "SELECT table_name FROM ArmyWeb.dbo.code_table_manage WHERE type = @Type AND table_ch_name = @chineseName";
                SqlParameter[] tableNameParameter =
                {
                    new SqlParameter("@Type", SqlDbType.VarChar) { Value = "new" },
                    new SqlParameter("@chineseName", SqlDbType.VarChar) { Value =  codeData.ChineseTitle }
                };

                DataTable tableNameTb = _dbHelper.ArmyWebExecuteQuery(tableNameSql, tableNameParameter);

                if (tableNameTb == null || tableNameTb.Rows.Count == 0)
                {
                    return Ok(new { Result = "There is no Table"});
                }

                // step 2. 根據查詢到的table_name取得代碼表資料
                foreach (var updateList in codeData.DataList) 
                {
                    string codeDataSql = @"UPDATE ArmyWeb.dbo." + tableNameTb.Rows[0]["table_name"].ToString() + @" SET code = @Code, memo = @Memo
                                        WHERE code = @OrignCode";
                    SqlParameter[] codeDataParameter =
                    {
                        new SqlParameter("@Code", SqlDbType.VarChar){Value = updateList.Code},
                        new SqlParameter("@Memo", SqlDbType.VarChar){Value = updateList.Memo},
                        new SqlParameter("@OrignCode", SqlDbType.VarChar){Value = updateList.OrignCode}
                    };

                    bool codeDataResult = _dbHelper.ArmyWebUpdate(codeDataSql, codeDataParameter);
                    if (!codeDataResult)
                    {
                        return Ok(new { Result = "Update Fail" });
                    }
                }

                return Ok(new { Result = "Success" });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【updateCodeData Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【updateCodeData Fail】");
            }
        }

        // [棄用]
        // 線傳代碼 - 線傳各類代碼查詢 - 刪除代碼表資料
        [HttpDelete]
        [ActionName("deleteCodeData")]
        public async Task<IHttpActionResult> deleteCodeData(string chineseName, string code)
        {
            try
            {
                // step 1. 查詢中文代碼表名稱對應的table name
                string tableNameSql = "SELECT table_name FROM ArmyWeb.dbo.code_table_manage WHERE type = @Type AND table_ch_name = @chineseName";
                SqlParameter[] tableNameParameter =
                {
                    new SqlParameter("@Type", SqlDbType.VarChar) { Value = "new" },
                    new SqlParameter("@chineseName", SqlDbType.VarChar) { Value =  chineseName }
                };

                DataTable tableNameTb = _dbHelper.ArmyWebExecuteQuery(tableNameSql, tableNameParameter);

                if (tableNameTb == null || tableNameTb.Rows.Count == 0)
                {
                    return Ok(new { Result = "There is no Table" });
                }

                // step 2. 根據查詢到的table_name刪除代碼表資料
                string delCodeSql = @"DELETE ArmyWeb.dbo." + tableNameTb.Rows[0]["table_name"].ToString() + @" WHERE code = @Code";

                SqlParameter[] delCodeParameter =
                {
                    new SqlParameter("@Code",SqlDbType.VarChar){Value = code}
                };

                bool delCodeResult = _dbHelper.ArmyWebUpdate(delCodeSql, delCodeParameter);
                if (!delCodeResult) 
                {
                    return Ok(new { Result = "Delete data Fail" });
                }

                return Ok(new { Result = "Success" });

            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【deleteCodeData Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【deleteCodeData Fail】");
            }
        }
    }
}
