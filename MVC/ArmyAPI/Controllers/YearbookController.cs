using ArmyAPI.Services;
using ArmyAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using OfficeOpenXml;
using System.IO;
using System.Net.Http;
using System.Configuration;
using ArmyAPI.Commons;
using Org.BouncyCastle.Bcpg;

namespace ArmyAPI.Controllers
{
    public class YearbookController : ApiController
    {
        private readonly DbHelper _dbHelper;        
        private readonly MakeReport _makeReport;
        private readonly CodetoName _codeToName;
        

        public YearbookController()
        {
            _dbHelper = new DbHelper();            
            _makeReport = new MakeReport();
            _codeToName = new CodetoName();            
        }

        // Post api/Yearbook
        // 現員年籍冊查詢(手動輸入)
        [HttpPost]
        [ActionName("YearbookSearch")]
        public async Task<IHttpActionResult> YearbookSearch([FromBody] List<string> idNumber)
        {
            try
            {
                var yearBookList = new List<object>();

                // Step 1. 身份證字號的驗證
                List<string> wrongId = new List<string>();
                bool wrongReq = true;
                foreach (string userId in idNumber)
                {
                    string msg = "";
                    var result = (new Class_TaiwanID()).Check(userId, out msg);
                    if (!result)
                    {
                        wrongId.Add(userId);
                        wrongReq = false;
                    }
                }

                if (!wrongReq)
                {
                    return Ok(new { Result = "Wrong Member Id", WrongId = wrongId, yearBookList });
                }
                

                // Step 2. 年籍冊查詢
                string getMemberSql = $@"SELECT  
                    v_member_data.es_rank_code,
                    v_member_data.rank_code,
                    v_member_data.service_code, 
                    v_member_data.unit_code, 
                    v_member_data.item_no, 
                    v_member_data.es_skill_code, 
                    v_member_data.title_code, 
                    v_member_data.member_id, 
                    v_member_data.member_name, 
                    v_member_data.supply_rank, 
                    v_member_data.group_code, 
                    v_member_data.m_skill_code, 
                    v_member_data.military_educ_code, 
                    v_member_data.school_code,
                    v_member_data.rank_date, 
                    v_member_data.pay_date, 
                    v_member_data.salary_date, 
                    v_member_data.birthday, 
                    v_member_data.corner_code, 
                    v_member_data.campaign_code,
                    CASE 
                        WHEN SUBSTRING(v_member_data.member_id, 2, 1) = '1' THEN '男'
                        WHEN SUBSTRING(v_member_data.member_id, 2, 1) = '2' THEN '女'
                    END AS 性別,
                    REPLACE(vepj.item_no + '' + vepj.column_code + '' + t1.group_code + '' + vepj.serial_code, ' ', '') AS 編制號,
                    REPLACE(t2.group_code + '' + t2.group_title, ' ', '') AS 編制官科,
                    REPLACE(es.school_code + '' + es.school_desc, ' ', '') AS 軍校名稱,
                    vec.class_name AS 基礎軍事學資,
                    pc1.perform_name AS N_1年考績,
                    pc2.perform_name AS N_2年考績,
                    pc3.perform_name AS N_3年考績,
                    pc4.perform_name AS N_4年考績,
                    pc5.perform_name AS N_5年考績
                FROM Army.dbo.v_member_data
                LEFT JOIN Army.dbo.v_es_person_join AS vepj ON vepj.member_id = v_member_data.member_id
                LEFT JOIN Army.dbo.tgroup AS t1 ON t1.group_code = vepj.group_code -- 編官科
                LEFT JOIN Army.dbo.tgroup AS t2 ON t2.group_code = vepj.member_group -- 現官科
                LEFT JOIN Army.dbo.educ_school AS es ON es.school_code = vepj.educ_school -- 學校
                LEFT JOIN (
                    SELECT member_id, class_name
                    FROM Army.dbo.v_education
                    LEFT JOIN Army.dbo.educ_class AS ecl ON ecl.class_code = v_education.class_code
                    WHERE v_education.educ_code = 'H'
                ) AS vec ON vec.member_id = v_member_data.member_id
                LEFT JOIN Army.dbo.v_performance AS vp1 ON vp1.member_id = v_member_data.member_id AND vp1.p_year = YEAR(GETDATE()) - 1911 - 1
                LEFT JOIN Army.dbo.perf_code AS pc1 ON pc1.perform_code = vp1.perform_code
                LEFT JOIN Army.dbo.v_performance AS vp2 ON vp2.member_id = v_member_data.member_id AND vp2.p_year = YEAR(GETDATE()) - 1911 - 2
                LEFT JOIN Army.dbo.perf_code AS pc2 ON pc2.perform_code = vp2.perform_code
                LEFT JOIN Army.dbo.v_performance AS vp3 ON vp3.member_id = v_member_data.member_id AND vp3.p_year = YEAR(GETDATE()) - 1911 - 3
                LEFT JOIN Army.dbo.perf_code AS pc3 ON pc3.perform_code = vp3.perform_code
                LEFT JOIN Army.dbo.v_performance AS vp4 ON vp4.member_id = v_member_data.member_id AND vp4.p_year = YEAR(GETDATE()) - 1911 - 4
                LEFT JOIN Army.dbo.perf_code AS pc4 ON pc4.perform_code = vp4.perform_code
                LEFT JOIN Army.dbo.v_performance AS vp5 ON vp5.member_id = v_member_data.member_id AND vp5.p_year = YEAR(GETDATE()) - 1911 - 5
                LEFT JOIN Army.dbo.perf_code AS pc5 ON pc5.perform_code = vp5.perform_code
                WHERE v_member_data.member_id IN ({string.Join(",", idNumber.Select(id => $"'{id}'"))})
                ORDER BY CASE";

                // 照使用者輸入的順序排序
                int SortingWeight = 1;
                foreach (string memberId in idNumber)
                {
                    getMemberSql += " WHEN v_member_data.member_id = '" + memberId + "' THEN " + SortingWeight;
                    SortingWeight++;
                }
                getMemberSql += @" ELSE 999
                                  END;";

                DataTable getMemberTb = _dbHelper.ArmyWebExecuteQuery(getMemberSql);

                if (getMemberTb == null || getMemberTb.Rows.Count == 0)
                {
                    return Ok(new { Result = "No Member", WrongId = wrongId, yearBookList });
                }

                
                // Step 3. 每筆資料做民國年和代碼的轉換
                foreach (DataRow row in getMemberTb.Rows)
                {
                    string rank_date = _codeToName.dateTimeTran(row["rank_date"].ToString(), "yyy年MM月dd日", true);
                    string pay_date = _codeToName.dateTimeTran(row["pay_date"].ToString(), "yyy年MM月dd日", true);
                    string salary_date = _codeToName.dateTimeTran(row["salary_date"].ToString(), "yyy年MM月dd日", true);
                    string birthday = _codeToName.dateTimeTran(row["birthday"].ToString(), "yyy年MM月dd日", true);

                    row["item_no"] = _codeToName.itemName(row["item_no"].ToString(), row["unit_code"].ToString());
                    row["unit_code"] = _codeToName.unitName(row["unit_code"].ToString());
                    row["es_rank_code"] = _codeToName.rankName(row["es_rank_code"].ToString());
                    row["rank_code"] = _codeToName.rankName(row["rank_code"].ToString());
                    row["service_code"] = _codeToName.serviceName(row["service_code"].ToString());
                    row["es_skill_code"] = _codeToName.skillName(row["es_skill_code"].ToString());
                    row["title_code"] = _codeToName.titleName(row["title_code"].ToString());
                    row["group_code"] = _codeToName.groupName(row["group_code"].ToString());
                    row["m_skill_code"] = _codeToName.skillName(row["m_skill_code"].ToString());
                    row["military_educ_code"] = _codeToName.educName(row["military_educ_code"].ToString());
                    row["school_code"] = _codeToName.schoolName(row["school_code"].ToString());
                    row["campaign_code"] = _codeToName.campaignName(row["campaign_code"].ToString());

                    // Step 4. 填充回傳給前端的資料
                    var memberData = new
                    {                        
                        EsRankCode = row["es_rank_code"].ToString(),
                        RankCode = row["rank_code"].ToString(),
                        ServiceCode = row["service_code"].ToString(),
                        UnitCode = row["unit_code"].ToString(),
                        ItemNo = row["item_no"].ToString(),
                        EsSkillCode = row["es_skill_code"].ToString(),
                        TitleCode = row["title_code"].ToString(),
                        MemberId = row["member_id"].ToString(),
                        MemberName = row["member_name"].ToString(),
                        Gender = row["性別"].ToString(),
                        EstablishmentNumber = row["編制號"].ToString(),
                        SupplyRank = row["supply_rank"].ToString(),
                        GroupCode = row["group_code"].ToString(),
                        EstablishmentOfficial = row["編制官科"].ToString(),
                        MSkillCode = row["m_skill_code"].ToString(),
                        MilitaryEducCode = row["military_educ_code"].ToString(),
                        MilitarySchoolName = row["軍校名稱"].ToString(),
                        SchoolCode = row["school_code"].ToString(),
                        RankDate = rank_date,
                        PayDate = pay_date,
                        SalaryDate = salary_date,
                        Birthday = birthday,
                        CornerCode = row["corner_code"].ToString(),
                        CampaignCode = row["campaign_code"].ToString(),
                        BasicMilitaryEducation = row["基礎軍事學資"].ToString(),
                        PerformanceAppraisal1 = row["N_1年考績"].ToString(),
                        PerformanceAppraisal2 = row["N_2年考績"].ToString(),
                        PerformanceAppraisal3 = row["N_3年考績"].ToString(),
                        PerformanceAppraisal4 = row["N_4年考績"].ToString(),
                        PerformanceAppraisal5 = row["N_5年考績"].ToString()
                    };

                    yearBookList.Add(memberData);
                }

                return Ok(new { Result = "Success", WrongId = wrongId, yearBookList });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【YearbookSearch Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【YearbookSearch Fail】");
            }
        }

