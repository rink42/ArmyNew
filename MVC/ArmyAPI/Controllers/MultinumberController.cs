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

namespace ArmyAPI.Controllers
{
    public class MultinumberController : ApiController
    {
        private readonly DbHelper _dbHelper;
        private readonly MakeReport _makeReport;

        public MultinumberController()
        {
            _dbHelper = new DbHelper();
            _makeReport = new MakeReport();
        }

        // Post api/Multinumber
        // 多兵號查詢(手動輸入)
        [HttpPost]
        [ActionName("MultinumberSearch")]
        public async Task<IHttpActionResult> MultinumberSearch([FromBody] List<string> idNumber)
        {
            try
            {
                // 根據提供的欄位構建SQL語句
                string getMemberSql = $@"SELECT member_id, member_name, unit_code, non_es_code, item_no,
                                column_no, serial_code, pre_es_skill_code, es_skill_code, es_rank_code,
                                title_code, pay_date, service_code, group_code, campaign_code, rank_code,
                                supply_rank, recampaign_month, rank_date, pre_m_skill_code, m_skill_code,
                                pay_unit_code, pay_remark, bonus_code, main_bonus, work_status,
                                original_pay, corner_code, update_date, trans_code, campaign_serial,
                                volun_soldier_date, volun_sergeant_date, volun_officer_date, 
                                again_campaign_date, stop_volunteer_date
                         FROM Army.dbo.v_member_data 
                         WHERE member_id IN ({string.Join(",", idNumber.Select(id => $"'{id}'"))})";

                DataTable getMemberTb = _dbHelper.ArmyExecuteQuery(getMemberSql);

                if (getMemberTb == null || getMemberTb.Rows.Count == 0)
                {
                    return Ok(new { Result = "No Member" });
                }

                var MultinumberList = new List<object>();

                foreach (DataRow row in getMemberTb.Rows)
                {
                    // 按照你所需的欄位填充屬性
                    var memberData = new
                    {
                        MemberId = row["member_id"].ToString(),
                        MemberName = row["member_name"].ToString(),
                        UnitCode = row["unit_code"].ToString(),
                        NonEsCode = row["non_es_code"].ToString(),
                        ItemNo = row["item_no"].ToString(),
                        ColumnNo = row["column_no"].ToString(),
                        SerialCode = row["serial_code"].ToString(),
                        PreEsSkillCode = row["pre_es_skill_code"].ToString(),
                        EsSkillCode = row["es_skill_code"].ToString(),
                        EsRankCode = row["es_rank_code"].ToString(),
                        TitleCode = row["title_code"].ToString(),
                        PayDate = row["pay_date"].ToString(),
                        ServiceCode = row["service_code"].ToString(),
                        GroupCode = row["group_code"].ToString(),
                        CampaignCode = row["campaign_code"].ToString(),
                        RankCode = row["rank_code"].ToString(),
                        SupplyRank = row["supply_rank"].ToString(),
                        RecampaignMonth = row["recampaign_month"].ToString(),
                        RankDate = row["rank_date"].ToString(),
                        PreMSkillCode = row["pre_m_skill_code"].ToString(),
                        MSkillCode = row["m_skill_code"].ToString(),
                        PayUnitCode = row["pay_unit_code"].ToString(),
                        PayRemark = row["pay_remark"].ToString(),
                        BonusCode = row["bonus_code"].ToString(),
                        MainBonus = row["main_bonus"].ToString(),
                        WorkStatus = row["work_status"].ToString(),
                        OriginalPay = row["original_pay"].ToString(),
                        CornerCode = row["corner_code"].ToString(),
                        UpdateDate = row["update_date"].ToString(),
                        TransCode = row["trans_code"].ToString(),
                        CampaignSerial = row["campaign_serial"].ToString(),
                        VolunSoldierDate = row["volun_soldier_date"].ToString(),
                        VolunSergeantDate = row["volun_sergeant_date"].ToString(),
                        VolunOfficerDate = row["volun_officer_date"].ToString(),
                        AgainCampaignDate = row["again_campaign_date"].ToString(),
                        StopVolunteerDate = row["stop_volunteer_date"].ToString()
                    };

                    MultinumberList.Add(memberData);
                }

                return Ok(new { Result = "Success", MultinumberList });
            }
            catch (Exception ex)
            {
                return BadRequest("【MultinumberSearch Fail】" + ex.ToString());
            }
        }


