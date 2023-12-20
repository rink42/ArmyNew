using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using ArmyAPI.Commons;
using ArmyAPI.Models;
using static ArmyAPI.Data.MsSqlDataProvider.DB_Tableau;

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
			StringBuilder sb = new StringBuilder();
			List<string> queries = new List<string>();
			List<object> parametersList = new List<object>();
			#region CommandText
			sb.AppendLine($"IF NOT EXISTS (SELECT 1 FROM {usersTableName} WHERE UserID = @UserID) ");
			sb.AppendLine("BEGIN ");
			sb.AppendLine("  SELECT -1");
			sb.AppendLine("  RETURN ");
			sb.AppendLine("END ");

			sb.AppendLine("DECLARE @Rank1 VARCHAR(2) ");
			sb.AppendLine("DECLARE @Title1 VARCHAR(4) ");
			sb.AppendLine("DECLARE @Skill1 VARCHAR(6) ");
			sb.AppendLine("SET @Rank1 = @RankCode ");
			sb.AppendLine("SET @Title1 = @TitleCode ");
			sb.AppendLine("SET @Skill1 = @SkillCode ");

			sb.AppendLine("IF EXISTS (SELECT vm.member_id ");
			sb.AppendLine("           FROM Army.dbo.v_member_data AS vm ");
			sb.AppendLine("             LEFT JOIN Army.dbo.rank r ON r.rank_code = vm.rank_code ");
			sb.AppendLine("             LEFT JOIN Army.dbo.skill s ON s.skill_code = vm.es_skill_code ");
			sb.AppendLine("             LEFT JOIN Army.dbo.title t ON t.title_code = vm.title_code ");
			sb.AppendLine("           WHERE vm.member_id = @UserID ");
			sb.AppendLine("             AND LEN(TRIM(r.rank_title)) > 0) ");
			sb.AppendLine("  BEGIN ");
			sb.AppendLine("    SET @Rank1 = NULL ");
			sb.AppendLine("    SET @Title1 = NULL ");
			sb.AppendLine("    SET @Skill1 = NULL ");
			sb.AppendLine("  END ");

			sb.AppendLine($"UPDATE {usersTableName} ");
			sb.Append("    SET [Name] = @Name, [Rank] = @Rank1, [Title] = @Title1, [Skill] = @Skill1, [IPAddr1] = @IPAddr1, [Email] = @Email, [PhoneMil] = @PhoneMil, [Phone] = @Phone, [TGroups] = @TGroups, [ApplyDate] = GETDATE(), [Reason] = @Reason ");
			if (isAdmin)
				sb.AppendLine(", [IPAddr2] = @IPAddr2, [Process] = @Process, [Review] = @Review, [Outcome] = @Outcome ");
			else
				sb.Append("\n ");
			sb.AppendLine("WHERE [UserID] = @UserID ");

			queries.Add(sb.ToString());
			sb.Length = 0;
			// Outcome 0 駁回 1 同意 2 臨時用
			// Status  NULL：(註冊後，未填人事權限申請)
			//           -3：駁回
			//           -2：停用(登入距上一次登入超過2個月)
			//           -1：申請中(註冊後，填完人事權限申請)
			//            0：審核中
			//            1：通過
			// 更新 ApplyDate
			sb.AppendLine($"UPDATE {usersTableName} ");
			sb.AppendLine("    SET [Status] = CASE WHEN [Outcome] IS NULL THEN 0 WHEN [Outcome] = 0 THEN -3 WHEN [Outcome] = 1 THEN 1 END  ");
			sb.AppendLine("WHERE [UserID] = @UserID ");

			queries.Add(sb.ToString());
			sb.Length = 0;

			sb.AppendLine($"DELETE FROM {menuUserTableName} ");
			sb.AppendLine("WHERE 1=1 ");
			sb.AppendLine("  AND [UserID] = @UserID ");

			sb.AppendLine("IF LEN(@MenuUser) > 0 ");
			sb.AppendLine("BEGIN ");
			sb.AppendLine($"  INSERT INTO {menuUserTableName} ");
			sb.AppendLine($"      SELECT value, @UserID FROM STRING_SPLIT(@MenuUser, ',') ");
			sb.AppendLine("END ");

			queries.Add(sb.ToString());
			sb.Length = 0;

			sb.AppendLine($"DELETE FROM {limitsUserTableName} ");
			sb.AppendLine("WHERE 1=1 ");
			sb.AppendLine("  AND [UserID] = @UserID ");

			sb.AppendLine($"INSERT INTO {limitsUserTableName} ");
			sb.AppendLine($"    SELECT L.[LimitCode], @UserID FROM {limitTableName} L CROSS APPLY STRING_SPLIT(@LimitCodes, ',') AS SplitCodes WHERE L.[LimitCode] LIKE SplitCodes.value + '%' ");

			queries.Add(sb.ToString());
			sb.Length = 0;
			#endregion CommandText

			parametersList.Add(user);

			dynamic updateStatus = new { UserID = user.UserID };
			parametersList.Add(updateStatus);

			parametersList.Add(menusUser);

			parametersList.Add(limitCodes);

			return ExecuteTransaction(queries, parametersList);
		}
	}
}