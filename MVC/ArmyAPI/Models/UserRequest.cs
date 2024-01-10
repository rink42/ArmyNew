﻿using NPOI.SS.Formula;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArmyAPI.Models
{
    public class UserReq
    {
        public string member_id { get; set; }

        public string member_name { get; set; }
    }

    public class BatchDeleteRegisterReq
    {
        public string FormType { get; set; }

        public List<string> MemberId { get; set; }
    }
    public class RegisterReq
    {
        public string PrimaryUnit { get; set; }

        public string CurrentPosition { get; set; }

        public string Name { get; set; }

        public string IdNumber { get; set; }

        public string Branch { get; set; }

        public string Rank { get; set; }

        public string OldRankCode { get; set; }

        public string NewRankCode { get; set; }

        public DateTime? EffectDate { get; set; }

        public string FormType { get; set; }

        public string OriginalId { get; set; }

        public string OriginalForm { get; set; }
    }

    public class UpdateSignatureReq
    {
        public string Name { get; set; } = null;

        public string JobTitle { get; set; } = null;

        public string OrignName { get; set; } = null;
    }

    public class SaveCaseReq
    {
        public string CreateMember {get; set; }

        public string CreateMemberId { get; set; }
        
        public string FormType { get; set; }

        public string[] IdNumber { get; set; }
    }

    public class CaseExcelReq
    {
        public string PrimaryUnit { get; set; }

        public string CurrentPosition { get; set; }

        public string Name { get; set; }

        public string IdNumber { get; set; }

        public string Branch { get; set; }

        public string Rank { get; set; }

        public string OldRankCode { get; set; }

        public string NewRankCode { get; set; }

        public string EffectDate { get; set; }

        public string Year { get; set; }

        public string Month { get; set; }

        public string Day { get; set; }

        public string FormType { get; set; }

        public string CaseId { get; set; }
    }

    public class ReprintCaseReq
    {
        public string OldCaseId { get; set;}        

        public string CreateMember { get; set; }

        public string CreateMemberId { get; set; }

        public string[] IdNumber { get; set; }
    }

    public class CreateCodeTableReq
    {
        public string TableTitle { get; set; }

        public string ChineseTitle { get; set; }
    }

    public class UpdateCodeDataReq
    {
        public string OrignCode { get; set; }

        public string Code { get;set; }

        public string Memo { get; set; }
    }

    public class UpdateCodeReq
    {
        public string ChineseTitle { get; set; }

        public List<UpdateCodeDataReq> DataList { get; set; }
    }

    public class EditCodeDataReq
    {
        public string Code { get; set; }

        public string Memo { get; set; }
    }

    public class EditCodeReq
    {
        public string ChineseTitle { get; set; }

        public List<EditCodeDataReq> DataList { get; set; }
    }

    public class HierarchicalDataReq
    {
        public string OldSupplyPoint { get; set; }       
        public string OldSupplyRank { get; set; }
        public string OldRankCode { get; set; }
        public string NewSupplyPoint { get; set; }
        public string NewSupplyRank { get; set; }
        public string NewRankCode { get; set; }
    }
    public class EditHierarchicalReq
    {
        public List<HierarchicalDataReq> DataList { get; set; }
    }

    public class advancedSearchWhereReq
    {
        public List<string> member_name { get; set; } = new List<string>();

        public List<string> unit_code { get; set; } = new List<string>();

        public List<string> rank_date { get; set; } = new List<string>();

        public List<string> es_skill_code { get; set; } = new List<string>();

        public List<string> service_code { get; set; } = new List<string>();

        public List<string> trans_code { get; set; } = new List<string>();

        public List<string> common_educ_code { get; set; } = new List<string>();

        public List<string> Sex { get; set; } = new List<string>();

        public List<string> Birthday { get; set; } = new List<string>();

        public List<string> group_code { get; set; } = new List<string>();

        public List<string> m_skill_code { get; set; } = new List<string>();

        public List<string> title_code { get; set; } = new List<string>();

        public List<string> campaign_code { get; set; } = new List<string>();

        public List<string> military_educ_code { get; set; } = new List<string>();

        public List<string> campaign_date { get; set; } = new List<string>();

        public List<string> blood_type { get; set; } = new List<string>();

        public List<string> salary_date { get; set; } = new List<string>();

        public List<string> rank_code { get; set; } = new List<string>();

        public List<string> es_rank_code { get; set; } = new List<string>();

        public List<string> pay_date { get; set; } = new List<string>();

        public List<string> non_es_code { get; set; } = new List<string>();

        public List<string> school_code { get; set; } = new List<string>();

        public List<string> supply_rank { get; set; } = new List<string>();

        public List<string> campaign_serial { get; set; } = new List<string>();

        public List<string> recampaign_month { get; set; } = new List<string>();

        public List<string> AAAAA { get; set; } = new List<string>();        //民間專長
    }

    public class advSearchConditionReq
    {
        public string ConditionName { get; set; }

        public List<string> ConditionValue { get; set; }
    }

    public class advancedSearchMemberReq
    {        
        public bool Performance { get; set; }

        public List<string> ColumnName { get; set; }

        public List<advSearchConditionReq> SearchData { get; set; }
    }

    public class advExcelDataReq
    {
        public string UserId { get; set; }

        public List<string> MemberId { get; set; }

        public List<string> ColumnName { get; set; }

        public List<List<string>> Data { get; set; }        
    }

    public class GeneralReq
    {
        public string GeneralId { get; set;}

        public string GeneralName { get; set;}

        public string GeneralRank { get; set; }
    }

    public class IdNumberReq
    {
        public string UserId { get; set; }

        public List<string> IdNumber { get; set; }        
    }

    public class RetireMemberReq
    {
        public string Name { get; set; } = null;

        public string Id { get; set; }

        public string Unit { get; set; } = null;

        public string Distinction { get; set; }

        public string Analyze { get; set; }
    }

    public class RetireReasonReq
    {
        public List<RetireMemberReq> Member { get; set; }
    }
}