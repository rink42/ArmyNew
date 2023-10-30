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
				sb.AppendLine("    Sort, ");
				sb.AppendLine("    Title, ");
				sb.AppendLine("    ParentIndex, ");
				sb.AppendLine("    Route_Tableau, ");
				sb.AppendLine("    IsEnable, ");
				sb.AppendLine("    AddDatetime, ");
				sb.AppendLine("    ModifyDatetime, ");
				sb.AppendLine("    ModifyUserID, ");
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
				sb.AppendLine("    t.Sort, ");
				sb.AppendLine("    t.Title, ");
				sb.AppendLine("    t.ParentIndex, ");
				sb.AppendLine("    t.Route_Tableau, ");
				sb.AppendLine("    t.IsEnable, ");
				sb.AppendLine("    t.AddDatetime, ");
				sb.AppendLine("    t.ModifyDatetime, ");
				sb.AppendLine("    t.ModifyUserID, ");
				sb.AppendLine("    rm.[Level] + 1 ");
				sb.AppendLine("  FROM ");
				sb.AppendLine("    Menus t ");
				sb.AppendLine("  JOIN ");
				sb.AppendLine("    RecursiveMenu rm ");
				sb.AppendLine("  ON ");
				sb.AppendLine("    t.ParentIndex = rm.[Index] ");
				if (!showDisable)
				{
					sb.AppendLine("    AND IsEnable = 1");
				}
				sb.AppendLine(") ");

				sb.AppendLine("SELECT ");
				sb.AppendLine("  rm.* ");
				sb.AppendLine("FROM ");
				sb.AppendLine("  RecursiveMenu rm ");
				sb.AppendLine("ORDER BY ");
				sb.AppendLine("  [Level], Sort; ");
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
				sb.AppendLine("    Sort, ");
				sb.AppendLine("    Title, ");
				sb.AppendLine("    ParentIndex, ");
				sb.AppendLine("    Route_Tableau, ");
				sb.AppendLine("    IsEnable, ");
				sb.AppendLine("    AddDatetime, ");
				sb.AppendLine("    ModifyDatetime, ");
				sb.AppendLine("    ModifyUserID, ");
				sb.AppendLine("    1 AS [Level] ");
				sb.AppendLine("  FROM ");
				sb.AppendLine("    Menus ");
				sb.AppendLine("  WHERE 1=1 ");
				sb.AppendLine("    AND ParentIndex = 0 ");
				if (!showDisable)
				{
					sb.AppendLine("    AND IsEnable = 1 ");
				}
				sb.AppendLine("    AND [Index] IN( ");
				sb.AppendLine("    	SELECT TOP 4[Index] ");
				sb.AppendLine("    	FROM Menus ");
				sb.AppendLine("    	WHERE 1 = 1 ");
				sb.AppendLine("    	  AND ParentIndex = 0 ");
				sb.AppendLine("    	  AND IsFix = 0 ");
				if (!showDisable)
				{
					sb.AppendLine("    	  AND IsEnable = 1 ");
				}
				sb.AppendLine("    	ORDER BY Sort ");
				sb.AppendLine("    ) ");

				sb.AppendLine("  UNION ALL ");

				sb.AppendLine("  SELECT ");
				sb.AppendLine("    t.[Index], ");
				sb.AppendLine("    t.Sort, ");
				sb.AppendLine("    t.Title, ");
				sb.AppendLine("    t.ParentIndex, ");
				sb.AppendLine("    t.Route_Tableau, ");
				sb.AppendLine("    t.IsEnable, ");
				sb.AppendLine("    t.AddDatetime, ");
				sb.AppendLine("    t.ModifyDatetime, ");
				sb.AppendLine("    t.ModifyUserID, ");
				sb.AppendLine("    rm.[Level] + 1 ");
				sb.AppendLine("  FROM ");
				sb.AppendLine("    Menus t ");
				sb.AppendLine("  JOIN ");
				sb.AppendLine("    RecursiveMenu rm ");
				sb.AppendLine("  ON ");
				sb.AppendLine("    t.ParentIndex = rm.[Index] ");
				if (!showDisable)
				{
					sb.AppendLine("    AND IsEnable = 1 ");
				}
				sb.AppendLine(") ");

				sb.AppendLine("SELECT ");
				sb.AppendLine("  rm.* ");
				sb.AppendLine("FROM ");
				sb.AppendLine("  RecursiveMenu rm ");
				sb.AppendLine("ORDER BY ");
				sb.AppendLine("  [Level], Sort; ");
				#endregion CommandText

				List<Menus> result = Get<Menus>(ConnectionString, sb.ToString(), null);

				return result;
			}
			#endregion List<Menus> GetPrev4(bool showDisable)

			#region List<Menus> GetWithoutFix(bool showDisable)
			public List<Menus> GetWithoutFix(bool showDisable)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("WITH RecursiveMenu AS ( ");
				sb.AppendLine("  SELECT ");
				sb.AppendLine("    [Index], ");
				sb.AppendLine("    Sort, ");
				sb.AppendLine("    Title, ");
				sb.AppendLine("    ParentIndex, ");
				sb.AppendLine("    Route_Tableau, ");
				sb.AppendLine("    IsEnable, ");
				sb.AppendLine("    AddDatetime, ");
				sb.AppendLine("    ModifyDatetime, ");
				sb.AppendLine("    ModifyUserID, ");
				sb.AppendLine("    1 AS [Level] ");
				sb.AppendLine("  FROM ");
				sb.AppendLine("    Menus ");
				sb.AppendLine("  WHERE 1=1 ");
				sb.AppendLine("    AND ParentIndex = 0 ");
				if (!showDisable)
				{
					sb.AppendLine("    AND IsEnable = 1 ");
				}
				sb.AppendLine("      AND IsFix = 0 ");

				sb.AppendLine("  UNION ALL ");

				sb.AppendLine("  SELECT ");
				sb.AppendLine("    t.[Index], ");
				sb.AppendLine("    t.Sort, ");
				sb.AppendLine("    t.Title, ");
				sb.AppendLine("    t.ParentIndex, ");
				sb.AppendLine("    t.Route_Tableau, ");
				sb.AppendLine("    t.IsEnable, ");
				sb.AppendLine("    t.AddDatetime, ");
				sb.AppendLine("    t.ModifyDatetime, ");
				sb.AppendLine("    t.ModifyUserID, ");
				sb.AppendLine("    rm.[Level] + 1 ");
				sb.AppendLine("  FROM ");
				sb.AppendLine("    Menus t ");
				sb.AppendLine("  JOIN ");
				sb.AppendLine("    RecursiveMenu rm ");
				sb.AppendLine("  ON ");
				sb.AppendLine("    t.ParentIndex = rm.[Index] ");
				if (!showDisable)
				{
					sb.AppendLine("    AND t.IsEnable = 1 ");
				}
				sb.AppendLine(") ");

				sb.AppendLine("SELECT ");
				sb.AppendLine("  rm.* ");
				sb.AppendLine("FROM ");
				sb.AppendLine("  RecursiveMenu rm ");
				sb.AppendLine("ORDER BY ");
				sb.AppendLine("  [Level], Sort; ");
				#endregion CommandText

				List<Menus> result = Get<Menus>(ConnectionString, sb.ToString(), null);

				return result;
			}
			#endregion List<Menus> GetWithoutFix(bool showDisable)

			#region int Add(string newTitle, bool isEnable, string userId)
			public int Add(string title, int parentIndex, string route_Tableau, bool isEnable, string userId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("IF (@Route_Tableau = '') ");
				sb.AppendLine("  BEGIN");
				sb.AppendLine("    SET @Route_Tableau = NULL");
				sb.AppendLine("  END");
				sb.AppendLine("INSERT INTO Menus ");
				sb.AppendLine("         ([Title], [ParentIndex], [Route_Tableau], [IsEnable], [ModifyUserID]) ");
				sb.AppendLine("    VALUES (@Title, @ParentIndex, @Route_Tableau, @IsEnable, @ModifyUserID)");

				sb.AppendLine("SELECT SCOPE_IDENTITY();");
				#endregion CommandText

				List <SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 50));
				parameters[parameterIndex++].Value = title;
				parameters.Add(new SqlParameter("@ParentIndex", SqlDbType.Int));
				parameters[parameterIndex++].Value = parentIndex;
				parameters.Add(new SqlParameter("@Route_Tableau", SqlDbType.VarChar, 500));
				parameters[parameterIndex++].Value = route_Tableau;
				parameters.Add(new SqlParameter("@IsEnable", SqlDbType.Bit));
				parameters[parameterIndex++].Value = isEnable;
				parameters.Add(new SqlParameter("@ModifyUserID", SqlDbType.VarChar, 50));
				parameters[parameterIndex++].Value = userId;

				InsertUpdateDeleteDataThenSelectData(ConnectionString, sb.ToString(), parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion int Add(int index, string newTitle, bool isEnable, string userId)

			#region int Update(int index, string newTitle, bool? isEnable, string userId, ChangeParent cp)
			public int Update(int index, string newTitle, bool? isEnable, string userId, ChangeParent cp)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("UPDATE Menus ");
				sb.Append("    SET ");
				if (!string.IsNullOrEmpty(newTitle))
					sb.Append("Title = @Title, ");
				if (isEnable != null)
					sb.Append("IsEnable = @IsEnable, ");
				if (cp != null)
					sb.Append("ParentIndex = @NewParentIndex, ");
				sb.AppendLine(" ModifyDatetime = GETDATE(), ModifyUserID = @ModifyUserID ");
				sb.AppendLine("WHERE 1=1 ");
				sb.AppendLine("  AND [Index] = @Index ");
				if (cp != null)
					sb.AppendLine("  AND ParentIndex = @OldParentIndex ");
				#endregion CommandText

					List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@Index", SqlDbType.Int));
				parameters[parameterIndex++].Value = index;
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
				if (cp != null)
				{
					parameters.Add(new SqlParameter("@NewParentIndex", SqlDbType.Int));
					parameters[parameterIndex++].Value = cp.n;
					parameters.Add(new SqlParameter("@OldParentIndex", SqlDbType.Int));
					parameters[parameterIndex++].Value = cp.o;
				}
				parameters.Add(new SqlParameter("@ModifyUserID", SqlDbType.VarChar, 50));
				parameters[parameterIndex++].Value = userId;

				int result = InsertUpdateDeleteData(ConnectionString, sb.ToString(), parameters.ToArray(), true);

				return result;
			}
			#endregion int Update(int index, string newTitle, bool? isEnable, string userId, ChangeParent cp)

			#region int Delete(int index, string userId)
			/// <summary>
			/// 刪除
			/// </summary>
			/// <param name="index"></param>
			/// <param name="id"></param>
			/// <param name="userId"></param>
			/// <returns></returns>
			public int Delete(int index, string userId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("DELETE FROM Menus ");
				sb.AppendLine("WHERE 1=1 ");
				sb.AppendLine("  AND [Index] = @Index ");

				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@Index", SqlDbType.Int));
				parameters[parameterIndex++].Value = index;

				int result = InsertUpdateDeleteData(ConnectionString, sb.ToString(), parameters.ToArray(), true);

				return result;
			}
			#endregion int Delete(int index, string userId)
		}
	}
}