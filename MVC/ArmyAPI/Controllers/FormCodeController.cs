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
                string transCodeSql = "SELECT trans_type, trans_code, memo FROM Army.dbo.memb_trans_code WHERE concat(trans_type, trans_code, memo) like @keyWord";
                SqlParameter[] codeParameters = { new SqlParameter("@keyWord", SqlDbType.VarChar) { Value = (object)newKeyWord ?? DBNull.Value } };
                DataTable tranCodeTb = _dbHelper.ArmyExecuteQuery(transCodeSql, codeParameters);
                if (tranCodeTb == null || tranCodeTb.Rows.Count == 0)
                {
                    return Ok(new { Result = "Trans Code Not Found", transCodeList });
                }

                foreach (DataRow row in tranCodeTb.Rows)
                {
                    string codeTitle = string.Empty;
                    string codeTitleSql = "SELECT trans_title FROM memb_trans_code_data WHERE trans_type = @transType";
                    SqlParameter[] titleParameters = { new SqlParameter("@transType", SqlDbType.VarChar) { Value = row["trans_type"].ToString() } };
                    DataTable codeTitleTb = _dbHelper.ArmyWebExecuteQuery(codeTitleSql, titleParameters);

                    if (codeTitleTb != null || codeTitleTb.Rows.Count >= 0)
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
                return BadRequest("【searchTranscode Fail】" + ex.ToString());
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
                string tableCatalog = "Army";
                string tableManagerSql = "SELECT * FROM code_table_manage";
                DataTable tableManagerTB = _dbHelper.ArmyWebExecuteQuery(tableManagerSql);
                if (tableManagerTB == null || tableManagerTB.Rows.Count == 0)
                {
                    return Ok(new { Result = "Table Not Found" });
                }
                foreach (DataRow row in tableManagerTB.Rows)
                {
                    string codeTableSql = "SELECT COLUMN_NAME FROM Army.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_CATALOG = @tableCatalog AND TABLE_NAME = @tableName";
                    SqlParameter[] codeParameter =
                    {
                    new SqlParameter("@tableCatalog",SqlDbType.VarChar){Value = tableCatalog},
                    new SqlParameter("@tableName",SqlDbType.VarChar){Value= row["table_name"].ToString()}
                };
                    string aaaaaaa = row.ToString();
                    DataTable codeTableTB = _dbHelper.ArmyExecuteQuery(codeTableSql, codeParameter);
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

                        DataTable codeDataTB = _dbHelper.ArmyExecuteQuery(codeDataSql, codeDataSqlParameter);
                        if (codeDataTB == null || codeDataTB.Rows.Count == 0)
                        {
                            CodeTableRes codeTableResult = new CodeTableRes
                            {
                                Result = "No Data",
                                Table = row["table_name"].ToString(),
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
                                Table = row["table_name"].ToString(),
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
                                Table = row["table_name"].ToString(),
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
                return BadRequest("【searchCodeTable Fail】" + ex.ToString());
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
                string getTableSql = @"SELECT * FROM Army.sys.tables WHERE name = @tableTitle";
                SqlParameter[] getTableParameter = { new SqlParameter("@tableTitle", SqlDbType.VarChar) { Value = tableTitle } };
                DataTable getTableTB = _dbHelper.ArmyWebExecuteQuery(getTableSql, getTableParameter);
                if (getTableTB == null||getTableTB.Rows.Count != 0) 
                {
                    return Ok(new { Result = "Table Already Exists", codeList });
                }

                // Step 2. 檢查Table是否存在，不存在則創建新Table
                string createTableSql = @"IF NOT EXISTS (SELECT * FROM Army.sys.tables WHERE name = @tableTitle)
                                                BEGIN
                                                    CREATE TABLE Army.dbo." + tableTitle + @"(code VARCHAR(20), memo VARCHAR(100))
                                                END";
                SqlParameter[] createTbParameter = { new SqlParameter("@tableTitle", SqlDbType.VarChar) { Value = tableTitle } };
                bool createTable = _dbHelper.ArmyUpdate(createTableSql, createTbParameter);

                if (!createTable)
                {
                    return Ok(new {Result = "Fail Create Table", codeList});
                }

                // Step 3.檢查Table Manager是否存在此資料表名稱，不存在則新建 
                string manageSql = @"IF NOT EXISTS (SELECT * FROM code_table_manage WHERE table_name = @tableTitle)
                                        BEGIN
                                            INSERT INTO code_table_manage VALUES (@tableTitle, @chineseTitle, @Type)
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
                            for (int row = firstRow + 1; row < lastRow; row++)
                            {
                                var rowSheet = workSheet.GetRow(row);
                                string insertCodeDataSql = "INSERT INTO Army.dbo." + tableTitle + " VALUES (@code, @memo)";
                                int code = firstCell;
                                int memo = firstCell + 1;
                                SqlParameter[] codeDataParameter =  {
                                    new SqlParameter("@code",SqlDbType.VarChar){Value = rowSheet.GetCell(code).ToString()},
                                    new SqlParameter("@memo",SqlDbType.VarChar){Value = rowSheet.GetCell(memo).ToString()}
                                };

                                bool insertCodeDataResult = _dbHelper.ArmyUpdate(insertCodeDataSql, codeDataParameter);

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
                return BadRequest("【createCodeTable Fail】" + ex.ToString());
            }
        }


        [HttpGet]
        [ActionName("searchNewCodeTable")]
        public async Task<IHttpActionResult> searchNewCodeTable()
        {
            try
            {
                string newCodeSql = "SELECT table_ch_name FROM code_table_manage Where type = @Type";

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
                return BadRequest("【searchNewCodeTable Fail】" + ex.ToString());
            }
        }


        [HttpDelete]
        [ActionName("dropCodeTable")]
        public async Task<IHttpActionResult> dropCodeTable(string chineseName)
        {
            try
            {
                // step 1. 查詢中文代碼表名稱對應的table name
                DataTable codeDataTb = new DataTable();
                string tableNameSql = "SELECT table_name FROM code_table_manage Where type = @Type AND table_ch_name = @chineseName";
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
                string delManagerSql = "DELETE code_table_manage WHERE table_name = @tableName AND type = @Type";
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
                string dropTbSql = @"IF EXISTS (SELECT * FROM Army.sys.tables WHERE name = @tableTitle) 
                                    BEGIN
                                        DROP TABLE Army.dbo." + tableNameTb.Rows[0]["table_name"].ToString() +
                                    @" END";
                SqlParameter[] dropTbParameter = { new SqlParameter("@tableTitle", SqlDbType.VarChar) { Value = tableNameTb.Rows[0]["table_name"].ToString() } };

                bool dropTbResult = _dbHelper.ArmyUpdate(dropTbSql, dropTbParameter);

                if (!dropTbResult)
                {
                    return Ok(new { Result = "Fail to Drop the Table" });
                }
                return Ok(new { Result = "Success" });
            }
            catch (Exception ex)
            {
                return BadRequest("【dropCodeTable Fail】" + ex.ToString());
            }
        }

        [HttpGet]
        [ActionName("getCodeData")]
        public async Task<IHttpActionResult> searchCodeData(string chineseName)
        {
            try
            {
                // step 1. 查詢中文代碼表名稱對應的table name
                DataTable codeDataTb = new DataTable();
                string tableNameSql = "SELECT table_name FROM code_table_manage Where type = @Type AND table_ch_name = @chineseName";
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
                string codeDataSql = @"SELECT code, memo FROM Army.dbo." + tableNameTb.Rows[0]["table_name"].ToString();
                //SqlParameter[] codeDataParameter =
                //{
                //    new SqlParameter("@tableTitle", SqlDbType.VarChar) { Value =  tableNameTb.Rows[0]["table_name"].ToString()},
                //};

                codeDataTb = _dbHelper.ArmyExecuteQuery(codeDataSql);

                if(codeDataTb == null || codeDataTb.Rows.Count == 0)
                {
                    return Ok(new { Result = "There is no Data in Table", codeDataTb });
                }

            
                return Ok(new { Result = "Success", codeDataTb });

            }
            catch (Exception ex)
            {
                return BadRequest("【searchCodeData Fail】" + ex.ToString());
            }
        }


        [HttpPut]
        [ActionName("updateCodeData")]
        public async Task<IHttpActionResult> updateCodeData([FromBody] UpdateCodeReq codeData)
        {
            try
            {
                // step 1. 查詢中文代碼表名稱對應的table name
                string tableNameSql = "SELECT table_name FROM code_table_manage Where type = @Type AND table_ch_name = @chineseName";
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
                    string codeDataSql = @"UPDATE Army.dbo." + tableNameTb.Rows[0]["table_name"].ToString() + @" SET code = @Code, memo = @Memo
                                        WHERE code = @OrignCode";
                    SqlParameter[] codeDataParameter =
                    {
                        new SqlParameter("@Code", SqlDbType.VarChar){Value = updateList.Code},
                        new SqlParameter("@Memo", SqlDbType.VarChar){Value = updateList.Memo},
                        new SqlParameter("@OrignCode", SqlDbType.VarChar){Value = updateList.OrignCode}
                    };

                    bool codeDataResult = _dbHelper.ArmyUpdate(codeDataSql, codeDataParameter);
                    if (!codeDataResult)
                    {
                        return Ok(new { Result = "Update Fail" });
                    }
                }

                return Ok(new { Result = "Success" });
            }
            catch (Exception ex)
            {
                return BadRequest("【updateCodeData Fail】" + ex.ToString());
            }
        }

        [HttpDelete]
        [ActionName("deleteCodeData")]
        public async Task<IHttpActionResult> deleteCodeData(string chineseName, string code)
        {
            try
            {
                // step 1. 查詢中文代碼表名稱對應的table name
                string tableNameSql = "SELECT table_name FROM code_table_manage Where type = @Type AND table_ch_name = @chineseName";
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
                string delCodeSql = @"DELETE Army.dbo." + tableNameTb.Rows[0]["table_name"].ToString() + @" WHERE code = @Code";

                SqlParameter[] delCodeParameter =
                {
                    new SqlParameter("@Code",SqlDbType.VarChar){Value = code}
                };

                bool delCodeResult = _dbHelper.ArmyUpdate(delCodeSql, delCodeParameter);
                if (!delCodeResult) 
                {
                    return Ok(new { Result = "Delete data Fail" });
                }

                return Ok(new { Result = "Success" });

            }
            catch (Exception ex)
            {
                return BadRequest("【deleteCodeData Fail】" + ex.ToString());
            }
        }
    }
}
