using ArmyAPI.Commons;
using ArmyAPI.Models;
using System.Collections.Generic;
using System.Text;

namespace ArmyAPI.Data
{
    public class DB_UpdateDetail_Limits : DapperHelper
	{
		public DB_UpdateDetail_Limits() : base(BaseController._ConnectionString)
		{
		}

		public int Run(UserDetail user, dynamic menusUser, dynamic limitCodes, bool isAdmin)
		{
			string usersTableName = "Users";
			string menuUserTableName = "MenuUser";
			string limitsUserTableName = "LimitsUser";
			string limitTableName = "Limits";
			List<string> queries = new List<string>();
			List<object> parametersList = new List<object>();
            dynamic userIdObj = new { UserID = user.UserID };
			#region CommandText
			string commText = $@"
IF NOT EXISTS (SELECT 1 FROM {usersTableName} WHERE UserID = @UserID) 
BEGIN 
  SELECT -1
  RETURN 
END 

DECLARE @Rank1 VARCHAR(2) 
DECLARE @Title1 VARCHAR(4) 
DECLARE @Skill1 VARCHAR(6) 
SET @Rank1 = @RankCode 
SET @Title1 = @TitleCode 
SET @Skill1 = @SkillCode 

IF EXISTS (SELECT vm.member_id 
           FROM Army.dbo.v_member_data AS vm 
             LEFT JOIN Army.dbo.rank r ON r.rank_code = vm.rank_code 
             LEFT JOIN Army.dbo.skill s ON s.skill_code = vm.es_skill_code 
             LEFT JOIN Army.dbo.title t ON t.title_code = vm.title_code 
           WHERE vm.member_id = @UserID 
             AND LEN(TRIM(r.rank_title)) > 0) 
  BEGIN 
    SET @Rank1 = NULL 
    SET @Title1 = NULL 
    SET @Skill1 = NULL 
  END 

UPDATE {usersTableName} 
    SET [Name] = @Name, [UnitCode] = @UnitCode, [Rank] = @Rank1, [Title] = @Title1, [Skill] = @Skill1, [IPAddr1] = @IPAddr1, {(!string.IsNullOrEmpty(user.PP) && user.PP.Length == 32 ? "[Password] = @PP, " : "")}[Email] = @Email, [PhoneMil] = @PhoneMil, [Phone] = @Phone, [TGroups] = @TGroups, [ApplyDate] = GETDATE(), [Reason] = @Reason {(isAdmin ? ", [IPAddr2] = @IPAddr2, [Process] = @Process, [Review] = @Review, [Outcome] = @Outcome " : "")}
WHERE [UserID] = @UserID 
";
			queries.Add(commText);
            parametersList.Add(user);

            // Outcome 0 駁回 1 同意 2 臨時用
            // Status  NULL：(註冊後，未填人事權限申請)
            //           -3：駁回
            //           -2：停用(登入距上一次登入超過2個月)
            //           -1：申請中(註冊後，填完人事權限申請)
            //            0：審核中
            //            1：通過
            // 更新 ApplyDate
            commText = $@"
UPDATE {usersTableName} 
    SET [Status] = CASE WHEN [Outcome] IS NULL THEN 0 WHEN [Outcome] = 0 THEN -3 WHEN [Outcome] = 1 THEN 1 END  
WHERE [UserID] = @UserID 
";
			queries.Add(commText);
            parametersList.Add(userIdObj);

            commText = $@"
DELETE FROM {menuUserTableName} 
WHERE 1=1 
  AND [UserID] = @UserID 
";
            queries.Add(commText);
            parametersList.Add(menusUser);

            commText = $@"
IF LEN(@MenuIndexs) > 0 
BEGIN 
  INSERT INTO {menuUserTableName} 
    SELECT DISTINCT value, @UserID FROM STRING_SPLIT(@MenuIndexs, ',') 
END 
";
			queries.Add(commText);
			parametersList.Add(menusUser);

			commText = $@"
DELETE FROM {limitsUserTableName} 
WHERE 1=1 
  AND [UserID] = @UserID
";
			queries.Add(commText);
            parametersList.Add(userIdObj);

			commText = $@"
INSERT INTO {limitsUserTableName} 
    SELECT DISTINCT L.[LimitCode], @UserID FROM {limitTableName} L CROSS APPLY STRING_SPLIT(@LimitCodes, ',') AS SplitCodes WHERE LEFT(L.[LimitCode], 6) = SplitCodes.value
";
			queries.Add(commText);
			parametersList.Add(limitCodes);
			#endregion CommandText




            return ExecuteTransaction(queries, parametersList);
		}
	}
}