        // 現員年籍冊匯出
        [HttpPost]
        [ActionName("YearbookExport")]
        public async Task<IHttpActionResult> YearbookExport([FromBody] IdNumberReq selData)
        {
            try
            {
                List<List<string>> excelData = new List<List<string>>();
                List<GeneralReq> generalReq = new List<GeneralReq>();
                // 根據提供的欄位構建SQL語句
                string getMemberSql = $@"SELECT                     
                    v_member_data.es_rank_code, 
                    v_member_data.rank_code, 
                    v_member_data.service_code, 
                    v_member_data.unit_code, 
                    v_member_data.item_no, 
                    v_member_data.es_skill_code, 
                    v_member_data.title_code, 
                    v_member_data.member_id, 
                    v_member_data.member_name, 
                    v_member_data.supply_rank, 
                    v_member_data.group_code, 
                    v_member_data.m_skill_code, 
                    v_member_data.military_educ_code, 
                    v_member_data.school_code,
                    v_member_data.rank_date, 
                    v_member_data.pay_date, 
                    v_member_data.salary_date, 
                    v_member_data.birthday, 
                    v_member_data.corner_code, 
                    v_member_data.campaign_code,
                    CASE 
                        WHEN SUBSTRING(v_member_data.member_id, 2, 1) = '1' THEN '男'
                        WHEN SUBSTRING(v_member_data.member_id, 2, 1) = '2' THEN '女'
                    END AS 性別,
                    REPLACE(vepj.item_no + '' + vepj.column_code + '' + t1.group_code + '' + vepj.serial_code, ' ', '') AS 編制號,
                    REPLACE(t2.group_code + '' + t2.group_title, ' ', '') AS 編制官科,
                    REPLACE(es.school_code + '' + es.school_desc, ' ', '') AS 軍校名稱,
                    vec.class_name AS 基礎軍事學資,
                    pc1.perform_name AS N_1年考績,
                    pc2.perform_name AS N_2年考績,
                    pc3.perform_name AS N_3年考績,
                    pc4.perform_name AS N_4年考績,
                    pc5.perform_name AS N_5年考績
                FROM Army.dbo.v_member_data  
                LEFT JOIN Army.dbo.v_es_person_join AS vepj ON vepj.member_id = v_member_data.member_id
                LEFT JOIN Army.dbo.tgroup AS t1 ON t1.group_code = vepj.group_code -- 編官科
                LEFT JOIN Army.dbo.tgroup AS t2 ON t2.group_code = vepj.member_group -- 現官科
                LEFT JOIN Army.dbo.educ_school AS es ON es.school_code = vepj.educ_school -- 學校
                LEFT JOIN (
                    SELECT member_id, class_name
                    FROM Army.dbo.v_education
                    LEFT JOIN Army.dbo.educ_class AS ecl ON ecl.class_code = v_education.class_code
                    WHERE v_education.educ_code = 'H'
                ) AS vec ON vec.member_id = v_member_data.member_id
                LEFT JOIN Army.dbo.v_performance AS vp1 ON vp1.member_id = v_member_data.member_id AND vp1.p_year = YEAR(GETDATE()) - 1911 - 1
                LEFT JOIN Army.dbo.perf_code AS pc1 ON pc1.perform_code = vp1.perform_code
                LEFT JOIN Army.dbo.v_performance AS vp2 ON vp2.member_id = v_member_data.member_id AND vp2.p_year = YEAR(GETDATE()) - 1911 - 2
                LEFT JOIN Army.dbo.perf_code AS pc2 ON pc2.perform_code = vp2.perform_code
                LEFT JOIN Army.dbo.v_performance AS vp3 ON vp3.member_id = v_member_data.member_id AND vp3.p_year = YEAR(GETDATE()) - 1911 - 3
                LEFT JOIN Army.dbo.perf_code AS pc3 ON pc3.perform_code = vp3.perform_code
                LEFT JOIN Army.dbo.v_performance AS vp4 ON vp4.member_id = v_member_data.member_id AND vp4.p_year = YEAR(GETDATE()) - 1911 - 4
                LEFT JOIN Army.dbo.perf_code AS pc4 ON pc4.perform_code = vp4.perform_code
                LEFT JOIN Army.dbo.v_performance AS vp5 ON vp5.member_id = v_member_data.member_id AND vp5.p_year = YEAR(GETDATE()) - 1911 - 5
                LEFT JOIN Army.dbo.perf_code AS pc5 ON pc5.perform_code = vp5.perform_code
                WHERE v_member_data.member_id IN ({string.Join(",", selData.IdNumber.Select(id => $"'{id}'"))})
                ORDER BY CASE";

                int SortingWeight = 1;
                foreach (string memberId in selData.IdNumber)
                {
                    getMemberSql += " WHEN v_member_data.member_id = '" + memberId + "' THEN " + SortingWeight;
                    SortingWeight++;
                }
                getMemberSql += @" ELSE 999
                                  END;";

                DataTable getMemberTb = _dbHelper.ArmyWebExecuteQuery(getMemberSql);

                if (getMemberTb == null || getMemberTb.Rows.Count == 0)
                {
                    return Ok(new { Result = "No Member" });
                }

               

                foreach (DataRow row in getMemberTb.Rows)
                {
                    int rank = int.Parse(row["rank_code"].ToString());
                    if (rank > 0 && rank <= 23)
                    {
                        GeneralReq generalRecord = new GeneralReq
                        {
                            GeneralId = row["member_id"].ToString(),

                            GeneralName = row["member_name"].ToString(),

                            GeneralRank = row["rank_code"].ToString()
                        };
                        generalReq.Add(generalRecord);
                    }

                    string rank_date = _codeToName.dateTimeTran(row["rank_date"].ToString(), "yyy年MM月dd日", true);
                    string pay_date = _codeToName.dateTimeTran(row["pay_date"].ToString(), "yyy年MM月dd日", true);
                    string salary_date = _codeToName.dateTimeTran(row["salary_date"].ToString(), "yyy年MM月dd日", true);
                    string birthday = _codeToName.dateTimeTran(row["birthday"].ToString(), "yyy年MM月dd日", true);
                    string name = row["member_name"].ToString();
                    string id = row["member_id"].ToString();

                    row["item_no"] = _codeToName.itemName(row["item_no"].ToString(), row["unit_code"].ToString());
                    row["unit_code"] = _codeToName.unitName(row["unit_code"].ToString());
                    row["es_rank_code"] = _codeToName.rankName(row["es_rank_code"].ToString());
                    row["rank_code"] = _codeToName.rankName(row["rank_code"].ToString());
                    row["service_code"] = _codeToName.serviceName(row["service_code"].ToString());
                    row["es_skill_code"] = _codeToName.skillName(row["es_skill_code"].ToString());
                    row["title_code"] = _codeToName.titleName(row["title_code"].ToString());
                    row["group_code"] = _codeToName.groupName(row["group_code"].ToString());
                    row["m_skill_code"] = _codeToName.skillName(row["m_skill_code"].ToString());
                    row["military_educ_code"] = _codeToName.educName(row["military_educ_code"].ToString());
                    row["school_code"] = _codeToName.schoolName(row["school_code"].ToString());
                    row["campaign_code"] = _codeToName.campaignName(row["campaign_code"].ToString());

                    // 按照你所需的欄位填充屬性
                    List<string> memberData = new List<string>
                    {   
                        row["unit_code"].ToString(),
                        row["item_no"].ToString(),                     
                        row["es_rank_code"].ToString(),
                        row["rank_code"].ToString(),
                        row["service_code"].ToString(),                        
                        row["es_skill_code"].ToString(),
                        row["title_code"].ToString(),
                        row["member_id"].ToString(),
                        row["member_name"].ToString(),
                        row["性別"].ToString(),
                        row["編制號"].ToString(),
                        row["supply_rank"].ToString(),
                        row["group_code"].ToString(),
                        row["編制官科"].ToString(),
                        row["m_skill_code"].ToString(),
                        row["military_educ_code"].ToString(),
                        row["軍校名稱"].ToString(),
                        row["school_code"].ToString(),
                        rank_date,
                        pay_date,
                        salary_date,
                        birthday,
                        row["corner_code"].ToString(),
                        row["campaign_code"].ToString(),
                        row["基礎軍事學資"].ToString(),
                        row["N_1年考績"].ToString(),
                        row["N_2年考績"].ToString(),
                        row["N_3年考績"].ToString(),
                        row["N_4年考績"].ToString(),
                        row["N_5年考績"].ToString()
                    };

                    excelData.Add(memberData);
                }
                string dateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                string excelName = "~/Report/" + dateTime + "_年籍冊查詢.xlsx";
                string excelOutputPath = System.Web.Hosting.HostingEnvironment.MapPath(excelName);
                string urlPath = Request.RequestUri.GetLeftPart(UriPartial.Authority) + $"/{ConfigurationManager.AppSettings.Get("ApiPath")}/Report/";
                string excelHttpPath = string.Empty;
                bool excelResult = true;

                excelResult = _makeReport.exportYearbookExcel(excelData, excelOutputPath, "N");
                
                if (excelResult)
                {
                    excelName = dateTime + "_年籍冊查詢.xlsx";                    
                    excelHttpPath = urlPath + excelName;
                    _makeReport.checkGeneral(generalReq, selData.UserId, excelName, "現員年籍冊下載");
                }

                return Ok(new { Result = "Success", excelPath = excelHttpPath });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【YearbookExport Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【YearbookExport Fail】");
            }
        }