        [HttpPost]
        [ActionName("MultinumberExport")]
        public async Task<IHttpActionResult> MultinumberExport([FromBody] List<string> idNumber)
        {
            try
            {
                List<List<string>> excelData = new List<List<string>>();
                // 根據提供的欄位構建SQL語句
                string getMemberSql = $@"SELECT member_id, member_name, unit_code, non_es_code, item_no,
                                column_no, serial_code, pre_es_skill_code, es_skill_code, es_rank_code,
                                title_code, pay_date, service_code, group_code, campaign_code, rank_code,
                                supply_rank, recampaign_month, rank_date, pre_m_skill_code, m_skill_code,
                                pay_unit_code, pay_remark, bonus_code, main_bonus, work_status,
                                original_pay, corner_code, update_date, trans_code, campaign_serial,
                                volun_soldier_date, volun_sergeant_date, volun_officer_date, 
                                again_campaign_date, stop_volunteer_date
                         FROM Army.dbo.v_member_data 
                         WHERE member_id IN ({string.Join(",", idNumber.Select(id => $"'{id}'"))})";

                DataTable getMemberTb = _dbHelper.ArmyExecuteQuery(getMemberSql);

                if (getMemberTb == null || getMemberTb.Rows.Count == 0)
                {
                    return Ok(new { Result = "No Member" });
                }

                var MultinumberList = new List<object>();

                foreach (DataRow row in getMemberTb.Rows)
                {
                    // 按照你所需的欄位填充屬性
                    List<string> memberData = new List<string>
                    {
                        row["member_id"].ToString(),
                        row["member_name"].ToString(),
                        row["unit_code"].ToString(),
                        row["non_es_code"].ToString(),
                        row["item_no"].ToString(),
                        row["column_no"].ToString(),
                        row["serial_code"].ToString(),
                        row["pre_es_skill_code"].ToString(),
                        row["es_skill_code"].ToString(),
                        row["es_rank_code"].ToString(),
                        row["title_code"].ToString(),
                        row["pay_date"].ToString(),
                        row["service_code"].ToString(),
                        row["group_code"].ToString(),
                        row["campaign_code"].ToString(),
                        row["rank_code"].ToString(),
                        row["supply_rank"].ToString(),
                        row["recampaign_month"].ToString(),
                        row["rank_date"].ToString(),
                        row["pre_m_skill_code"].ToString(),
                        row["m_skill_code"].ToString(),
                        row["pay_unit_code"].ToString(),
                        row["pay_remark"].ToString(),
                        row["bonus_code"].ToString(),
                        row["main_bonus"].ToString(),
                        row["work_status"].ToString(),
                        row["original_pay"].ToString(),
                        row["corner_code"].ToString(),
                        row["update_date"].ToString(),
                        row["trans_code"].ToString(),
                        row["campaign_serial"].ToString(),
                        row["volun_soldier_date"].ToString(),
                        row["volun_sergeant_date"].ToString(),
                        row["volun_officer_date"].ToString(),
                        row["again_campaign_date"].ToString(),
                        row["stop_volunteer_date"].ToString()
                    };

                    excelData.Add(memberData);
                }
                string dateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                string excelName = "~/Report/" + dateTime + "_多兵號查詢.xlsx";
                string excelOutputPath = System.Web.Hosting.HostingEnvironment.MapPath(excelName);
                string urlPath = Request.RequestUri.GetLeftPart(UriPartial.Authority) + $"/{ConfigurationManager.AppSettings.Get("ApiPath")}/Report/";
                string excelHttpPath = string.Empty;
                bool excelResult = true;

                excelResult = _makeReport.exportMultinumberExcel(excelData, excelOutputPath);

                if (excelResult)
                {
                    excelName = dateTime + "_多兵號查詢.xlsx";
                    excelHttpPath = urlPath + excelName;
                }

                return Ok(new { Result = "Success", excelPath = excelHttpPath });
            }
            catch (Exception ex)
            {
                return BadRequest("【MultinumberSearch Fail】" + ex.ToString());
            }
        }

        // Post api/Multinumber
        // 多兵號查詢(檔案輸入)
        [HttpPost]
        [ActionName("MultinumberSearchFile")]
        public async Task<IHttpActionResult> MultinumberSearchFile()
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

