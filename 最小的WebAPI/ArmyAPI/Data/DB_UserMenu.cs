#define DEBUG // 定义 DEBUG 符号
using ArmyAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ArmyAPI.Data
{
	public partial class MsSqlDataProvider : IDisposable
	{
		public class DB_UserMenu : MsSqlDataProvider
		{
			#region static DB_UserMenu GetInstance ()
			public static DB_UserMenu GetInstance()
			{
				return (new DB_UserMenu());
			}
			#endregion static DB_UserMenu GetInstance ()

			#region 建構子
			public DB_UserMenu()
			{
			}
			public DB_UserMenu(string connectionString) : base(connectionString, typeof(DB_UserMenu))
			{
			}
            #endregion 建構子

            #region List<UserMenu> GetByUserIndex(int userIndex)
            public List<UserMenu> GetByUserIndex(int userIndex)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

                #region CommandText
                sb.AppendLine("--DECLARE @UserIndex INT");
                sb.AppendLine($"--SET @UserIndex = {userIndex};");

                sb.AppendLine("WITH RecursiveMenu AS (");
                sb.AppendLine("  SELECT");
                sb.AppendLine("    [Index],");
                sb.AppendLine("    Title,");
                sb.AppendLine("    ParentIndex,");
                sb.AppendLine("    CreateDatetime,");
                sb.AppendLine("    1 AS [Level],");
                sb.AppendLine("	C,");
                sb.AppendLine("	U,");
                sb.AppendLine("	D,");
                sb.AppendLine("	R");
                sb.AppendLine("  FROM");
                sb.AppendLine("    Menus");
                sb.AppendLine("  WHERE");
                sb.AppendLine("    ParentIndex IS NULL");

                sb.AppendLine("  UNION ALL");

                sb.AppendLine("  SELECT");
                sb.AppendLine("    t.[Index],");
                sb.AppendLine("    t.Title,");
                sb.AppendLine("    t.ParentIndex,");
                sb.AppendLine("    t.CreateDatetime,");
                sb.AppendLine("    rm.[Level] + 1,");
                sb.AppendLine("	t.C,");
                sb.AppendLine("	t.U,");
                sb.AppendLine("	t.D,");
                sb.AppendLine("	t.R");
                sb.AppendLine("  FROM");
                sb.AppendLine("    Menus t");
                sb.AppendLine("  JOIN");
                sb.AppendLine("    RecursiveMenu rm");
                sb.AppendLine("  ON");
                sb.AppendLine("    t.ParentIndex = rm.[Index]");
                sb.AppendLine(")");

                sb.AppendLine("SELECT");
                sb.AppendLine("  rm.[Index],");
                sb.AppendLine("  rm.Title,");
                sb.AppendLine("  rm.ParentIndex,");
                sb.AppendLine("  rm.CreateDatetime,");
                sb.AppendLine("  rm.[Level],");
                sb.AppendLine("  rm.C,");
                sb.AppendLine("  rm.U,");
                sb.AppendLine("  rm.D,");
                sb.AppendLine("  rm.R");
                sb.AppendLine("FROM");
                sb.AppendLine("  RecursiveMenu rm");
                sb.AppendLine("WHERE 1=1");
                sb.AppendLine("  AND rm.[Index] IN (");
                sb.AppendLine("          SELECT MenuIndex");
                sb.AppendLine("		  FROM UserMenuMapping UM ");
                sb.AppendLine("		  WHERE 1=1");
                sb.AppendLine("		    AND UM.UserIndex = @UserIndex");
                sb.AppendLine("      )");
                sb.AppendLine("ORDER BY");
                sb.AppendLine("  [Level], [Index];");
                #endregion CommandText

                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@UserIndex", SqlDbType.Int);
                parameters[0].Value = userIndex;

                List<UserMenu> result = Get<UserMenu>(ConnectionString, sb.ToString(), parameters);

				return result;
			}
            #endregion List<UserMenu> GetByUserIndex(int userIndex)
        }
    }
}