        // Post api/Yearbook
        // 現員年籍冊查詢(檔案輸入)
        [HttpPost]
        [ActionName("YearbookSearchFile")]
        public async Task<IHttpActionResult> YearbookSearchFile()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return BadRequest("Invalid request, expecting multipart file upload");
                }

                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                var idNumberList = new List<string>();
                var yearBookList = new List<object>();
                // 取得上傳的文件
                foreach (var file in provider.Contents)
                {
                    
                    var buffer = await file.ReadAsByteArrayAsync();
                    var filename = file.Headers.ContentDisposition.FileName.Trim('\"');
                    var fileExtension = Path.GetExtension(filename).ToLower();
                    // 將文件保存到 MemoryStream
                    using (var stream = new MemoryStream(buffer))
                    {
                        switch (fileExtension)
                        {
                            case ".txt":
                                idNumberList = _makeReport.txtReadLines(stream);
                                break;
                            case ".xlsx":
                                idNumberList = _makeReport.excelReadLines(stream);
                                break;
                            default:
                                return Ok(new { Result = "不支援的檔案格式", yearBookList });                        
                        }
                    }
                }

                // 過濾空白或無效的身分證號碼
                //idNumberList = idNumberList.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();

                if (!idNumberList.Any())
                {
                    return BadRequest("No valid ID numbers found in the provided file.");
                }