                // 取得上傳的文件
                foreach (var file in provider.Contents)
                {
                    var buffer = await file.ReadAsByteArrayAsync();

                    // 將文件保存到 MemoryStream
                    using (var stream = new MemoryStream(buffer))
                    {
                        using (var package = new ExcelPackage(stream)) 
                        {
                            var worksheet = package.Workbook.Worksheets[0];
                            var rowCount = worksheet.Dimension.Rows;

                            for (int row = 1; row <= rowCount; row++)
                            {
                                idNumberList.Add(worksheet.Cells[row, 1].Text);
                            }
                        }
                    }
                }

                // 過濾空白或無效的身分證號碼
                //idNumberList = idNumberList.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();

                if (!idNumberList.Any())
                {
                    return BadRequest("No valid ID numbers found in the provided file.");
                }

                string getMemberSql = $@"SELECT member_id, member_name, unit_code, non_es_code, item_no,
                            column_no, serial_code, pre_es_skill_code, es_skill_code, es_rank_code,
                            title_code, pay_date, service_code, group_code, campaign_code, rank_code,
                            supply_rank, recampaign_month, rank_date, pre_m_skill_code, m_skill_code,
                            pay_unit_code, pay_remark, bonus_code, main_bonus, work_status,
                            original_pay, corner_code, update_date, trans_code, campaign_serial,
                            volun_soldier_date, volun_sergeant_date, volun_officer_date, 
                            again_campaign_date, stop_volunteer_date
                    FROM Army.dbo.v_member_data 
                    WHERE member_id IN ({string.Join(",", idNumberList.Select(id => $"'{id}'"))})";

                DataTable getMemberTb = _dbHelper.ArmyExecuteQuery(getMemberSql);

                if (getMemberTb == null || getMemberTb.Rows.Count == 0)
                {
                    return Ok(new { Result = "No Member" });
                }

                var MultinumberList = new List<object>();

                foreach (DataRow row in getMemberTb.Rows)
                {
                    var memberData = new
                    {
                        MemberId = row["member_id"].ToString(),
                        MemberName = row["member_name"].ToString(),
                        UnitCode = row["unit_code"].ToString(),
                        NonEsCode = row["non_es_code"].ToString(),
                        ItemNo = row["item_no"].ToString(),
                        ColumnNo = row["column_no"].ToString(),
                        SerialCode = row["serial_code"].ToString(),
                        PreEsSkillCode = row["pre_es_skill_code"].ToString(),
                        EsSkillCode = row["es_skill_code"].ToString(),
                        EsRankCode = row["es_rank_code"].ToString(),
                        TitleCode = row["title_code"].ToString(),
                        PayDate = row["pay_date"].ToString(),
                        ServiceCode = row["service_code"].ToString(),
                        GroupCode = row["group_code"].ToString(),
                        CampaignCode = row["campaign_code"].ToString(),
                        RankCode = row["rank_code"].ToString(),
                        SupplyRank = row["supply_rank"].ToString(),
                        RecampaignMonth = row["recampaign_month"].ToString(),
                        RankDate = row["rank_date"].ToString(),
                        PreMSkillCode = row["pre_m_skill_code"].ToString(),
                        MSkillCode = row["m_skill_code"].ToString(),
                        PayUnitCode = row["pay_unit_code"].ToString(),
                        PayRemark = row["pay_remark"].ToString(),
                        BonusCode = row["bonus_code"].ToString(),
                        MainBonus = row["main_bonus"].ToString(),
                        WorkStatus = row["work_status"].ToString(),
                        OriginalPay = row["original_pay"].ToString(),
                        CornerCode = row["corner_code"].ToString(),
                        UpdateDate = row["update_date"].ToString(),
                        TransCode = row["trans_code"].ToString(),
                        CampaignSerial = row["campaign_serial"].ToString(),
                        VolunSoldierDate = row["volun_soldier_date"].ToString(),
                        VolunSergeantDate = row["volun_sergeant_date"].ToString(),
                        VolunOfficerDate = row["volun_officer_date"].ToString(),
                        AgainCampaignDate = row["again_campaign_date"].ToString(),
                        StopVolunteerDate = row["stop_volunteer_date"].ToString()
                    };

                    MultinumberList.Add(memberData);
                }

                return Ok(new { Result = "Success", MultinumberList });
            }
            catch (Exception ex)
            {
                return BadRequest("【MultinumberSearchFile Fail】" + ex.ToString());
            }
        }


       



    }
}