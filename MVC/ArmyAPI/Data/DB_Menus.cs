#define DEBUG // 定义 DEBUG 符号
using ArmyAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Web.Http;

namespace ArmyAPI.Data
{
	public partial class MsSqlDataProvider : IDisposable
	{
		public class DB_Menus : MsSqlDataProvider
		{
			#region static DB_Menus GetInstance ()
			public static DB_Menus GetInstance()
			{
				return (new DB_Menus());
			}
			#endregion static DB_Menus GetInstance ()

			#region 建構子
			public DB_Menus()
			{
			}
			public DB_Menus(string connectionString) : base(connectionString, typeof(DB_Menus))
			{
			}
			#endregion 建構子

			#region List<Menus> GetAll(bool showDisable)
			public List<Menus> GetAll(bool showDisable)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("WITH RecursiveMenu AS ( ");
				sb.AppendLine("  SELECT ");
				sb.AppendLine("    [Index], ");
				sb.AppendLine("	Title, ");
				sb.AppendLine("	ID, ");
				sb.AppendLine("	ParentIndex, ");
				sb.AppendLine("	Route_Tableau, ");
				sb.AppendLine("	IsEnable, ");
				sb.AppendLine("	AddDatetime, ");
				sb.AppendLine("	ModifyDatetime, ");
				sb.AppendLine("	ModifyUserID, ");
				sb.AppendLine("    1 AS [Level] ");
				sb.AppendLine("  FROM ");
				sb.AppendLine("    Menus ");
				sb.AppendLine("  WHERE 1=1 ");
				sb.AppendLine("    AND ParentIndex = 0 ");
				if (!showDisable)
				{
					sb.AppendLine("    AND IsEnable = 1");
				}

				sb.AppendLine("  UNION ALL ");

				sb.AppendLine("  SELECT ");
				sb.AppendLine("    t.[Index], ");
				sb.AppendLine("	t.Title, ");
				sb.AppendLine("	t.ID, ");
				sb.AppendLine("	t.ParentIndex, ");
				sb.AppendLine("	t.Route_Tableau, ");
				sb.AppendLine("	t.IsEnable, ");
				sb.AppendLine("	t.AddDatetime, ");
				sb.AppendLine("	t.ModifyDatetime, ");
				sb.AppendLine("	t.ModifyUserID, ");
				sb.AppendLine("    rm.[Level] + 1 ");
				sb.AppendLine("  FROM ");
				sb.AppendLine("    Menus t ");
				sb.AppendLine("  JOIN ");
				sb.AppendLine("    RecursiveMenu rm ");
				sb.AppendLine("  ON ");
				sb.AppendLine("    t.ParentIndex = rm.[Index] ");
				sb.AppendLine(") ");

				sb.AppendLine("SELECT ");
				sb.AppendLine("  rm.* ");
				sb.AppendLine("FROM ");
				sb.AppendLine("  RecursiveMenu rm ");
				sb.AppendLine("ORDER BY ");
				sb.AppendLine("  [Level], [ID]; ");
				#endregion CommandText

				List<Menus> result = Get<Menus>(ConnectionString, sb.ToString(), null);

				return result;
			}
			#endregion List<Menus> GetAll(bool showDisable)

