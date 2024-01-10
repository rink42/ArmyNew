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
using System.Data.SqlClient;

namespace ArmyAPI.Controllers
{
    public class MultinumberController : ApiController
    {
        private readonly DbHelper _dbHelper;
        private readonly MakeReport _makeReport;
        private readonly CodetoName _codeToName;        

        public MultinumberController()
        {
            _dbHelper = new DbHelper();
            _makeReport = new MakeReport();
            _codeToName = new CodetoName();
        }

        
        // 多兵號查詢(手動輸入)
        [HttpPost]
        [ActionName("MultinumberSearch")]
        public async Task<IHttpActionResult> MultinumberSearch([FromBody] List<string> idNumber)
        {
            try
            {
                List<MultinumberRes> MultinumberList = new List<MultinumberRes>();

                // 身份證字號的驗證
                // Step 1. 身份證字號的驗證
                List<string> wrongId = new List<string>();
                bool wrongReq = true;
                foreach (string userId in idNumber)
                {
                    if(userId != "")
                    {
                        string msg = "";
                        var result = (new Class_TaiwanID()).Check(userId, out msg);
                        if (!result)
                        {
                            wrongId.Add(userId);
                            wrongReq = false;
                        }
                    }
                }

                if (!wrongReq)
                {
                    return Ok(new { Result = "Wrong Member Id", WrongId = wrongId, MultinumberList });
                }

                // 資料格式化
                foreach (string id in idNumber)
                {
                    MultinumberRes memberData = new MultinumberRes();
                    if (id == "") 
                    {
                        memberData = new MultinumberRes
                        {
                            MemberId = id,
                            MemberName = "查無此人",
                        };
                        MultinumberList.Add(memberData);
                        continue;
                    }
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
                        WHERE member_id = @memberId";

                    SqlParameter[] getMemberPara = { new SqlParameter("@memberId",SqlDbType.VarChar) {Value = id } };

                    DataTable getMemberTb = _dbHelper.ArmyWebExecuteQuery(getMemberSql, getMemberPara);

                    if (getMemberTb != null && getMemberTb.Rows.Count != 0)
                    {
                        DataRow row = getMemberTb.Rows[0];
                        string Pay_Date = _codeToName.dateTimeTran(row["pay_date"].ToString(), "yyy年MM月dd日", true);
                        string Rank_Date = _codeToName.dateTimeTran(row["rank_date"].ToString(), "yyy年MM月dd日", true);
                        string Update_Date = _codeToName.dateTimeTran(row["update_date"].ToString(), "yyy年MM月dd日", true);
                        string Volun_Soldier_Date = _codeToName.dateTimeTran(row["volun_soldier_date"].ToString(), "yyy年MM月dd日", true);
                        string Volun_Sergeant_Date = _codeToName.dateTimeTran(row["volun_sergeant_date"].ToString(), "yyy年MM月dd日", true);
                        string Volun_Officer_Date = _codeToName.dateTimeTran(row["volun_officer_date"].ToString(), "yyy年MM月dd日", true);
                        string Again_Campaign_Date = _codeToName.dateTimeTran(row["again_campaign_date"].ToString(), "yyy年MM月dd日", true);
                        string Stop_Volunteer_Date = _codeToName.dateTimeTran(row["stop_volunteer_date"].ToString(), "yyy年MM月dd日", true);

                        // 按照你所需的欄位填充屬性
                        memberData = new MultinumberRes
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
                            PayDate = Pay_Date,
                            ServiceCode = row["service_code"].ToString(),
                            GroupCode = row["group_code"].ToString(),
                            CampaignCode = row["campaign_code"].ToString(),
                            RankCode = row["rank_code"].ToString(),
                            SupplyRank = row["supply_rank"].ToString(),
                            RecampaignMonth = row["recampaign_month"].ToString(),
                            RankDate = Rank_Date,
                            PreMSkillCode = row["pre_m_skill_code"].ToString(),
                            MSkillCode = row["m_skill_code"].ToString(),
                            PayUnitCode = row["pay_unit_code"].ToString(),
                            PayRemark = row["pay_remark"].ToString(),
                            BonusCode = row["bonus_code"].ToString(),
                            MainBonus = row["main_bonus"].ToString(),
                            WorkStatus = row["work_status"].ToString(),
                            OriginalPay = row["original_pay"].ToString(),
                            CornerCode = row["corner_code"].ToString(),
                            UpdateDate = Update_Date,
                            TransCode = row["trans_code"].ToString(),
                            CampaignSerial = row["campaign_serial"].ToString(),
                            VolunSoldierDate = Volun_Soldier_Date,
                            VolunSergeantDate = Volun_Sergeant_Date,
                            VolunOfficerDate = Volun_Officer_Date,
                            AgainCampaignDate = Again_Campaign_Date,
                            StopVolunteerDate = Stop_Volunteer_Date
                        };
                    }
                    else
                    {
                        memberData = new MultinumberRes
                        {
                            MemberId = id,
                            MemberName = "查無此人",
                        };
                    }
                    MultinumberList.Add(memberData);
                }

                return Ok(new { Result = "Success", WrongId = wrongId, MultinumberList });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【MultinumberSearch Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【MultinumberSearch Fail】");
            }
        }