                // 身份證字號的驗證
                List<string> wrongId = new List<string>();
                bool wrongReq = true;
                foreach (string userId in idNumberList)
                {
                    string msg = "";
                    var result = (new Class_TaiwanID()).Check(userId, out msg);
                    if (!result)
                    {
                        wrongId.Add(userId);
                        wrongReq = false;
                    }
                }

                if (!wrongReq)
                {
                    return Ok(new { Result = "Wrong Member Id", WrongId = wrongId, yearBookList });
                }
                

                string getMemberSql = $@"SELECT                     
                    v_member_data.es_rank_code, 
                    v_member_data.rank_code, 
                    v_member_data.service_code, 
                    v_member_data.unit_code, 
                    v_member_data.item_no, 
                    v_member_data.es_skill_code, 
                    v_member_data.title_code, 
                    v_member_data.member_id, 
                    v_member_data.member_name, 
                    v_member_data.supply_rank, 
                    v_member_data.group_code, 
                    v_member_data.m_skill_code, 
                    v_member_data.military_educ_code, 
                    v_member_data.school_code,
                    v_member_data.rank_date, 
                    v_member_data.pay_date, 
                    v_member_data.salary_date, 
                    v_member_data.birthday, 
                    v_member_data.corner_code, 
                    v_member_data.campaign_code,
                    CASE 
                        WHEN SUBSTRING(v_member_data.member_id, 2, 1) = '1' THEN '男'
                        WHEN SUBSTRING(v_member_data.member_id, 2, 1) = '2' THEN '女'
                    END AS 性別,
                    REPLACE(vepj.item_no + '' + vepj.column_code + '' + t1.group_code + '' + vepj.serial_code, ' ', '') AS 編制號,
                    REPLACE(t2.group_code + '' + t2.group_title, ' ', '') AS 編制官科,
                    REPLACE(es.school_code + '' + es.school_desc, ' ', '') AS 軍校名稱,
                    vec.class_name AS 基礎軍事學資,
                    pc1.perform_name AS N_1年考績,
                    pc2.perform_name AS N_2年考績,
                    pc3.perform_name AS N_3年考績,
                    pc4.perform_name AS N_4年考績,
                    pc5.perform_name AS N_5年考績
                FROM Army.dbo.v_member_data  
                LEFT JOIN Army.dbo.v_es_person_join AS vepj ON vepj.member_id = v_member_data.member_id
                LEFT JOIN Army.dbo.tgroup AS t1 ON t1.group_code = vepj.group_code -- 編官科
                LEFT JOIN Army.dbo.tgroup AS t2 ON t2.group_code = vepj.member_group -- 現官科
                LEFT JOIN Army.dbo.educ_school AS es ON es.school_code = vepj.educ_school -- 學校
                LEFT JOIN (
                    SELECT member_id, class_name
                    FROM Army.dbo.v_education
                    LEFT JOIN Army.dbo.educ_class AS ecl ON ecl.class_code = v_education.class_code
                    WHERE v_education.educ_code = 'H'
                ) AS vec ON vec.member_id = v_member_data.member_id
                LEFT JOIN Army.dbo.v_performance AS vp1 ON vp1.member_id = v_member_data.member_id AND vp1.p_year = YEAR(GETDATE()) - 1911 - 1
                LEFT JOIN Army.dbo.perf_code AS pc1 ON pc1.perform_code = vp1.perform_code
                LEFT JOIN Army.dbo.v_performance AS vp2 ON vp2.member_id = v_member_data.member_id AND vp2.p_year = YEAR(GETDATE()) - 1911 - 2
                LEFT JOIN Army.dbo.perf_code AS pc2 ON pc2.perform_code = vp2.perform_code
                LEFT JOIN Army.dbo.v_performance AS vp3 ON vp3.member_id = v_member_data.member_id AND vp3.p_year = YEAR(GETDATE()) - 1911 - 3
                LEFT JOIN Army.dbo.perf_code AS pc3 ON pc3.perform_code = vp3.perform_code
                LEFT JOIN Army.dbo.v_performance AS vp4 ON vp4.member_id = v_member_data.member_id AND vp4.p_year = YEAR(GETDATE()) - 1911 - 4
                LEFT JOIN Army.dbo.perf_code AS pc4 ON pc4.perform_code = vp4.perform_code
                LEFT JOIN Army.dbo.v_performance AS vp5 ON vp5.member_id = v_member_data.member_id AND vp5.p_year = YEAR(GETDATE()) - 1911 - 5
                LEFT JOIN Army.dbo.perf_code AS pc5 ON pc5.perform_code = vp5.perform_code
                WHERE v_member_data.member_id IN ({string.Join(",", idNumberList.Select(id => $"'{id}'"))})
                ORDER BY CASE";

