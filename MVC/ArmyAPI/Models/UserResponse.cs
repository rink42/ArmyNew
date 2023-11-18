using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Data;

namespace ArmyAPI.Models
{
    public class RegisterRes
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

        public string FormType { get; set; }

        public string CheckMemberResult { get; set; }

        public bool InsertResult { get; set; }
    }

    public class MemRes
    {
        public string Result { get; set; }

        public string MemberId { get; set; }

        public string MemberName { get; set; }
    }

    public class SaveCaseRes
    {
        public bool InsertResult { get; set; }

        public bool DelResult { get; set; }

        public string CaseId { get; set; }

        public string CaseName { get; set; }

        public string MemberId { get; set; }
    }

    public class SaveRegRes
    {
        public string CheckMemberResult { get; set; }

        public bool InsertResult { get; set; }

        public string MemberId { get; set; }

        public string MemberName { get; set; }
    }

    public class TransCodeRes
    {
        public string TransType { get; set; }

        public string TransTitle { get; set; }

        public string TransCode { get; set; }

        public string Memo { get; set; }
    }

    public class CodeTableRes
    {
        public string Result { get; set; }

        public string Table { get; set; }

        public List<string> Columns { get; set; }

        public DataTable codeTable { get; set; }
    }

    public class CreateCodeTbRes
    {
        public bool Result { get; set; }

        public string Code { get; set; }
    }

    public class HierarchicalRes
    {
        public string MemberId { get; set; } = string.Empty;

        public string MemberName { get; set; } = string.Empty;

        public string OldRankTitle { get; set; }

        public string OldSupplyRank { get; set; }

        public string OldSupplyPoint { get; set; }

        public string NewRankTitle { get; set; }

        public string NewSupplyRank { get; set; }

        public string NewSupplyPoint { get; set; }

        public string Massage { get; set; } = string.Empty;

    }

    public class memberDetailedRes
    {
        public string MemberName { get; set; }

        public string MemberId { get; set; }

        public string BloodType { get; set; }

        public string Birthday { get; set; }

        public string GroupCode { get; set; }

        public string RankCode { get; set; }

        public string SupplyRank { get; set; }

        public string MSkillCode { get; set; }

        public string GroupSkill { get; set; } = string.Empty;

        public string FirstSkill { get; set; } = string.Empty;

        public string SecondSkill { get; set; } = string.Empty;

        public string ThirdSkill { get; set; } = string.Empty;

        public string SalaryDate { get; set; }

        public string RankDate { get; set; }

        public string ServiceCode { get; set; }

        public string UnPromoteCode { get; set; }

        public string UnitName { get; set; }

        public string UnitCode { get; set; }

        public string PayUnitCode { get; set; }

        public string CornerCode { get; set; }

        public string EsRankCode { get; set; }

        public string TitleCode { get; set; }

        public string EsSkillCode { get; set; }

        public string PayDate { get; set; }

        public string CampaignCode { get; set; }

        public string CampaignSerial { get; set; }

        public string CampaignDate { get; set; }

        public string NonEsCode { get; set; }

        public string GroupNo { get; set; }

        public string ColumnNo { get; set; }

        public string SerialCode { get; set; }

        public string TransCode { get; set; }

        public string BasicMilitaryCode { get; set; }

        public string SchoolCode { get; set; }

        public string ClassCode { get; set; }

        public string MilitaryEducCode { get; set; }

        public string HighSchoolCode { get; set; }

        public string HighClassCode { get; set; }

        public string CommonEducCode { get; set; }

        public string IqScore { get; set; }

        public string LocalCode { get; set; }

        public string ResidenceAddress { get; set; }

        public string VolunOfficerDate { get; set; }

        public string VolunSergeantDate { get; set; }

        public string VolunSoldierDate { get; set; }
        
        public string StopVolunteerDate { get;set; }

        public string LocalMark { get;set; }

        public string RetireDate { get;set; }
    }

    public class PQPMRes
    {
        public string MemberId { get; set; }

        public string MemberName { get; set; }

        public string CampaignSerial { get; set; }

        public string UnitCode { get; set; }

        public string GroupCode { get; set; }

        public string SalaryDate { get; set; }

        public string Birthday { get; set; }

        public string RankCode { get; set; }

        public string CommonEducCode { get; set; }

        public string LocalCode { get; set; }

        public string RankDate { get; set; }

        public string BasicEdu { get; set; }

        public string NonEsCode { get; set; }

        public string SupplyRank { get; set; }

        public string MilitaryEducCode { get; set; }

        public string EsRankCode { get; set; }

        public string PayUnitCode { get; set; }

        public string BloodType { get; set; }

        public string EsSkillCode { get; set; }

        public string PayRemark { get; set; }

        public string IqScore { get; set; }

        public string TitleCode { get; set; }

        public string BonusCode { get; set; }

        public string UnPromoteCode { get; set; }

        public string PayDate { get; set; }

        public string EsNumber { get; set; }

        public string OriginalPay { get; set; }

        public string CampaignCode { get; set; }

        public string MSkillCode { get; set; }

        public string CornerCode { get; set; }

        public string ServiceCode { get; set; }

        public string WorkStatus { get; set; }

        public string RecampaignMonth { get; set; }

        public string UpdateDate { get; set; }

        public string TransCode { get; set; }

        public string MainBonus { get; set; }

        public string VolunOfficerDate { get; set; }

        public string VolunSergeantDate { get; set; }

        public string VolunSoldierDate { get; set; }

        public string AgainCampaignDate { get; set; }

        public string StopVolunteerDate { get; set; }

        public string LocalMark { get; set; }

        public string RetireDate { get; set; }
    }

    public class experienceRes
    {
        public string UnitCode { get; set; }
        public string RankCode { get; set; }
        public string TitleCode { get; set; }
        public string EsRankCode { get; set; }
        public string SkillCode { get; set; }
        public string EffectDate { get; set; }
        public string DocNo { get; set; }
        public string DocDate { get; set; }
        public string NonEsCode { get; set; }
        public string TransCode { get; set; }
    }

    public class PerformanceRes
    {
        public string PYear { get; set; }

        public string PerformCode { get; set; }

        public string IdeologyCode { get; set; }

        public string QualityCode { get; set; }

        public string PotentialCode { get; set; }

        public string WorkPerformCode { get; set; }

        public string BodyCode { get; set; }

        public string KnowledgeCode { get; set; }

        public string PerformRank { get; set; }

        public string TotalRank { get; set; }
    }

    public class EducationRes
    {
        public string CountryCode { get; set; }

        public string SchoolCode { get; set; }

        public string DisciplineCode { get; set; }

        public string ClassCode { get; set; }

        public string PeriodNo { get; set; }

        public string EducCode { get; set; }

        public string StudyDate { get; set; }

        public string GraduateDate { get; set; }

        public string ClassmateAmt { get; set; }

        public string GraduateScore { get; set; }

        public string GraduateRank { get; set; }

        public string ThesisScore { get; set; }
    }

    public class EncourageRes 
    {
        public string UnitCode { get; set; }

        public string EncUnitCode { get; set; }

        public string RankCode { get; set; }

        public string DocDate { get; set; }

        public string DocNo { get; set; }

        public string EncReasonCode { get; set; }

        public string EncGroup { get; set; }

        public string DocItem { get; set; }

        public string EncPointIdent { get; set; }

        public string EncCancelDate { get; set; }

        public string EncCancelDoc { get; set; }

        public string UnitName { get; set; }

        public string DocCh { get; set; }

        public string EncReasonCh { get; set; }
    }

    public class SkillRes 
    {
        public string EsSkillCode { get; set; }
        public string RankCode { get;set; }
        public string CommandSkillCode { get; set; }
        public string ComRankCode { get; set; }
        public string Skill1Code { get; set; }
        public string Skill1RankCode { get; set; }
        public string Skill2Code { get; set; }
        public string Skill2RankCode { get; set; }
        public string Skill3Code { get; set; }
        public string Skill3RankCode { get; set; }
        public string UnitCode { get; set; }
        public string DocCh { get; set; }
        public string DocDate { get; set; }
        public string EffectDate { get; set; }
        public string TransCode { get; set; }
        public string TransDate { get; set; }
        public string GetTypeA { get; set; }
    }

    public class SupplyRes
    {
        public string OldRankCode { get; set; }
        public string OldSupplyRank { get; set; }
        public string NewRankCode { get; set; }
        public string NewSupplyRank { get; set; }
        public string ApproveUnit { get; set; }
        public string EffectDate { get; set; }
        public string DocDate { get; set; }
        public string DocNo { get; set; }
        public string DocCh { get; set; }
    }

    public class RetiredateRes 
    {
        public string ControlCode { get; set; }
        public string SubtractDay { get; set; }
        public string ApproveUnit { get; set; }
        public string ApvStartDate { get; set; }
        public string DocCh { get; set; }
        public string DocNo { get; set; }
        public string DocDate { get; set; }
        public string ApvEncDate { get; set; }
        public string ControlDate { get; set; }
    }

    public class ExamRes
    {
        public string ExamCode { get; set; }
        public string ExamDate { get; set; }
        public string Score { get; set; }
        public string ExamLevel { get; set; }
        public string ApproveUnit { get; set; }
        public string DocDate { get; set; }
        public string DocNo { get; set; }
        public string DocCh { get; set; }
    }

    public class AppointmentRes
    {
        public string ClassCode { get; set; }
        public string AppointmentDate { get; set; }
        public string NumberA { get; set; }
        public string NewServiceCode { get; set; }
        public string NewRankCode { get; set; }
        public string NewGroupCode { get; set; }
        public string OldServiceCode { get; set; }
        public string OldRankCode { get; set; }
        public string OldGroupCode { get; set; }
        public string UnitCode { get; set; }
        public string EffectDate { get; set; }
    }

    public class EduControlRes
    {
        public string ControlCode { get; set; }
        public string CountryCode { get; set; }
        public string SchoolCode { get; set; }
        public string DisciplineCode { get; set; }
        public string ClassCode { get; set; }
        public string EducCode { get; set; }
        public string StudyDate { get; set; }
        public string GraduateDate { get; set; }
        public string PeriodNo { get; set; }
        public string YearClass { get; set; }
        public string ControlStatus { get; set; }
        public string DocDate { get; set; }
        public string DocNo { get; set; }
        public string DocCh { get; set; }
        public string LeaveDate { get; set; }
        public string ApproveUnit { get; set; }
    }

    
    public class CertificateRes
    {
        public string CertificateSort { get; set; }
        public string CertificateJobCode { get; set; }
        public string CertificateGradeCode { get; set; }
        public string GetDate { get; set; }
        public string CertificateNo { get; set; }
        public string ApproveUnit { get; set; }
        public string DocDate { get; set; }
        public string DocNo { get; set; }
        public string DocCh { get; set; }
    }

    public class WritingRes
    {
        public string TitleHeading { get; set; }
        public string Publisher { get; set; }
        public string JobCode { get; set; }
        public string WritingsCode { get; set; }
        public string PressDate { get; set; }
        public string ApproveUnit { get; set; }
        public string DocDate { get; set; }
        public string DocNo { get; set; }
        public string DocCh { get; set; }
    }

    public class BuyRes
    {
        public string StartEffectDate { get; set; }
        public string EndEffectDate { get; set; }
        public string ApproveDocDate { get; set; }
        public string ApproveDocNo { get; set; }
        public string DocDate { get; set; }
        public string DocNo { get; set; }
        public string UpdateDate { get; set; }
    }

    public class ExitRes
    {        
        public string ApproveUnit { get; set; }
        public string BelongUnit { get; set; }
        public string DocDate { get; set; }
        public string DocNo { get; set; }
        public string ApvOutDate { get; set; }
        public string ApvBackDate { get; set; }
        public string Goal { get; set; }
        public string ReApproveDate { get; set; }
        public string ReApproveMk { get; set; }
        public string TranoutMk { get; set; }
        public string RejectMk { get; set; }
    }

    public class advancedCodeRes
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string CodeName { get; set; }
    }

    public class CaseListRes
    {
        public string CaseId { get; set; }

        public string CaseName { get; set; }

        public string CreateMember { get; set; }

        public string MemberId { get; set; }

        public string HostUrl { get; set; }

        public string PdfName { get; set; }

        public string ExcelName { get; set; }

        public string CreateDate { get; set; }
    }

    public class CaseRegisterRes
    {
        public string CaseId { get; set; }

        public string CaseName { get; set; }

        public string PrimaryUnit { get; set; } = null;

        public string CurrentPosition { get; set; } = null;

        public string Name { get; set; } = null;

        public string IdNumber { get; set; }

        public string Branch { get; set; } = null;

        public string Rank { get; set; } = null;

        public string OldRankCode { get; set; } = null;

        public string NewRankCode { get; set; } = null;

        public string EffectDate { get; set; }

        public string FormType { get; set; }
    }
}