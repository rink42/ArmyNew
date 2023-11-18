using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;

namespace ArmyAPI.Services
{
    public class CodetoName
    {
        private readonly DbHelper _dbHelper;

        public CodetoName()
        {
            _dbHelper = new DbHelper();
        }

        // 日期轉換
        public string dateTimeTran(string dateTime, string dateFormat, bool country = false)
        {
            try
            {
                string date = string.Empty;

                if (dateTime == null || dateTime == "")
                {
                    return " ";
                }
                else
                {
                    //設置西元轉民國
                    CultureInfo Tocalendar = new CultureInfo("zh-TW");
                    Tocalendar.DateTimeFormat.Calendar = new TaiwanCalendar();
                    

                    if (country == true)
                    {
                        date = DateTime.Parse(dateTime).ToString(dateFormat, Tocalendar);
                    }
                    else
                    {
                        date = DateTime.Parse(dateTime).ToString(dateFormat);
                    }
                }

                return date;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Date Time Transform Fail. {0}", ex.ToString()));
                return " ";
            }
            
        }

        // 字串日期分割
        public string stringToDate(string date) 
        {
            string dateString = string.Empty;
            if(string.IsNullOrEmpty(date))
            {
                return date;
            }

            switch (date.Length)
            {
                case 6:
                    dateString = "0" + date;
                    break;
                case 7:
                    dateString = date;
                    break;
                default:
                    return date;                    
            }
            
           
            string year = dateString.Substring(0, 3);
            string month = dateString.Substring(3, 2);
            string day = dateString.Substring(5, 2);

            string resultData = $"{year}年{month}月{day}日";
            return resultData;
        }
        // 階級代碼
        public string rankName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                rank_title
                            FROM
                                rank
                            WHERE
                                rank_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["rank_title"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["rank_title"].ToString().Trim();
                }
                
                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Rank Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 職稱代碼
        public string titleName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                title_name
                            FROM
                                title
                            WHERE
                                title_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["title_name"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["title_name"].ToString().Trim();
                }
                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Title to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 役別代碼
        public string campaignName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                campaign_desc
                            FROM
                                memb_campaign_code
                            WHERE
                                campaign_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["campaign_desc"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["campaign_desc"].ToString().Trim();
                }
                
                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Campaign Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 官科代碼
        public string groupName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                group_title
                            FROM
                                tgroup
                            WHERE
                                group_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["group_title"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["group_title"].ToString().Trim();
                }
                
                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Group Code to Name. {0}", ex.ToString()));
                return code;
            }  
        }

        // 專長代碼
        public string skillName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                skill_desc
                            FROM
                                skill
                            WHERE
                                skill_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["skill_desc"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["skill_desc"].ToString().Trim();
                }
                
                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Skill Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 專長授予代碼
        public string skillTypeName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                GetTypeTitle
                            FROM
                                skill_GetType
                            WHERE
                                GetTypeCode = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["GetTypeTitle"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["GetTypeTitle"].ToString().Trim();
                }

                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Skill GetType Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 軍種代碼
        public string serviceName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                service_name
                            FROM
                                service
                            WHERE
                                service_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["service_name"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["service_name"].ToString().Trim();
                }
                
                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Service Code to Name. {0}", ex.ToString()));
                return code;
            }

        }

        // 異動代號
        public string transName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                memo
                            FROM
                                memb_trans_code
                            WHERE
                                trans_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["memo"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["memo"].ToString().Trim();
                }

                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Trans Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 最高教育代碼(棄用??)
        public string militaryName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                military_educ_desc
                            FROM
                                military_educ
                            WHERE
                                military_educ_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["military_educ_desc"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["military_educ_desc"].ToString().Trim();
                }

                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Military Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 單位代號
        public string unitName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                unit_title
                            FROM
                                v_mu_unit
                            WHERE
                                unit_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["unit_title"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["unit_title"].ToString().Trim();
                }
                
                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Unit Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 考績績等代碼
        public string perfName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                perform_name
                            FROM
                                perf_code
                            WHERE
                                perform_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["perform_name"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["perform_name"].ToString().Trim();
                }
                
                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Perform Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 班別對照檔
        public string className(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                class_name
                            FROM
                                educ_class
                            WHERE
                                class_code  = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["class_name"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["class_name"].ToString().Trim();
                }
                                
                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Class Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 教育程度對照檔
        public string educName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                educ_name
                            FROM
                                educ_code
                            WHERE
                                educ_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["educ_name"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["educ_name"].ToString().Trim();
                }
                
                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Educ Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 出國進修國家
        public string countryName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                country_desc
                            FROM
                                educ_country
                            WHERE
                                country_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["country_desc"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["country_desc"].ToString().Trim();
                }
                
                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Country Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 教育科系對照檔
        public string disciplineName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                discipline_desc
                            FROM
                                educ_discipline
                            WHERE
                                discipline_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["discipline_desc"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["discipline_desc"].ToString().Trim();
                }
                
                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Discipline Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 教育學校對照檔
        public string schoolName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                school_desc
                            FROM
                                educ_school
                            WHERE
                                school_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["school_desc"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["school_desc"].ToString().Trim();
                }
                
                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("School Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 教育學校對照檔
        public string eduControlName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                control_desc
                            FROM
                                education_control_code
                            WHERE
                                control_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["control_desc"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["control_desc"].ToString().Trim();
                }

                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Edu Control Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 獎懲性質
        public string attribName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                enc_attrib_desc 
                            FROM
                                enco_attrib
                            WHERE
                                enc_attrib_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["enc_attrib_desc"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["enc_attrib_desc"].ToString().Trim();
                }

                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Enc Attrib Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 獎懲事由
        public string encoGroupName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                enc_group_desc 
                            FROM
                                enco_group
                            WHERE
                                enc_group_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["enc_group_desc"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["enc_group_desc"].ToString().Trim();
                }

                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Enc Group Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 獎章種類
        public string metalName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                enc_metal_desc 
                            FROM
                                enco_metal
                            WHERE
                                enc_metal_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["enc_metal_desc"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["enc_metal_desc"].ToString().Trim();
                }

                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Enc Metal Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 勳獎章代號
        public string reasonName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                jh_name
                            FROM
                                enco_reason
                            WHERE
                                jh_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["jh_name"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["jh_name"].ToString().Trim();
                }

                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Reason Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 獎懲運用原因
        public string usedName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                enc_used_desc
                            FROM
                                enco_used
                            WHERE
                                enc_used_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["enc_used_desc"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["enc_used_desc"].ToString().Trim();
                }

                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Enc Used Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 役期代碼
        public string controlName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                control_name
                            FROM
                                control_code
                            WHERE
                                control_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["control_name"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["control_name"].ToString().Trim();
                }

                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("control Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 任職代碼
        public string appointmentName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                appointment_desc
                            FROM
                                appointment_code
                            WHERE
                                appointment_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["appointment_desc"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["appointment_desc"].ToString().Trim();
                }

                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Appointment Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 資格考試證照級別代碼
        public string certGradeName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                certificate_grade_name
                            FROM
                                certificate_grade
                            WHERE
                                certificate_grade_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["certificate_grade_name"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["certificate_grade_name"].ToString().Trim();
                }

                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Certificate Grade Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 職系別/考試項目/證照類別代碼
        public string certJobName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                certificate_job_name
                            FROM
                                certificate_job
                            WHERE
                                certificate_job_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["certificate_job_name"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["certificate_job_name"].ToString().Trim();
                }

                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Certificate Job Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 資格考試證照分類代碼
        public string certSortName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                certificate_sort_name
                            FROM
                                certificate_sort
                            WHERE
                                certificate_sort = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["certificate_sort_name"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["certificate_sort_name"].ToString().Trim();
                }

                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Certificate Sort Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 出生地代碼
        public string locateName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                locate_name
                            FROM
                                memb_locate
                            WHERE
                                locate_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["locate_name"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["locate_name"].ToString().Trim();
                }

                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Locate Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 縣市代碼
        public string cityName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                city_name
                            FROM
                                memb_city
                            WHERE
                                city_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["city_name"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["city_name"].ToString().Trim();
                }

                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("City Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        // 編外因素
        public string esName(string code, bool intact = true)
        {
            try
            {
                string Sql = @"
                            SELECT 
                                non_es_name
                            FROM
                                non_es_code
                            WHERE
                                non_es_code = @Code";
                SqlParameter[] Parameter = { new SqlParameter("@Code", SqlDbType.VarChar) { Value = code } };

                DataTable TB = _dbHelper.ArmyExecuteQuery(Sql, Parameter);
                if (TB == null || TB.Rows.Count == 0)
                {
                    return code;
                }

                string Result = string.Empty;
                if (intact)
                {
                    Result = code + " - " + TB.Rows[0]["non_es_name"].ToString().Trim();
                }
                else
                {
                    Result = TB.Rows[0]["non_es_name"].ToString().Trim();
                }

                return Result;
            }
            catch (Exception ex)
            {
                WriteLog.Log(String.Format("Non Es Code to Name. {0}", ex.ToString()));
                return code;
            }
        }

        public DataTable Transformer(DataTable data, List<string> column, bool sex = false) 
        {
            DataTable tranTable = new DataTable();
            foreach (string col in column) 
            {
                tranTable.Columns.Add(col);                
            }
            if(sex == true)
            {
                tranTable.Columns.Add("sex");
            }

            foreach(DataRow row in data.Rows)
            {
                DataRow tranRow = tranTable.NewRow();
                foreach(string col in column)
                {
                    switch (col)
                    {
                        case "unit_code": // 單位代號
                            tranRow[col] = unitName(row[col].ToString());
                            break;
                        case "rank_date": // 任本階日期
                            tranRow[col] = dateTimeTran(row[col].ToString(), "yyyy/MM/dd");
                            break;
                        case "es_skill_code": // 編制專長代碼
                            tranRow[col] = skillName(row[col].ToString());
                            break;
                        case "service_code": // 軍種
                            tranRow[col] = serviceName(row[col].ToString());
                            break;
                        case "trans_code": // 異動代號
                            tranRow[col] = transName(row[col].ToString());
                            break;
                        case "common_educ_code": // 最高普通教育
                            tranRow[col] = educName(row[col].ToString());
                            break;
                        case "birthday": // 生日
                            tranRow[col] = dateTimeTran(row[col].ToString(), "yyyy/MM/dd");
                            break;
                        case "group_code": // 官科
                            tranRow[col] = groupName(row[col].ToString());
                            break;
                        case "m_skill_code": // 本人主專長代碼
                            tranRow[col] = skillName(row[col].ToString());
                            break;
                        case "title_code": // 職稱
                            tranRow[col] = titleName(row[col].ToString());
                            break;
                        case "campaign_code": // 役別代碼
                            tranRow[col] = campaignName(row[col].ToString());
                            break;
                        case "military_educ_code": // 最高軍事教育
                            tranRow[col] = educName(row[col].ToString());
                            break;
                        case "campaign_date": // 入伍日期
                            tranRow[col] = dateTimeTran(row[col].ToString(), "yyyy/MM/dd");
                            break;
                        case "salary_date": // 任官日期
                            tranRow[col] = dateTimeTran(row[col].ToString(), "yyyy/MM/dd");
                            break;
                        case "rank_code": // 階級
                            tranRow[col] = rankName(row[col].ToString());
                            break;
                        case "es_rank_code": // 編階
                            tranRow[col] = rankName(row[col].ToString());
                            break;
                        case "pay_date": // 任本職日期
                            tranRow[col] = dateTimeTran(row[col].ToString(), "yyyy/MM/dd");
                            break;
                        case "non_es_code": // 編外因素
                            tranRow[col] = esName(row[col].ToString());
                            break;
                        case "school_code": // 普通教育學校
                            tranRow[col] = schoolName(row[col].ToString());
                            break;
                        case "recampaign_month": // 回役晉支月
                            tranRow[col] = dateTimeTran(row[col].ToString(), "yyyy/MM/dd");
                            break;
                        default:
                            // 默認處理
                            tranRow[col] = row[col].ToString();
                            break;
                    }
                }
                if (sex == true)
                {
                    tranRow["sex"] = row["Sex"].ToString();
                }
                tranTable.Rows.Add(tranRow);
            }
            return tranTable;
        }
    }
}