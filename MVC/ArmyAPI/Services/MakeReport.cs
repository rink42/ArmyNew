using ArmyAPI.Models;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using static log4net.Appender.RollingFileAppender;
using NPOI.XWPF.UserModel;
using Microsoft.Ajax.Utilities;


namespace ArmyAPI.Services
{
    public class MakeReport
    {
        private readonly DbHelper _dbHelper;

        public MakeReport()
        {
            _dbHelper = new DbHelper();
        }

        public bool exportFirstToExcel(List<CaseExcelReq> caseData, string excelPath)
        {
            try
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    // 創建 Excel 工作表
                    ExcelWorksheet ws1 = pck.Workbook.Worksheets.Add("任官令");
                    ExcelWorksheet ws2 = pck.Workbook.Worksheets.Add("分發名冊");
                    // 從第一列開始寫入標題
                    ws1.Cells[1, 1].Value = "編號";
                    ws1.Cells[1, 2].Value = "一級單位";
                    ws1.Cells[1, 3].Value = "現職單位";
                    ws1.Cells[1, 4].Value = "姓名";
                    ws1.Cells[1, 5].Value = "兵籍號碼";
                    ws1.Cells[1, 6].Value = "軍種";
                    ws1.Cells[1, 7].Value = "官科";
                    ws1.Cells[1, 8].Value = "初任官階";
                    ws1.Cells[1, 9].Value = "年";
                    ws1.Cells[1, 10].Value = "月";
                    ws1.Cells[1, 11].Value = "日";
                    ws1.Cells[1, 12].Value = caseData[0].CaseId;

                    ws2.Cells["A1:J1"].Merge = true; // 假設您希望大標題跨越 A1 到 D1
                    ws2.Cells["A1"].Value = "國防部陸軍司令部" + caseData[0].Year + "年士官任官令分發名冊";
                    ws2.Cells["A1"].Style.Font.Size = 48; // 設定字體大小
                    ws2.Cells["A1"].Style.Font.Bold = true; // 設定粗體
                    ws2.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // 設定水平置中
                    ws2.Cells[2, 1].Value = "編號";
                    ws2.Cells[2, 2].Value = "一級單位";
                    ws2.Cells[2, 3].Value = "現職單位";
                    ws2.Cells[2, 4].Value = "姓名";
                    ws2.Cells[2, 5].Value = "兵籍號碼";
                    ws2.Cells[2, 6].Value = "軍種";
                    ws2.Cells[2, 7].Value = "官科";
                    ws2.Cells[2, 8].Value = "初任官階";
                    ws2.Cells[2, 9].Value = "生效日期";
                    ws2.Cells[2, 10].Value = caseData[0].CaseId;

                    // 從第二行開始寫入資料
                    int ws1RowIndex = 2;
                    int ws2RowIndex = 3;
                    int Number = 1;
                    foreach (var item in caseData)
                    {
                        ws1.Cells[ws1RowIndex, 1].Value = Number;
                        ws1.Cells[ws1RowIndex, 2].Value = item.PrimaryUnit;
                        ws1.Cells[ws1RowIndex, 3].Value = item.CurrentPosition;
                        ws1.Cells[ws1RowIndex, 4].Value = item.Name;
                        ws1.Cells[ws1RowIndex, 5].Value = item.IdNumber;
                        ws1.Cells[ws1RowIndex, 6].Value = item.Branch;
                        ws1.Cells[ws1RowIndex, 7].Value = item.Rank;
                        ws1.Cells[ws1RowIndex, 8].Value = item.NewRankCode;
                        ws1.Cells[ws1RowIndex, 9].Value = item.Year;
                        ws1.Cells[ws1RowIndex, 10].Value = item.Month;
                        ws1.Cells[ws1RowIndex, 11].Value = item.Day;
                        ws1.Cells[ws1RowIndex, 12].Value = item.CaseId;

                        ws2.Cells[ws2RowIndex, 1].Value = Number;
                        ws2.Cells[ws2RowIndex, 2].Value = item.PrimaryUnit;
                        ws2.Cells[ws2RowIndex, 3].Value = item.CurrentPosition;
                        ws2.Cells[ws2RowIndex, 4].Value = item.Name;
                        ws2.Cells[ws2RowIndex, 5].Value = item.IdNumber;
                        ws2.Cells[ws2RowIndex, 6].Value = item.Branch;
                        ws2.Cells[ws2RowIndex, 7].Value = item.Rank;
                        ws2.Cells[ws2RowIndex, 8].Value = item.NewRankCode;
                        ws2.Cells[ws2RowIndex, 9].Value = item.EffectDate;
                        ws2.Cells[ws2RowIndex, 10].Value = item.CaseId;

                        Number++;
                        ws1RowIndex++;
                        ws2RowIndex++;
                    }
                    //"A1:J1"
                    string finalRowCol = "A" + ws2RowIndex + ":J" + ws2RowIndex;
                    string finalRow = "A" + ws2RowIndex;
                    ws2.Cells[finalRowCol].Merge = true;
                    ws2.Cells[finalRow].Value = "共計" + caseData.Count + "員";
                    ws2.Cells[finalRow].Style.Font.Size = 26; // 設定字體大小
                    ws2.Cells[finalRow].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // 設定水平置中

                    // 寫入 Excel 檔案
                    System.IO.File.WriteAllBytes(excelPath, pck.GetAsByteArray());
                }
                return true;
            }
            catch (Exception ex)
            {
                //throw new Exception(String.Format("exportFirstToExcel Error. {0}", ex.ToString()));
                Console.WriteLine(ex.ToString());
                return false;
            }
            
        }


        public bool exportPromotionToExcel(List<CaseExcelReq> caseData, string excelPath)
        {
            try
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    // 創建 Excel 工作表
                    ExcelWorksheet ws1 = pck.Workbook.Worksheets.Add("任官令");
                    ExcelWorksheet ws2 = pck.Workbook.Worksheets.Add("分發名冊");

                    // 從第一列開始寫入標題
                    ws1.Cells[1, 1].Value = "編號";
                    ws1.Cells[1, 2].Value = "一級單位";
                    ws1.Cells[1, 3].Value = "現職單位";
                    ws1.Cells[1, 4].Value = "姓名";
                    ws1.Cells[1, 5].Value = "兵籍號碼";
                    ws1.Cells[1, 6].Value = "軍種";
                    ws1.Cells[1, 7].Value = "官科";
                    ws1.Cells[1, 8].Value = "原官階";
                    ws1.Cells[1, 9].Value = "晉任官皆";
                    ws1.Cells[1, 10].Value = "年";
                    ws1.Cells[1, 11].Value = "月";
                    ws1.Cells[1, 12].Value = "日";
                    ws1.Cells[1, 13].Value = caseData[0].CaseId;

                    ws2.Cells["A1:K1"].Merge = true; // 大標題跨越 A1 到 K1
                    ws2.Cells["A1"].Value = "國防部陸軍司令部" + caseData[0].Year + "年士官任官令分發名冊";
                    ws2.Cells["A1"].Style.Font.Size = 48; // 設定字體大小
                    ws2.Cells["A1"].Style.Font.Bold = true; // 設定粗體
                    ws2.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // 設定水平置中
                    ws2.Cells[2, 1].Value = "編號";
                    ws2.Cells[2, 2].Value = "一級單位";
                    ws2.Cells[2, 3].Value = "現職單位";
                    ws2.Cells[2, 4].Value = "姓名";
                    ws2.Cells[2, 5].Value = "兵籍號碼";
                    ws2.Cells[2, 6].Value = "軍種";
                    ws2.Cells[2, 7].Value = "官科";
                    ws2.Cells[2, 8].Value = "原官階";
                    ws2.Cells[2, 9].Value = "晉任官皆";
                    ws2.Cells[2, 10].Value = "生效日期";
                    ws2.Cells[2, 11].Value = caseData[0].CaseId;

                    // 從第二行開始寫入資料
                    int ws1RowIndex = 2;
                    int ws2RowIndex = 3;
                    int Number = 1;
                    foreach (var item in caseData)
                    {
                        ws1.Cells[ws1RowIndex, 1].Value = Number;
                        ws1.Cells[ws1RowIndex, 2].Value = item.PrimaryUnit;
                        ws1.Cells[ws1RowIndex, 3].Value = item.CurrentPosition;
                        ws1.Cells[ws1RowIndex, 4].Value = item.Name;
                        ws1.Cells[ws1RowIndex, 5].Value = item.IdNumber;
                        ws1.Cells[ws1RowIndex, 6].Value = item.Branch;
                        ws1.Cells[ws1RowIndex, 7].Value = item.Rank;
                        ws1.Cells[ws1RowIndex, 8].Value = item.OldRankCode;
                        ws1.Cells[ws1RowIndex, 9].Value = item.NewRankCode;
                        ws1.Cells[ws1RowIndex, 10].Value = item.Year;
                        ws1.Cells[ws1RowIndex, 11].Value = item.Month;
                        ws1.Cells[ws1RowIndex, 12].Value = item.Day;
                        ws1.Cells[ws1RowIndex, 13].Value = item.CaseId;

                        ws2.Cells[ws2RowIndex, 1].Value = Number;
                        ws2.Cells[ws2RowIndex, 2].Value = item.PrimaryUnit;
                        ws2.Cells[ws2RowIndex, 3].Value = item.CurrentPosition;
                        ws2.Cells[ws2RowIndex, 4].Value = item.Name;
                        ws2.Cells[ws2RowIndex, 5].Value = item.IdNumber;
                        ws2.Cells[ws2RowIndex, 6].Value = item.Branch;
                        ws2.Cells[ws2RowIndex, 7].Value = item.Rank;
                        ws2.Cells[ws2RowIndex, 8].Value = item.OldRankCode;
                        ws2.Cells[ws2RowIndex, 9].Value = item.NewRankCode;
                        ws2.Cells[ws2RowIndex, 10].Value = item.EffectDate;
                        ws2.Cells[ws2RowIndex, 11].Value = item.CaseId;

                        Number++;
                        ws1RowIndex++;
                        ws2RowIndex++;
                    }
                    //"A1:J1"
                    string finalRowCol = "A" + ws2RowIndex + ":K" + ws2RowIndex;
                    string finalRow = "A" + ws2RowIndex;
                    ws2.Cells[finalRowCol].Merge = true;
                    ws2.Cells[finalRow].Value = "共計" + caseData.Count + "員";
                    ws2.Cells[finalRow].Style.Font.Size = 26; // 設定字體大小
                    ws2.Cells[finalRow].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // 設定水平置中

                    // 寫入 Excel 檔案
                    System.IO.File.WriteAllBytes(excelPath, pck.GetAsByteArray());
                }
                return true;
            }
            catch (Exception ex)
            {
                //throw new Exception(String.Format("exportPromotionToExcel Error. {0}", ex.ToString()));
                Console.WriteLine(ex.ToString());
                return false;
            }
            
        }


        public bool exportFirstToPDF(DataTable caseDataTb, string outputPath, string caseName)
        {
            if(caseDataTb == null || caseDataTb.Rows.Count == 0)
            {
                return false;
            }          

            //設置西元轉民國
            CultureInfo Tocalendar = new CultureInfo("zh-TW");
            Tocalendar.DateTimeFormat.Calendar = new TaiwanCalendar();
            
            //設置編輯的rpt
            ReportDocument rd = new ReportDocument();
            string reportPath = System.Web.Hosting.HostingEnvironment.MapPath("~/CrystalReport4.rpt");
            rd.Load(reportPath);

            string signatureSql = "SELECT * FROM signature";
            DataTable signatureTb = _dbHelper.ArmyWebExecuteQuery(signatureSql);
            if(signatureTb == null || signatureTb.Rows.Count == 0)
            {
                return false;
            }
            string createYear = DateTime.Now.ToString("yyy", Tocalendar);
            string createMonth = DateTime.Now.ToString("MM", Tocalendar);
            string createDay = DateTime.Now.ToString("dd", Tocalendar);

            //取得CR中Text1,3物件並設定Text屬性
            ((TextObject)rd.ReportDefinition.ReportObjects["Text9"]).Text = "中華民國" + createYear + "年" + createMonth + "月" + createDay + "日";
            ((TextObject)rd.ReportDefinition.ReportObjects["Text8"]).Text = caseName;
            //((TextObject)rd.ReportDefinition.ReportObjects["Text13"]).Text = signatureTb.Rows[0]["rank_title"].ToString();
            ((TextObject)rd.ReportDefinition.ReportObjects["Text16"]).Text = signatureTb.Rows[0]["name"].ToString();
            rd.SetDataSource(caseDataTb);

            try
            {
                // 匯出 Crystal Report 為 PDF                
                ExportOptions options = new ExportOptions();
                DiskFileDestinationOptions diskFileDestinationOptions = new DiskFileDestinationOptions();
                diskFileDestinationOptions.DiskFileName = outputPath;

                options.ExportDestinationType = ExportDestinationType.DiskFile;
                options.ExportFormatType = ExportFormatType.PortableDocFormat;
                options.ExportDestinationOptions = diskFileDestinationOptions;

                rd.Export(options);                
            }
            catch (Exception ex)
            {
                //throw new Exception(String.Format("exportFirstToPDF Error. {0}", ex.ToString()));
                Console.WriteLine(ex.ToString());
                return false;
            }
            finally 
            {
                rd.Close();
            }

            return true;
        }

        public bool exportPromotionToPDF(DataTable caseDataTb, string outputPath, string caseName)
        {
            if (caseDataTb == null || caseDataTb.Rows.Count == 0)
            {
                return false;
            }

            //設置西元轉民國
            CultureInfo Tocalendar = new CultureInfo("zh-TW");
            Tocalendar.DateTimeFormat.Calendar = new TaiwanCalendar();

            //設置編輯的rpt
            ReportDocument rd = new ReportDocument();
            string reportPath = System.Web.Hosting.HostingEnvironment.MapPath("~/CrystalReport3.rpt");
            rd.Load(reportPath);

            string signatureSql = "SELECT * FROM signature";
            DataTable signatureTb = _dbHelper.ArmyWebExecuteQuery(signatureSql);
            if (signatureTb == null || signatureTb.Rows.Count == 0)
            {
                return false;
            }

            string createYear = DateTime.Now.ToString("yyy", Tocalendar);
            string createMonth = DateTime.Now.ToString("MM", Tocalendar);
            string createDay = DateTime.Now.ToString("dd", Tocalendar);

            //取得CR中Text1,3物件並設定Text屬性
            ((TextObject)rd.ReportDefinition.ReportObjects["Text9"]).Text = "中華民國" + createYear + "年" + createMonth + "月" + createDay + "日";
            ((TextObject)rd.ReportDefinition.ReportObjects["Text8"]).Text = caseName;
            //((TextObject)rd.ReportDefinition.ReportObjects["Text13"]).Text = signatureTb.Rows[0]["rank_title"].ToString();
            ((TextObject)rd.ReportDefinition.ReportObjects["Text16"]).Text = signatureTb.Rows[0]["name"].ToString();
            rd.SetDataSource(caseDataTb);

            try
            {
                // 匯出 Crystal Report 為 PDF
                ExportOptions options = new ExportOptions();
                DiskFileDestinationOptions diskFileDestinationOptions = new DiskFileDestinationOptions();
                diskFileDestinationOptions.DiskFileName = outputPath;

                options.ExportDestinationType = ExportDestinationType.DiskFile;
                options.ExportFormatType = ExportFormatType.PortableDocFormat;
                options.ExportDestinationOptions = diskFileDestinationOptions;

                rd.Export(options);

                // 任官令官印  exportFirstToPDF(DataTable caseDataTb, string outputPath)
            }
            catch (Exception ex)
            {
                //throw new Exception(String.Format("exportPromotionToPDF Error. {0}", ex.ToString()));
                Console.WriteLine(ex.ToString());
                return false;
            }
            finally
            {
                rd.Close();
            }

            return true;
        }


        public bool exportMultinumberExcel(List<List<string>> caseData, string excelPath)
        {
            try
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    // 創建 Excel 工作表
                    ExcelWorksheet ws1 = pck.Workbook.Worksheets.Add("多兵號查詢");
                    string[] columnName = 
                    { 
                        "身分證字號", "姓名", "單位代號", "編外因素", "項次", "行次", "序號", "編制專長前置代碼", 
                        "編制專長代碼", "編階", "職稱", "任本職日期", "軍種", "官科", "役別代碼", "階級", "薪級", 
                        "回役晉支月", "任本階日期", "本人主專長前置代碼", "本人主專長代碼", "支薪單位", "支薪標註", 
                        "專業給付", "正副主官給付註記", "任職狀況", "支領原待遇", "四角號碼", "異動日期", "異動代號", 
                        "入伍梯次", "轉服志願士兵日期", "轉服志願士官日期", "轉服志願軍官日期", "再入營日期", 
                        "廢止志願役日期" 
                    };
                    // 從第一列開始寫入標題
                    int count = 1;
                    foreach(string column in columnName)
                    {
                        ws1.Cells[1,count].Value = column;
                        count++;
                    }

                    // 從第二行開始寫入資料
                    int ws1RowIndex = 2;
                    foreach (List<string> item in caseData)
                    {
                        for(int i = 1; i <= columnName.Count(); i++)
                        {
                            ws1.Cells[ws1RowIndex, i].Value = item[i-1];                            
                        }
                        ws1RowIndex++;
                    }

                    // 寫入 Excel 檔案
                    System.IO.File.WriteAllBytes(excelPath, pck.GetAsByteArray());
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("exportFirstToExcel Error. {0}", ex.ToString()));
                //return false;
            }
        }

        public bool exportYearbookExcel(List<List<string>> caseData, string excelPath, string formType)
        {
            try
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    // 創建 Excel 工作表
                    ExcelWorksheet ws1 = pck.Workbook.Worksheets.Add("年籍冊查詢");
                    string[] columnName;
                    switch (formType)
                    {
                        case "N":
                            columnName = new string[]
                            {
                                "單位代號", "項次", "編階", "階級", "軍種", "編專", "職稱", "兵籍代號",
                                "姓名", "性別", "編制號", "俸級", "現員官科", "編制官科", "現員主專", "最高軍事學資", "軍校名稱", "民間學資",
                                "本階日", "本職日", "任官日", "生日", "四角號碼", "役別", "基礎軍事學資", "第一年考績", "第二年考績",
                                "第三年考績", "第四年考績", "第五年考績"
                            };
                            break;
                        case "R":
                            columnName = new string[]
                            {
                                "編階", "階級", "單位代號", "編專", "職稱", "兵籍代號",
                                "姓名", "性別", "編制號", "現員官科",
                                "本階日", "本職日", "任官日", "生日", "四角號碼", "役別", "退伍日"
                            };
                            break;
                        default:
                            return false;
                    }

                    // 從第一列開始寫入標題
                    int count = 1;
                    foreach (string column in columnName)
                    {
                        ws1.Cells[1, count].Value = column;
                        count++;
                    }

                    // 從第二行開始寫入資料
                    int ws1RowIndex = 2;
                    foreach (List<string> item in caseData)
                    {
                        for (int i = 1; i <= columnName.Count(); i++)
                        {
                            ws1.Cells[ws1RowIndex, i].Value = item[i - 1];
                        }
                        ws1RowIndex++;
                    }

                    // 寫入 Excel 檔案
                    System.IO.File.WriteAllBytes(excelPath, pck.GetAsByteArray());
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("exportFirstToExcel Error. {0}", ex.ToString()));
                //return false;
            }

        }

        public bool exportAdvSearchExcel(advExcelDataReq excelData, string excelPath)
        {
            try
            {
				ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

				using (ExcelPackage pck = new ExcelPackage())
                {
                    ExcelWorksheet ws1 = pck.Workbook.Worksheets.Add("進階查詢");
                    List<string> columnName = excelData.ColumnName;

                    int count = 1;
                    foreach (string column in columnName)
                    {
                        ws1.Cells[1, count].Value = column;
                        count++;
                    }

                    int ws1RowIndex = 2;
                    foreach (List<string> item in excelData.Data)
                    {
                        for (int i = 1; i <= columnName.Count(); i++)
                        {
                            ws1.Cells[ws1RowIndex, i].Value = item[i - 1];
                        }
                        ws1RowIndex++;
                    }

                    // 寫入 Excel 檔案
                    System.IO.File.WriteAllBytes(excelPath, pck.GetAsByteArray());
                }
                return true;
            }           
            catch (Exception ex)
            {
                throw new Exception(String.Format("exportFirstToExcel Error. {0}", ex.ToString()));
                //return false;
            }
        }

        public List<string> txtReadLines(MemoryStream stream)
        {
            var lines = new List<string>();
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }
            return lines;
        }

        public List<string> excelReadLines(MemoryStream stream)
        {
            var lines = new List<string>();
            using (var package = new ExcelPackage(stream)) // 假定ERPlusReader接受Stream
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;
                
                for (int row = 1; row <= rowCount; row++)
                {
                    lines.Add(worksheet.Cells[row, 1].Text);
                }
            }
            return lines;
        }


        public void checkGeneral(List<GeneralReq> memberRank, string userId, string fileName, string reportName)
        {
            try
            {
                bool generalDownload = false;
                string dateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                foreach (GeneralReq member in memberRank)
                {
                    int rank = int.Parse(member.GeneralRank);
                    if (rank > 0 && rank <= 23) //rank_code 1~23 為將官階級
                    {
                        // 需要額外新增一筆記錄
                        generalDownload = true;

                        // 記錄包含將官的名冊和下載人
                        WriteLog.Log(String.Format("[YearBook Download Remind] GeneralId:{0}, MemberId:{1}, File:{2}, DownloadDate:{3}", member.GeneralId, userId, fileName, dateTime));
                        string remindSql = @"INSERT INTO 
                                                ArmyWeb.dbo.report_remind      
                                            VALUES(
                                                @reportName,
                                                @generalId,
                                                @generalName,
                                                @generalRankCode,
                                                @downloadMemberId,
                                                @fileName,
                                                @downloadDate)";

                        SqlParameter[] remindPar =
                        {
                            new SqlParameter("@reportName", reportName),
                            new SqlParameter("@generalId", member.GeneralId),
                            new SqlParameter("@generalName", member.GeneralName),
                            new SqlParameter("@generalRankCode", member.GeneralRank),
                            new SqlParameter("@downloadMemberId", userId),
                            new SqlParameter("@fileName", fileName),
                            new SqlParameter("@downloadDate", dateTime)
                        };
                        bool remind = _dbHelper.ArmyWebUpdate(remindSql, remindPar);
                    }
                }

                // 新增一筆下載記錄
                string recordSql = @"INSERT INTO 
                                    ArmyWeb.dbo.report_record
                                VALUES(
                                    @memberId,
                                    @memberAction,
                                    @actionDate)";
                SqlParameter[] recordPar =
                {
                    new SqlParameter("@memberId", userId),
                    new SqlParameter("@memberAction", reportName),
                    new SqlParameter("@actionDate", dateTime)
                };

                bool record = _dbHelper.ArmyWebUpdate(recordSql, recordPar);

                //若下載名冊有包含將官人員，則額外新增一筆記錄
                if (generalDownload) 
                {
                    string generalSql = @"INSERT INTO 
                                    ArmyWeb.dbo.report_record
                                VALUES(
                                    @memberId,
                                    @memberAction,
                                    @actionDate)";

                    string newRecordName = "將官" + reportName;
                    SqlParameter[] generalPara =
                    {
                        new SqlParameter("@memberId", userId),
                        new SqlParameter("@memberAction", newRecordName),
                        new SqlParameter("@actionDate", dateTime)
                    };

                    bool general = _dbHelper.ArmyWebUpdate(generalSql, generalPara);
                }

            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("[YearBook Download Write Log Fail] Error. {0}", ex.ToString()));
                throw new Exception(String.Format("[YearBook Download Write Log Fail] Error. {0}", ex.ToString()));
            }

        }
    }
}