                int SortingWeight = 1;
                foreach (string memberId in idNumberList)
                {
                    getMemberSql += " WHEN v_member_data.member_id = '" + memberId + "' THEN " + SortingWeight;
                    SortingWeight++;
                }
                getMemberSql += @" ELSE 999
                                  END;";

                DataTable getMemberTb = _dbHelper.ArmyWebExecuteQuery(getMemberSql);

                if (getMemberTb == null || getMemberTb.Rows.Count == 0)
                {
                    return Ok(new { Result = "No Member", WrongId = wrongId, yearBookList });
                }
                

                foreach (DataRow row in getMemberTb.Rows)
                {
                    string rank_date = _codeToName.dateTimeTran(row["rank_date"].ToString(), "yyy年MM月dd日", true);
                    string pay_date = _codeToName.dateTimeTran(row["pay_date"].ToString(), "yyy年MM月dd日", true);
                    string salary_date = _codeToName.dateTimeTran(row["salary_date"].ToString(), "yyy年MM月dd日", true);
                    string birthday = _codeToName.dateTimeTran(row["birthday"].ToString(), "yyy年MM月dd日", true);

                    row["item_no"] = _codeToName.itemName(row["item_no"].ToString(), row["unit_code"].ToString());
                    row["unit_code"] = _codeToName.unitName(row["unit_code"].ToString());
                    row["es_rank_code"] = _codeToName.rankName(row["es_rank_code"].ToString());
                    row["rank_code"] = _codeToName.rankName(row["rank_code"].ToString());
                    row["service_code"] = _codeToName.serviceName(row["service_code"].ToString());
                    row["es_skill_code"] = _codeToName.skillName(row["es_skill_code"].ToString());
                    row["title_code"] = _codeToName.titleName(row["title_code"].ToString());
                    row["group_code"] = _codeToName.groupName(row["group_code"].ToString());
                    row["m_skill_code"] = _codeToName.skillName(row["m_skill_code"].ToString());
                    row["military_educ_code"] = _codeToName.educName(row["military_educ_code"].ToString());
                    row["school_code"] = _codeToName.schoolName(row["school_code"].ToString());
                    row["campaign_code"] = _codeToName.campaignName(row["campaign_code"].ToString());

                    var memberData = new
                    {                      
                        EsRankCode = row["es_rank_code"].ToString(),
                        RankCode = row["rank_code"].ToString(),
                        ServiceCode = row["service_code"].ToString(),
                        UnitCode = row["unit_code"].ToString(),
                        ItemNo = row["item_no"].ToString(),
                        EsSkillCode = row["es_skill_code"].ToString(),
                        TitleCode = row["title_code"].ToString(),
                        MemberId = row["member_id"].ToString(),
                        MemberName = row["member_name"].ToString(),
                        Gender = row["性別"].ToString(),
                        EstablishmentNumber = row["編制號"].ToString(),
                        SupplyRank = row["supply_rank"].ToString(),
                        GroupCode = row["group_code"].ToString(),
                        EstablishmentOfficial = row["編制官科"].ToString(),
                        MSkillCode = row["m_skill_code"].ToString(),
                        MilitaryEducCode = row["military_educ_code"].ToString(),
                        MilitarySchoolName = row["軍校名稱"].ToString(),
                        SchoolCode = row["school_code"].ToString(),
                        RankDate = rank_date,
                        PayDate = pay_date,
                        SalaryDate = salary_date,
                        Birthday = birthday,
                        CornerCode = row["corner_code"].ToString(),
                        CampaignCode = row["campaign_code"].ToString(),
                        BasicMilitaryEducation = row["基礎軍事學資"].ToString(),
                        PerformanceAppraisal1 = row["N_1年考績"].ToString(),
                        PerformanceAppraisal2 = row["N_2年考績"].ToString(),
                        PerformanceAppraisal3 = row["N_3年考績"].ToString(),
                        PerformanceAppraisal4 = row["N_4年考績"].ToString(),
                        PerformanceAppraisal5 = row["N_5年考績"].ToString()
                    };

                    yearBookList.Add(memberData);
                }