        // 未到多兵號查詢(手動輸入)
        [HttpPost]
        [ActionName("RelayMultinumberSearch")]
        public async Task<IHttpActionResult> RelayMultinumberSearch([FromBody] List<string> idNumber)
        {
            try
            {
                List<RelayMultinumberRes> MultinumberList = new List<RelayMultinumberRes>();

                // 身份證字號的驗證
                List<string> wrongId = new List<string>();
                bool wrongReq = true;
                foreach (string userId in idNumber)
                {
                    if (userId != "")
                    {
                        string msg = "";
                        var result = (new Class_TaiwanID()).Check(userId, out msg);
                        if (!result)
                        {
                            wrongId.Add(userId);
                            wrongReq = false;
                        }
                    }
                }

                if (!wrongReq)
                {
                    return Ok(new { Result = "Wrong Member Id", WrongId = wrongId, MultinumberList });
                }

                // 資料格式化
                foreach (string id in idNumber)
                {
                    RelayMultinumberRes memberData = new RelayMultinumberRes();
                    if (id == "")
                    {
                        memberData = new RelayMultinumberRes
                        {
                            MemberId = id,
                            MemberName = "查無此人",
                        };
                        MultinumberList.Add(memberData);
                        continue;
                    }
                    // 根據提供的欄位構建SQL語句
                    string getMemberSql = $@"SELECT member_id, member_name, unit_code, non_es_code, item_no,
                                column_no, serial_code, pre_es_skill_code, es_skill_code, es_rank_code,
                                title_code, pay_date, service_code, group_code, campaign_code, rank_code,
                                supply_rank, recampaign_month, rank_date, pre_m_skill_code, m_skill_code,
                                pay_unit_code, pay_remark, bonus_code, main_bonus, work_status,
                                original_pay, corner_code, update_date, trans_code, campaign_serial,
                                volun_soldier_date, volun_sergeant_date, volun_officer_date, 
                                again_campaign_date, stop_volunteer_date, retire_date
                         FROM Army.dbo.v_member_relay
                         WHERE member_id = @memberId";

                    SqlParameter[] getMemberPara = { new SqlParameter("@memberId", SqlDbType.VarChar) { Value = id } };

                    DataTable getMemberTb = _dbHelper.ArmyWebExecuteQuery(getMemberSql, getMemberPara);

                    if (getMemberTb != null && getMemberTb.Rows.Count != 0)
                    {
                        DataRow row = getMemberTb.Rows[0];
                        string Pay_Date = _codeToName.dateTimeTran(row["pay_date"].ToString(), "yyy年MM月dd日", true);
                        string Rank_Date = _codeToName.dateTimeTran(row["rank_date"].ToString(), "yyy年MM月dd日", true);
                        string Update_Date = _codeToName.dateTimeTran(row["update_date"].ToString(), "yyy年MM月dd日", true);
                        string Volun_Soldier_Date = _codeToName.dateTimeTran(row["volun_soldier_date"].ToString(), "yyy年MM月dd日", true);
                        string Volun_Sergeant_Date = _codeToName.dateTimeTran(row["volun_sergeant_date"].ToString(), "yyy年MM月dd日", true);
                        string Volun_Officer_Date = _codeToName.dateTimeTran(row["volun_officer_date"].ToString(), "yyy年MM月dd日", true);
                        string Again_Campaign_Date = _codeToName.dateTimeTran(row["again_campaign_date"].ToString(), "yyy年MM月dd日", true);
                        string Stop_Volunteer_Date = _codeToName.dateTimeTran(row["stop_volunteer_date"].ToString(), "yyy年MM月dd日", true);
                        string Retire_Date = _codeToName.dateTimeTran(row["retire_date"].ToString(), "yyy年MM月dd日", true);

                        // 按照你所需的欄位填充屬性
                        memberData = new RelayMultinumberRes
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
                            PayDate = Pay_Date,
                            ServiceCode = row["service_code"].ToString(),
                            GroupCode = row["group_code"].ToString(),
                            CampaignCode = row["campaign_code"].ToString(),
                            RankCode = row["rank_code"].ToString(),
                            SupplyRank = row["supply_rank"].ToString(),
                            RecampaignMonth = row["recampaign_month"].ToString(),
                            RankDate = Rank_Date,
                            PreMSkillCode = row["pre_m_skill_code"].ToString(),
                            MSkillCode = row["m_skill_code"].ToString(),
                            PayUnitCode = row["pay_unit_code"].ToString(),
                            PayRemark = row["pay_remark"].ToString(),
                            BonusCode = row["bonus_code"].ToString(),
                            MainBonus = row["main_bonus"].ToString(),
                            WorkStatus = row["work_status"].ToString(),
                            OriginalPay = row["original_pay"].ToString(),
                            CornerCode = row["corner_code"].ToString(),
                            UpdateDate = Update_Date,
                            TransCode = row["trans_code"].ToString(),
                            CampaignSerial = row["campaign_serial"].ToString(),
                            VolunSoldierDate = Volun_Soldier_Date,
                            VolunSergeantDate = Volun_Sergeant_Date,
                            VolunOfficerDate = Volun_Officer_Date,
                            AgainCampaignDate = Again_Campaign_Date,
                            StopVolunteerDate = Stop_Volunteer_Date,
                            RetireDate = Retire_Date
                        };
                    }
                    else
                    {
                        memberData = new RelayMultinumberRes
                        {
                            MemberId = id,
                            MemberName = "查無此人",
                        };
                    }
                    MultinumberList.Add(memberData);
                }

                return Ok(new { Result = "Success", WrongId = wrongId, MultinumberList });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【MultinumberSearch Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【MultinumberSearch Fail】");
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

                var MultinumberList = new List<object>();
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
                    if (userId != "")
                    {
                        string msg = "";
                        var result = (new Class_TaiwanID()).Check(userId, out msg);
                        if (!result)
                        {
                            wrongId.Add(userId);
                            wrongReq = false;
                        }
                    }
                }