			#region List<Menus> GetPrev4(bool showDisable)
			public List<Menus> GetPrev4(bool showDisable)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("WITH RecursiveMenu AS ( ");
				sb.AppendLine("  SELECT ");
				sb.AppendLine("    [Index], ");
				sb.AppendLine("	Title, ");
				sb.AppendLine("	ID, ");
				sb.AppendLine("	ParentIndex, ");
				sb.AppendLine("	Route_Tableau, ");
				sb.AppendLine("	IsEnable, ");
				sb.AppendLine("	AddDatetime, ");
				sb.AppendLine("	ModifyDatetime, ");
				sb.AppendLine("	ModifyUserID, ");
				sb.AppendLine("    1 AS [Level] ");
				sb.AppendLine("  FROM ");
				sb.AppendLine("    Menus ");
				sb.AppendLine("  WHERE 1=1 ");
				sb.AppendLine("    AND ParentIndex = 0 ");
				if (!showDisable)
				{
					sb.AppendLine("    AND IsEnable = 1 ");
				}
				sb.AppendLine("    AND[Index] IN( ");
				sb.AppendLine("    	SELECT TOP 4[Index] ");
				sb.AppendLine("    	FROM Menus ");
				sb.AppendLine("    	WHERE 1 = 1 ");
				sb.AppendLine("    	  AND ParentIndex = 0 ");
				sb.AppendLine("    	  AND IsFix = 0 ");
				if (!showDisable)
				{
					sb.AppendLine("    	  AND IsEnable = 1 ");
				}
				sb.AppendLine("    	ORDER BY ID ");
				sb.AppendLine("    ) ");

				sb.AppendLine("  UNION ALL ");

				sb.AppendLine("  SELECT ");
				sb.AppendLine("    t.[Index], ");
				sb.AppendLine("	t.Title, ");
				sb.AppendLine("	t.ID, ");
				sb.AppendLine("	t.ParentIndex, ");
				sb.AppendLine("	t.Route_Tableau, ");
				sb.AppendLine("	t.IsEnable, ");
				sb.AppendLine("	t.AddDatetime, ");
				sb.AppendLine("	t.ModifyDatetime, ");
				sb.AppendLine("	t.ModifyUserID, ");
				sb.AppendLine("    rm.[Level] + 1 ");
				sb.AppendLine("  FROM ");
				sb.AppendLine("    Menus t ");
				sb.AppendLine("  JOIN ");
				sb.AppendLine("    RecursiveMenu rm ");
				sb.AppendLine("  ON ");
				sb.AppendLine("    t.ParentIndex = rm.[Index] ");
				sb.AppendLine(") ");

				sb.AppendLine("SELECT ");
				sb.AppendLine("  rm.* ");
				sb.AppendLine("FROM ");
				sb.AppendLine("  RecursiveMenu rm ");
				sb.AppendLine("ORDER BY ");
				sb.AppendLine("  [Level], [ID]; ");
				#endregion CommandText

				List<Menus> result = Get<Menus>(ConnectionString, sb.ToString(), null);

				return result;
			}
			#endregion List<Menus> GetPrev4(bool showDisable)

			#region int Update(int index, string id, string newTitle, bool? isEnable, string userId)
			public int Update(int index, string id, string newTitle, bool? isEnable, string userId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("UPDATE Menus ");
				sb.Append("    SET ");
				if (!string.IsNullOrEmpty(newTitle))
					sb.Append("Title = @Title, ");
				if (isEnable != null)
					sb.Append("IsEnable = @IsEnable, ");
				sb.AppendLine(" ModifyDatetime = GETDATE(), ModifyUserID = @ModifyUserID ");
				sb.AppendLine("WHERE 1=1 ");
				sb.AppendLine("  AND [Index] = @Index ");
				sb.AppendLine("  AND ID = @ID ");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@Index", SqlDbType.Int));
				parameters[parameterIndex++].Value = index;
				parameters.Add(new SqlParameter("@ID", SqlDbType.VarChar, 20));
				parameters[parameterIndex++].Value = id;
				if (!string.IsNullOrEmpty(newTitle))
				{
					parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 50));
					parameters[parameterIndex++].Value = newTitle;
				}
				if (isEnable != null)
				{
					parameters.Add(new SqlParameter("@IsEnable", SqlDbType.Bit));
					parameters[parameterIndex++].Value = isEnable;
				}
				parameters.Add(new SqlParameter("@ModifyUserID", SqlDbType.VarChar, 50));
				parameters[parameterIndex++].Value = userId;

				int result = InsertUpdateDeleteData(ConnectionString, sb.ToString(), parameters.ToArray(), true);

				return result;
			}
			#endregion int Update(int index, string id, string newTitle, bool? isEnable, string userId)
		}
	}
}