                return Ok(new { Result = "Success", WrongId = wrongId, yearBookList });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【YearbookSearchFile Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【YearbookSearchFile Fail】");
            }
        }

        // 年級冊 退員查詢
        [HttpPost]
        [ActionName("YearbookRetireSearch")]
        public async Task<IHttpActionResult> YearbookRetireSearch([FromBody] List<string> idNumber)
        {
            try
            {
                var yearBookList = new List<object>();

                // 身份證字號的驗證
                List<string> wrongId = new List<string>();
                bool wrongReq = true;
                foreach (string userId in idNumber)
                {
                    string msg = "";
                    var result = (new Class_TaiwanID()).Check(userId, out msg);
                    if (!result)
                    {
                        wrongId.Add(userId);
                        wrongReq = false;
                    }
                }

                if (!wrongReq)
                {
                    return Ok(new { Result = "Wrong Member Id", WrongId = wrongId, yearBookList });
                }
               

                // 根據提供的欄位構建SQL語句
                string getMemberSql = $@"SELECT 
                    v_member_retire.es_rank_code, 
                    v_member_retire.rank_code, 
                    v_member_retire.unit_code, 
                    v_member_retire.es_skill_code, 
                    v_member_retire.title_code, 
                    v_member_retire.member_id, 
                    v_member_retire.member_name, 
                    v_member_retire.group_code, 
                    v_member_retire.rank_date, 
                    v_member_retire.pay_date, 
                    v_member_retire.salary_date, 
                    v_member_retire.birthday, 
                    v_member_retire.corner_code, 
                    v_member_retire.campaign_code,
                    v_member_retire.retire_date,
                    CASE 
                        WHEN SUBSTRING(v_member_retire.member_id, 2, 1) = '1' THEN '男'
                        WHEN SUBSTRING(v_member_retire.member_id, 2, 1) = '2' THEN '女'
                    END AS 性別,
                    REPLACE(vepj.item_no + '' + vepj.column_code + '' + t1.group_code + '' + vepj.serial_code, ' ', '') AS 編制號                    
                FROM Army.dbo.v_member_retire  
                LEFT JOIN Army.dbo.v_es_person_join AS vepj ON vepj.member_id = v_member_retire.member_id
                LEFT JOIN Army.dbo.tgroup AS t1 ON t1.group_code = vepj.group_code -- 編官科
                LEFT JOIN Army.dbo.tgroup AS t2 ON t2.group_code = vepj.member_group -- 現官科    
                WHERE v_member_retire.member_id IN ({string.Join(",", idNumber.Select(id => $"'{id}'"))})
                ORDER BY CASE";

                int SortingWeight = 1;
                foreach (string memberId in idNumber)
                {
                    getMemberSql += " WHEN v_member_retire.member_id = '" + memberId + "' THEN " + SortingWeight;
                    SortingWeight++;
                }
                getMemberSql += @" ELSE 999
                                  END;";

                DataTable getMemberTb = _dbHelper.ArmyWebExecuteQuery(getMemberSql);

                if (getMemberTb == null || getMemberTb.Rows.Count == 0)
                {
                    return Ok(new { Result = "No Member", WrongId = wrongId, yearBookList });
                }                

                foreach (DataRow row in getMemberTb.Rows)
                {
                    string rank_date = _codeToName.dateTimeTran(row["rank_date"].ToString(), "yyy年MM月dd日", true);
                    string pay_date = _codeToName.dateTimeTran(row["pay_date"].ToString(), "yyy年MM月dd日", true);
                    string salary_date = _codeToName.dateTimeTran(row["salary_date"].ToString(), "yyy年MM月dd日", true);
                    string birthday = _codeToName.dateTimeTran(row["birthday"].ToString(), "yyy年MM月dd日", true);
                    string retire_date = _codeToName.dateTimeTran(row["retire_date"].ToString(), "yyy年MM月dd日", true);

                    row["unit_code"] = _codeToName.unitName(row["unit_code"].ToString());
                    row["es_rank_code"] = _codeToName.rankName(row["es_rank_code"].ToString());
                    row["rank_code"] = _codeToName.rankName(row["rank_code"].ToString());
                    row["es_skill_code"] = _codeToName.skillName(row["es_skill_code"].ToString());
                    row["title_code"] = _codeToName.titleName(row["title_code"].ToString());
                    row["group_code"] = _codeToName.groupName(row["group_code"].ToString());
                    row["campaign_code"] = _codeToName.campaignName(row["campaign_code"].ToString());

                    // 按照你所需的欄位填充屬性
                    var memberData = new
                    {
                        EsRankCode = row["es_rank_code"].ToString(),
                        RankCode = row["rank_code"].ToString(),
                        UnitCode = row["unit_code"].ToString(),
                        EsSkillCode = row["es_skill_code"].ToString(),
                        TitleCode = row["title_code"].ToString(),
                        MemberId = row["member_id"].ToString(),
                        MemberName = row["member_name"].ToString(),
                        Gender = row["性別"].ToString(),
                        EstablishmentNumber = row["編制號"].ToString(),
                        GroupCode = row["group_code"].ToString(),
                        RankDate = rank_date,
                        PayDate = pay_date,
                        SalaryDate = salary_date,
                        Birthday = birthday,
                        CornerCode = row["corner_code"].ToString(),
                        CampaignCode = row["campaign_code"].ToString(),
                        RetireDate = retire_date
                    };

                    yearBookList.Add(memberData);
                }

                return Ok(new { Result = "Success", WrongId = wrongId, yearBookList });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【YearbookSearch Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【YearbookSearch Fail】");
            }
        }


        // 年級冊 退員查詢(檔案輸入)
        [HttpPost]
        [ActionName("YearbookRetireFile")]
        public async Task<IHttpActionResult> YearbookRetireFile()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return BadRequest("Invalid request, expecting multipart file upload");
                }

                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                var idNumberList = new List<string>();
                var yearBookList = new List<object>();
                // 取得上傳的文件
                foreach (var file in provider.Contents)
                {

                    var buffer = await file.ReadAsByteArrayAsync();
                    var filename = file.Headers.ContentDisposition.FileName.Trim('\"');
                    var fileExtension = Path.GetExtension(filename).ToLower();
                    // 將文件保存到 MemoryStream
                    using (var stream = new MemoryStream(buffer))
                    {
                        switch (fileExtension)
                        {
                            case ".txt":
                                idNumberList = _makeReport.txtReadLines(stream);
                                break;
                            case ".xlsx":
                                idNumberList = _makeReport.excelReadLines(stream);
                                break;
                            default:
                                return Ok(new { Result = "不支援的檔案格式", yearBookList });
                        }
                    }
                }

                if (!idNumberList.Any())
                {
                    return BadRequest("No valid ID numbers found in the provided file.");
                }

                // 身份證字號的驗證
                List<string> wrongId = new List<string>();
                bool wrongReq = true;
                foreach (string userId in idNumberList)
                {
                    string msg = "";
                    var result = (new Class_TaiwanID()).Check(userId, out msg);
                    if (!result)
                    {
                        wrongId.Add(userId);
                        wrongReq = false;
                    }
                }

                if (!wrongReq)
                {
                    return Ok(new { Result = "Wrong Member Id", WrongId = wrongId, yearBookList });
                }
                

                // 根據提供的欄位構建SQL語句
                string getMemberSql = $@"SELECT 
                    v_member_retire.es_rank_code, 
                    v_member_retire.rank_code, 
                    v_member_retire.unit_code, 
                    v_member_retire.es_skill_code, 
                    v_member_retire.title_code, 
                    v_member_retire.member_id, 
                    v_member_retire.member_name, 
                    v_member_retire.group_code, 
                    v_member_retire.rank_date, 
                    v_member_retire.pay_date, 
                    v_member_retire.salary_date, 
                    v_member_retire.birthday, 
                    v_member_retire.corner_code, 
                    v_member_retire.campaign_code,
                    v_member_retire.retire_date,
                    CASE 
                        WHEN SUBSTRING(v_member_retire.member_id, 2, 1) = '1' THEN '男'
                        WHEN SUBSTRING(v_member_retire.member_id, 2, 1) = '2' THEN '女'
                    END AS 性別,
                    REPLACE(vepj.item_no + '' + vepj.column_code + '' + t1.group_code + '' + vepj.serial_code, ' ', '') AS 編制號                    
                FROM Army.dbo.v_member_retire  
                LEFT JOIN Army.dbo.v_es_person_join AS vepj ON vepj.member_id = v_member_retire.member_id
                LEFT JOIN Army.dbo.tgroup AS t1 ON t1.group_code = vepj.group_code -- 編官科
                LEFT JOIN Army.dbo.tgroup AS t2 ON t2.group_code = vepj.member_group -- 現官科   
                WHERE v_member_retire.member_id IN ({string.Join(",", idNumberList.Select(id => $"'{id}'"))})
                ORDER BY CASE";

                int SortingWeight = 1;
                foreach (string memberId in idNumberList)
                {
                    getMemberSql += " WHEN v_member_retire.member_id = '" + memberId + "' THEN " + SortingWeight;
                    SortingWeight++;
                }
                getMemberSql += @" ELSE 999
                                  END;";

                DataTable getMemberTb = _dbHelper.ArmyWebExecuteQuery(getMemberSql);

                if (getMemberTb == null || getMemberTb.Rows.Count == 0)
                {
                    return Ok(new { Result = "No Member", WrongId = wrongId, yearBookList });
                }
                
                foreach (DataRow row in getMemberTb.Rows)
                {
                    string rank_date = _codeToName.dateTimeTran(row["rank_date"].ToString(), "yyy年MM月dd日", true);
                    string pay_date = _codeToName.dateTimeTran(row["pay_date"].ToString(), "yyy年MM月dd日", true);
                    string salary_date = _codeToName.dateTimeTran(row["salary_date"].ToString(), "yyy年MM月dd日", true);
                    string birthday = _codeToName.dateTimeTran(row["birthday"].ToString(), "yyy年MM月dd日", true);
                    string retire_date = _codeToName.dateTimeTran(row["retire_date"].ToString(), "yyy年MM月dd日", true);

                    row["unit_code"] = _codeToName.unitName(row["unit_code"].ToString());
                    row["es_rank_code"] = _codeToName.rankName(row["es_rank_code"].ToString());
                    row["rank_code"] = _codeToName.rankName(row["rank_code"].ToString());
                    row["es_skill_code"] = _codeToName.skillName(row["es_skill_code"].ToString());
                    row["title_code"] = _codeToName.titleName(row["title_code"].ToString());
                    row["group_code"] = _codeToName.groupName(row["group_code"].ToString());
                    row["campaign_code"] = _codeToName.campaignName(row["campaign_code"].ToString());

                    // 按照你所需的欄位填充屬性
                    var memberData = new
                    {
                        EsRankCode = row["es_rank_code"].ToString(),
                        RankCode = row["rank_code"].ToString(),
                        UnitCode = row["unit_code"].ToString(),
                        EsSkillCode = row["es_skill_code"].ToString(),
                        TitleCode = row["title_code"].ToString(),
                        MemberId = row["member_id"].ToString(),
                        MemberName = row["member_name"].ToString(),
                        Gender = row["性別"].ToString(),
                        EstablishmentNumber = row["編制號"].ToString(),
                        GroupCode = row["group_code"].ToString(),
                        RankDate = rank_date,
                        PayDate = pay_date,
                        SalaryDate = salary_date,
                        Birthday = birthday,
                        CornerCode = row["corner_code"].ToString(),
                        CampaignCode = row["campaign_code"].ToString(),
                        RetireDate = retire_date
                    };

                    yearBookList.Add(memberData);
                }

                return Ok(new { Result = "Success", WrongId = wrongId, yearBookList });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【YearbookRetireFile Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【YearbookRetireFile Fail】");
            }
        }


        // 年級冊 退員excel匯出
        [HttpPost]
        [ActionName("YearbookRetireExport")]
        public async Task<IHttpActionResult> YearbookRetireExport([FromBody] IdNumberReq selData)
        {
            try
            {
                List<List<string>> excelData = new List<List<string>>();
                List<GeneralReq> generalReq = new List<GeneralReq>();
                // 根據提供的欄位構建SQL語句
                string getMemberSql = $@"SELECT 
                    v_member_retire.es_rank_code, 
                    v_member_retire.rank_code, 
                    v_member_retire.unit_code, 
                    v_member_retire.es_skill_code, 
                    v_member_retire.title_code, 
                    v_member_retire.member_id, 
                    v_member_retire.member_name, 
                    v_member_retire.group_code, 
                    v_member_retire.rank_date, 
                    v_member_retire.pay_date, 
                    v_member_retire.salary_date, 
                    v_member_retire.birthday, 
                    v_member_retire.corner_code, 
                    v_member_retire.campaign_code,
                    v_member_retire.retire_date,
                    CASE 
                        WHEN SUBSTRING(v_member_retire.member_id, 2, 1) = '1' THEN '男'
                        WHEN SUBSTRING(v_member_retire.member_id, 2, 1) = '2' THEN '女'
                    END AS 性別,
                    REPLACE(vepj.item_no + '' + vepj.column_code + '' + t1.group_code + '' + vepj.serial_code, ' ', '') AS 編制號                    
                FROM Army.dbo.v_member_retire  
                LEFT JOIN Army.dbo.v_es_person_join AS vepj ON vepj.member_id = v_member_retire.member_id
                LEFT JOIN Army.dbo.tgroup AS t1 ON t1.group_code = vepj.group_code -- 編官科
                LEFT JOIN Army.dbo.tgroup AS t2 ON t2.group_code = vepj.member_group -- 現官科   
                WHERE v_member_retire.member_id IN ({string.Join(",", selData.IdNumber.Select(id => $"'{id}'"))})
                ORDER BY CASE";

                int SortingWeight = 1;
                foreach (string memberId in selData.IdNumber)
                {
                    getMemberSql += " WHEN v_member_retire.member_id = '" + memberId + "' THEN " + SortingWeight;
                    SortingWeight++;
                }
                getMemberSql += @" ELSE 999
                                  END;";

                DataTable getMemberTb = _dbHelper.ArmyWebExecuteQuery(getMemberSql);

                if (getMemberTb == null || getMemberTb.Rows.Count == 0)
                {
                    return Ok(new { Result = "No Member" });
                }

                var yearBookList = new List<object>();

                foreach (DataRow row in getMemberTb.Rows)
                {
                    int rank = int.Parse(row["rank_code"].ToString());
                    if (rank > 0 && rank <= 23)
                    {
                        GeneralReq generalRecord = new GeneralReq
                        {
                            GeneralId = row["member_id"].ToString(),

                            GeneralName = row["member_name"].ToString(),

                            GeneralRank = row["rank_code"].ToString()
                        };
                        generalReq.Add(generalRecord);
                    }

                    string rank_date = _codeToName.dateTimeTran(row["rank_date"].ToString(), "yyy年MM月dd日", true);
                    string pay_date = _codeToName.dateTimeTran(row["pay_date"].ToString(), "yyy年MM月dd日", true);
                    string salary_date = _codeToName.dateTimeTran(row["salary_date"].ToString(), "yyy年MM月dd日", true);
                    string birthday = _codeToName.dateTimeTran(row["birthday"].ToString(), "yyy年MM月dd日", true);
                    string retire_date = _codeToName.dateTimeTran(row["retire_date"].ToString(), "yyy年MM月dd日", true);

                    row["unit_code"] = _codeToName.unitName(row["unit_code"].ToString());
                    row["es_rank_code"] = _codeToName.rankName(row["es_rank_code"].ToString());
                    row["rank_code"] = _codeToName.rankName(row["rank_code"].ToString());
                    row["es_skill_code"] = _codeToName.skillName(row["es_skill_code"].ToString());
                    row["title_code"] = _codeToName.titleName(row["title_code"].ToString());
                    row["group_code"] = _codeToName.groupName(row["group_code"].ToString());
                    row["campaign_code"] = _codeToName.campaignName(row["campaign_code"].ToString());
                        
                    // 按照你所需的欄位填充屬性
                    List<string> memberData = new List<string>
                    {
                        row["es_rank_code"].ToString(),
                        row["rank_code"].ToString(),
                        row["unit_code"].ToString(),
                        row["es_skill_code"].ToString(),
                        row["title_code"].ToString(),
                        row["member_id"].ToString(),
                        row["member_name"].ToString(),
                        row["性別"].ToString(),
                        row["編制號"].ToString(),
                        row["group_code"].ToString(),
                        rank_date, 
                        pay_date, 
                        salary_date,
                        birthday,                        
                        row["corner_code"].ToString(),
                        row["campaign_code"].ToString(),
                        retire_date
                    };

                    excelData.Add(memberData);
                }

                string dateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                string excelName = "~/Report/" + dateTime + "_退員年籍冊查詢.xlsx";
                string excelOutputPath = System.Web.Hosting.HostingEnvironment.MapPath(excelName);
                string urlPath = Request.RequestUri.GetLeftPart(UriPartial.Authority) + $"/{ConfigurationManager.AppSettings.Get("ApiPath")}/Report/";
                string excelHttpPath = string.Empty;
                bool excelResult = true;

                excelResult = _makeReport.exportYearbookExcel(excelData, excelOutputPath,"R");

                if (excelResult)
                {
                    excelName = dateTime + "_退員年籍冊查詢.xlsx";
                    excelHttpPath = urlPath + excelName;
                    _makeReport.checkGeneral(generalReq, selData.UserId, excelName, "退員年籍冊下載");
                }

                return Ok(new { Result = "Success", excelPath = excelHttpPath });                
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【YearbookRetireExport Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【YearbookRetireExport Fail】");
            }
        }

    }
}