                if (!wrongReq)
                {
                    return Ok(new { Result = "Wrong Member Id", WrongId = wrongId, MultinumberList });
                }

                // 資料格式化
                foreach (string id in idNumberList)
                {
                    MultinumberRes memberData = new MultinumberRes();
                    if (id == "")
                    {
                        memberData = new MultinumberRes
                        {
                            MemberId = id,
                            MemberName = "查無此人",
                        };
                        MultinumberList.Add(memberData);
                        continue;
                    }
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
                         WHERE member_id = @memberId";

                    SqlParameter[] getMemberPara = { new SqlParameter("@memberId", SqlDbType.VarChar) { Value = id } };

                    DataTable getMemberTb = _dbHelper.ArmyWebExecuteQuery(getMemberSql, getMemberPara);

                    if (getMemberTb != null && getMemberTb.Rows.Count != 0)
                    {
                        DataRow row = getMemberTb.Rows[0];
                        string Pay_Date = _codeToName.dateTimeTran(row["pay_date"].ToString(), "yyy年MM月dd日", true);
                        string Rank_Date = _codeToName.dateTimeTran(row["rank_date"].ToString(), "yyy年MM月dd日", true);
                        string Update_Date = _codeToName.dateTimeTran(row["update_date"].ToString(), "yyy年MM月dd日", true);
                        string Volun_Soldier_Date = _codeToName.dateTimeTran(row["volun_soldier_date"].ToString(), "yyy年MM月dd日", true);
                        string Volun_Sergeant_Date = _codeToName.dateTimeTran(row["volun_sergeant_date"].ToString(), "yyy年MM月dd日", true);
                        string Volun_Officer_Date = _codeToName.dateTimeTran(row["volun_officer_date"].ToString(), "yyy年MM月dd日", true);
                        string Again_Campaign_Date = _codeToName.dateTimeTran(row["again_campaign_date"].ToString(), "yyy年MM月dd日", true);
                        string Stop_Volunteer_Date = _codeToName.dateTimeTran(row["stop_volunteer_date"].ToString(), "yyy年MM月dd日", true);

                        // 按照你所需的欄位填充屬性
                        memberData = new MultinumberRes
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
                            PayDate = Pay_Date,
                            ServiceCode = row["service_code"].ToString(),
                            GroupCode = row["group_code"].ToString(),
                            CampaignCode = row["campaign_code"].ToString(),
                            RankCode = row["rank_code"].ToString(),
                            SupplyRank = row["supply_rank"].ToString(),
                            RecampaignMonth = row["recampaign_month"].ToString(),
                            RankDate = Rank_Date,
                            PreMSkillCode = row["pre_m_skill_code"].ToString(),
                            MSkillCode = row["m_skill_code"].ToString(),
                            PayUnitCode = row["pay_unit_code"].ToString(),
                            PayRemark = row["pay_remark"].ToString(),
                            BonusCode = row["bonus_code"].ToString(),
                            MainBonus = row["main_bonus"].ToString(),
                            WorkStatus = row["work_status"].ToString(),
                            OriginalPay = row["original_pay"].ToString(),
                            CornerCode = row["corner_code"].ToString(),
                            UpdateDate = Update_Date,
                            TransCode = row["trans_code"].ToString(),
                            CampaignSerial = row["campaign_serial"].ToString(),
                            VolunSoldierDate = Volun_Soldier_Date,
                            VolunSergeantDate = Volun_Sergeant_Date,
                            VolunOfficerDate = Volun_Officer_Date,
                            AgainCampaignDate = Again_Campaign_Date,
                            StopVolunteerDate = Stop_Volunteer_Date
                        };
                    }
                    else
                    {
                        memberData = new MultinumberRes
                        {
                            MemberId = id,
                            MemberName = "查無此人",
                        };
                    }
                    MultinumberList.Add(memberData);
                }

                return Ok(new { Result = "Success", WrongId = wrongId, MultinumberList });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【MultinumberSearchFile Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【MultinumberSearchFile Fail】");
            }
        }

        // 未到多兵號查詢(檔案輸入)
        [HttpPost]
        [ActionName("RelayMultinumberSearchFile")]
        public async Task<IHttpActionResult> RelayMultinumberSearchFile()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return BadRequest("Invalid request, expecting multipart file upload");
                }

                List<RelayMultinumberRes> MultinumberList = new List<RelayMultinumberRes>();
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
                    if (userId != "")
                    {
                        string msg = "";
                        var result = (new Class_TaiwanID()).Check(userId, out msg);
                        if (!result)
                        {
                            wrongId.Add(userId);
                            wrongReq = false;
                        }
                    }
                }

                if (!wrongReq)
                {
                    return Ok(new { Result = "Wrong Member Id", WrongId = wrongId, MultinumberList });
                }


                // 資料格式化
                foreach (string id in idNumberList)
                {
                    RelayMultinumberRes memberData = new RelayMultinumberRes();
                    if (id == "")
                    {
                        memberData = new RelayMultinumberRes
                        {
                            MemberId = id,
                            MemberName = "查無此人",
                        };
                        MultinumberList.Add(memberData);
                        continue;
                    }
                    // 根據提供的欄位構建SQL語句
                    string getMemberSql = $@"SELECT member_id, member_name, unit_code, non_es_code, item_no,
                                column_no, serial_code, pre_es_skill_code, es_skill_code, es_rank_code,
                                title_code, pay_date, service_code, group_code, campaign_code, rank_code,
                                supply_rank, recampaign_month, rank_date, pre_m_skill_code, m_skill_code,
                                pay_unit_code, pay_remark, bonus_code, main_bonus, work_status,
                                original_pay, corner_code, update_date, trans_code, campaign_serial,
                                volun_soldier_date, volun_sergeant_date, volun_officer_date, 
                                again_campaign_date, stop_volunteer_date, retire_date
                         FROM Army.dbo.v_member_relay 
                         WHERE member_id = @memberId";

                    SqlParameter[] getMemberPara = { new SqlParameter("@memberId", SqlDbType.VarChar) { Value = id } };

                    DataTable getMemberTb = _dbHelper.ArmyWebExecuteQuery(getMemberSql, getMemberPara);

                    if (getMemberTb != null && getMemberTb.Rows.Count != 0)
                    {
                        DataRow row = getMemberTb.Rows[0];
                        string Pay_Date = _codeToName.dateTimeTran(row["pay_date"].ToString(), "yyy年MM月dd日", true);
                        string Rank_Date = _codeToName.dateTimeTran(row["rank_date"].ToString(), "yyy年MM月dd日", true);
                        string Update_Date = _codeToName.dateTimeTran(row["update_date"].ToString(), "yyy年MM月dd日", true);
                        string Volun_Soldier_Date = _codeToName.dateTimeTran(row["volun_soldier_date"].ToString(), "yyy年MM月dd日", true);
                        string Volun_Sergeant_Date = _codeToName.dateTimeTran(row["volun_sergeant_date"].ToString(), "yyy年MM月dd日", true);
                        string Volun_Officer_Date = _codeToName.dateTimeTran(row["volun_officer_date"].ToString(), "yyy年MM月dd日", true);
                        string Again_Campaign_Date = _codeToName.dateTimeTran(row["again_campaign_date"].ToString(), "yyy年MM月dd日", true);
                        string Stop_Volunteer_Date = _codeToName.dateTimeTran(row["stop_volunteer_date"].ToString(), "yyy年MM月dd日", true);
                        string Retire_Date = _codeToName.dateTimeTran(row["retire_date"].ToString(), "yyy年MM月dd日", true);

                        // 按照你所需的欄位填充屬性
                        memberData = new RelayMultinumberRes
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
                            PayDate = Pay_Date,
                            ServiceCode = row["service_code"].ToString(),
                            GroupCode = row["group_code"].ToString(),
                            CampaignCode = row["campaign_code"].ToString(),
                            RankCode = row["rank_code"].ToString(),
                            SupplyRank = row["supply_rank"].ToString(),
                            RecampaignMonth = row["recampaign_month"].ToString(),
                            RankDate = Rank_Date,
                            PreMSkillCode = row["pre_m_skill_code"].ToString(),
                            MSkillCode = row["m_skill_code"].ToString(),
                            PayUnitCode = row["pay_unit_code"].ToString(),
                            PayRemark = row["pay_remark"].ToString(),
                            BonusCode = row["bonus_code"].ToString(),
                            MainBonus = row["main_bonus"].ToString(),
                            WorkStatus = row["work_status"].ToString(),
                            OriginalPay = row["original_pay"].ToString(),
                            CornerCode = row["corner_code"].ToString(),
                            UpdateDate = Update_Date,
                            TransCode = row["trans_code"].ToString(),
                            CampaignSerial = row["campaign_serial"].ToString(),
                            VolunSoldierDate = Volun_Soldier_Date,
                            VolunSergeantDate = Volun_Sergeant_Date,
                            VolunOfficerDate = Volun_Officer_Date,
                            AgainCampaignDate = Again_Campaign_Date,
                            StopVolunteerDate = Stop_Volunteer_Date,
                            RetireDate = Retire_Date
                        };
                    }
                    else
                    {
                        memberData = new RelayMultinumberRes
                        {
                            MemberId = id,
                            MemberName = "查無此人",
                        };
                    }
                    MultinumberList.Add(memberData);
                }

                return Ok(new { Result = "Success", WrongId = wrongId, MultinumberList });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【MultinumberSearchFile Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【MultinumberSearchFile Fail】");
            }
        }

        // 多兵號Excel匯出
        [HttpPost]
        [ActionName("MultinumberExport")]
        public async Task<IHttpActionResult> MultinumberExport([FromBody] IdNumberReq selData)
        {
            try
            {
                List<List<string>> excelData = new List<List<string>>();
                List<GeneralReq> generalReq = new List<GeneralReq>();
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
                         WHERE member_id IN ({string.Join(",", selData.IdNumber.Select(id => $"'{id}'"))})
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

                var MultinumberList = new List<object>();

                
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

                    string Pay_Date = _codeToName.dateTimeTran(row["pay_date"].ToString(), "yyy年MM月dd日", true);
                    string Rank_Date = _codeToName.dateTimeTran(row["rank_date"].ToString(), "yyy年MM月dd日", true);
                    string Update_Date = _codeToName.dateTimeTran(row["update_date"].ToString(), "yyy年MM月dd日", true);
                    string Volun_Soldier_Date = _codeToName.dateTimeTran(row["volun_soldier_date"].ToString(), "yyy年MM月dd日", true);
                    string Volun_Sergeant_Date = _codeToName.dateTimeTran(row["volun_sergeant_date"].ToString(), "yyy年MM月dd日", true);
                    string Volun_Officer_Date = _codeToName.dateTimeTran(row["volun_officer_date"].ToString(), "yyy年MM月dd日", true);
                    string Again_Campaign_Date = _codeToName.dateTimeTran(row["again_campaign_date"].ToString(), "yyy年MM月dd日", true);
                    string Stop_Volunteer_Date = _codeToName.dateTimeTran(row["stop_volunteer_date"].ToString(), "yyy年MM月dd日", true);

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
                        Pay_Date,
                        row["service_code"].ToString(),
                        row["group_code"].ToString(),
                        row["campaign_code"].ToString(),
                        row["rank_code"].ToString(),
                        row["supply_rank"].ToString(),
                        row["recampaign_month"].ToString(),
                        Rank_Date,
                        row["pre_m_skill_code"].ToString(),
                        row["m_skill_code"].ToString(),
                        row["pay_unit_code"].ToString(),
                        row["pay_remark"].ToString(),
                        row["bonus_code"].ToString(),
                        row["main_bonus"].ToString(),
                        row["work_status"].ToString(),
                        row["original_pay"].ToString(),
                        row["corner_code"].ToString(),
                        Update_Date,
                        row["trans_code"].ToString(),
                        row["campaign_serial"].ToString(),
                        Volun_Soldier_Date,
                        Volun_Sergeant_Date,
                        Volun_Officer_Date,
                        Again_Campaign_Date,
                        Stop_Volunteer_Date
                    };

                    excelData.Add(memberData);
                }

                string dateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                string excelName = "~/Report/" + dateTime + "_多兵號查詢.xlsx";
                string excelOutputPath = System.Web.Hosting.HostingEnvironment.MapPath(excelName);
                string urlPath = Request.RequestUri.GetLeftPart(UriPartial.Authority) + $"/{ConfigurationManager.AppSettings.Get("ApiPath")}/Report/";
                string excelHttpPath = string.Empty;
                bool excelResult = true;

                excelResult = _makeReport.exportMultinumberExcel(excelData, excelOutputPath, "N");

                if (excelResult)
                {
                    excelName = dateTime + "_多兵號查詢.xlsx";
                    excelHttpPath = urlPath + excelName;
                    _makeReport.checkGeneral(generalReq, selData.UserId, excelName, "多兵號名冊下載");
                }

                return Ok(new { Result = "Success", excelPath = excelHttpPath });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【MultinumberSearch Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【MultinumberSearch Fail】");
            }
        }

        // 未到多兵號匯出excel
        [HttpPost]
        [ActionName("RelayMultinumberExport")]
        public async Task<IHttpActionResult> RelayMultinumberExport([FromBody] IdNumberReq selData)
        {
            try
            {
                List<List<string>> excelData = new List<List<string>>();
                List<GeneralReq> generalReq = new List<GeneralReq>();
                // 根據提供的欄位構建SQL語句
                string getMemberSql = $@"SELECT member_id, member_name, unit_code, non_es_code, item_no,
                                column_no, serial_code, pre_es_skill_code, es_skill_code, es_rank_code,
                                title_code, pay_date, service_code, group_code, campaign_code, rank_code,
                                supply_rank, recampaign_month, rank_date, pre_m_skill_code, m_skill_code,
                                pay_unit_code, pay_remark, bonus_code, main_bonus, work_status,
                                original_pay, corner_code, update_date, trans_code, campaign_serial,
                                volun_soldier_date, volun_sergeant_date, volun_officer_date, 
                                again_campaign_date, stop_volunteer_date, retire_date
                         FROM Army.dbo.v_member_relay 
                         WHERE member_id IN ({string.Join(",", selData.IdNumber.Select(id => $"'{id}'"))})
                         ORDER BY CASE";

                int SortingWeight = 1;
                foreach (string memberId in selData.IdNumber)
                {
                    getMemberSql += " WHEN Army.dbo.v_member_relay.member_id = '" + memberId + "' THEN " + SortingWeight;
                    SortingWeight++;
                }
                getMemberSql += @" ELSE 999
                                  END;";

                DataTable getMemberTb = _dbHelper.ArmyWebExecuteQuery(getMemberSql);

                if (getMemberTb == null || getMemberTb.Rows.Count == 0)
                {
                    return Ok(new { Result = "No Member" });
                }

                var MultinumberList = new List<object>();


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

                    string Pay_Date = _codeToName.dateTimeTran(row["pay_date"].ToString(), "yyy年MM月dd日", true);
                    string Rank_Date = _codeToName.dateTimeTran(row["rank_date"].ToString(), "yyy年MM月dd日", true);
                    string Update_Date = _codeToName.dateTimeTran(row["update_date"].ToString(), "yyy年MM月dd日", true);
                    string Volun_Soldier_Date = _codeToName.dateTimeTran(row["volun_soldier_date"].ToString(), "yyy年MM月dd日", true);
                    string Volun_Sergeant_Date = _codeToName.dateTimeTran(row["volun_sergeant_date"].ToString(), "yyy年MM月dd日", true);
                    string Volun_Officer_Date = _codeToName.dateTimeTran(row["volun_officer_date"].ToString(), "yyy年MM月dd日", true);
                    string Again_Campaign_Date = _codeToName.dateTimeTran(row["again_campaign_date"].ToString(), "yyy年MM月dd日", true);
                    string Stop_Volunteer_Date = _codeToName.dateTimeTran(row["stop_volunteer_date"].ToString(), "yyy年MM月dd日", true);
                    string Retire_Date = _codeToName.dateTimeTran(row["retire_date"].ToString(), "yyy年MM月dd日", true);

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
                        Pay_Date,
                        row["service_code"].ToString(),
                        row["group_code"].ToString(),
                        row["campaign_code"].ToString(),
                        row["rank_code"].ToString(),
                        row["supply_rank"].ToString(),
                        row["recampaign_month"].ToString(),
                        Rank_Date,
                        row["pre_m_skill_code"].ToString(),
                        row["m_skill_code"].ToString(),
                        row["pay_unit_code"].ToString(),
                        row["pay_remark"].ToString(),
                        row["bonus_code"].ToString(),
                        row["main_bonus"].ToString(),
                        row["work_status"].ToString(),
                        row["original_pay"].ToString(),
                        row["corner_code"].ToString(),
                        Update_Date,
                        row["trans_code"].ToString(),
                        row["campaign_serial"].ToString(),
                        Volun_Soldier_Date,
                        Volun_Sergeant_Date,
                        Volun_Officer_Date,
                        Again_Campaign_Date,
                        Stop_Volunteer_Date,
                        Retire_Date
                    };

                    excelData.Add(memberData);
                }

                string dateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                string excelName = "~/Report/" + dateTime + "_未到多兵號查詢.xlsx";
                string excelOutputPath = System.Web.Hosting.HostingEnvironment.MapPath(excelName);
                string urlPath = Request.RequestUri.GetLeftPart(UriPartial.Authority) + $"/{ConfigurationManager.AppSettings.Get("ApiPath")}/Report/";
                string excelHttpPath = string.Empty;
                bool excelResult = true;

                excelResult = _makeReport.exportMultinumberExcel(excelData, excelOutputPath, "R");

                if (excelResult)
                {
                    excelName = dateTime + "_未到多兵號查詢.xlsx";
                    excelHttpPath = urlPath + excelName;
                    _makeReport.checkGeneral(generalReq, selData.UserId, excelName, "未到多兵號名冊下載");
                }

                return Ok(new { Result = "Success", excelPath = excelHttpPath });
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("【MultinumberSearch Fail】" + DateTime.Now.ToString() + " " + ex.Message));
                return BadRequest("【MultinumberSearch Fail】");
            }
        }

    }
}