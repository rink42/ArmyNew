#define DEBUG // 定义 DEBUG 符号
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ArmyAPI.Commons;
using ArmyAPI.Models;

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
				_TableName = "Menus";
			}
			public DB_Menus(string connectionString) : base(connectionString, typeof(DB_Menus))
			{
				_TableName = "Menus";
			}
			#endregion 建構子

			#region List<Menus> GetAll(bool showDisable)
			public List<Menus> GetAll(bool showDisable)
			{
				return GetAll(showDisable, "");
			}
			#endregion List<Menus> GetAll(bool showDisable)

			#region List<Menus> GetLeftMenu(string loginId)
			public List<Menus> GetLeftMenu(string loginId)
			{
				return GetAll(false, loginId);
			}
			#endregion List<Menus> GetLeftMenu(string loginId)

			#region List<Menus> GetAll(bool showDisable, string loginId)
			public List<Menus> GetAll(bool showDisable, string loginId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("SELECT * ");
				sb.AppendLine($"FROM {_TableName} ");
				sb.AppendLine("WHERE 1=1 ");
				if (!string.IsNullOrEmpty(loginId))
				{
					sb.AppendLine("  AND [Index] IN ( ");
					sb.AppendLine("    SELECT MenuIndex ");
					sb.AppendLine("    FROM MenuUser ");
					sb.AppendLine("    WHERE 1=1 ");
					sb.AppendLine("      AND UserID = @UserID ");
					sb.AppendLine("  ) ");
				}
				if (!showDisable)
					sb.AppendLine("  AND IsEnable = 1 ");
				sb.AppendLine("ORDER BY [Level], Sort; ");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = loginId;

				List<Menus> result = Get<Menus>(ConnectionString, sb.ToString(), parameters.ToArray());

				return result;
			}
			#endregion List<Menus> GetAll(bool showDisable, string loginId)

			#region List<Menus> GetWithoutFix(bool showDisable)
			public List<Menus> GetWithoutFix(bool showDisable)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("SELECT * ");
				sb.AppendLine($"FROM {_TableName} ");
				sb.AppendLine("WHERE 1=1");
				sb.AppendLine("  AND IsFix = 0 ");
				if (!showDisable)
				{
					sb.AppendLine("  AND IsEnable = 1 ");
				}
				sb.AppendLine("ORDER BY [Level], Sort; ");
				#endregion CommandText

				List<Menus> result = Get<Menus>(ConnectionString, sb.ToString(), null);

				return result;
			}
			#endregion List<Menus> GetWithoutFix(bool showDisable)

			#region List<Menus> GetWithoutFix(bool showDisable, string loginId)
			public List<Menus> GetWithoutFix(bool showDisable, string loginId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("SELECT * ");
				sb.AppendLine($"FROM {_TableName} ");
				sb.AppendLine("WHERE 1=1");
				if (!string.IsNullOrEmpty(loginId))
				{
					sb.AppendLine("  AND [Index] IN ( ");
					sb.AppendLine("    SELECT MenuIndex ");
					sb.AppendLine("    FROM MenuUser ");
					sb.AppendLine("    WHERE 1=1 ");
					sb.AppendLine("      AND UserID = @UserID ");
					sb.AppendLine("  ) ");
				}
				sb.AppendLine("  AND IsFix = 0 ");
				if (!showDisable)
				{
					sb.AppendLine("  AND IsEnable = 1 ");
				}
				sb.AppendLine("ORDER BY [Level], Sort; ");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = loginId;

				List<Menus> result = Get<Menus>(ConnectionString, sb.ToString(), parameters.ToArray());

				return result;
			}
			#endregion List<Menus> GetWithoutFix(bool showDisable, string loginId)

			#region List<Menus> GetLeftMenu(bool showDisable, string loginId)
			public List<Menus> GetLeftMenu(bool showDisable, string loginId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("DECLARE @IsAdmin BIT ");
				sb.AppendLine("SET @IsAdmin = 0 ");

				sb.AppendLine("SELECT @IsAdmin = COUNT(UG.[Index]) ");
				sb.AppendLine("FROM USerGroup AS UG ");
				sb.AppendLine("  LEFT JOIN Users AS U ON UG.[Index] = U.GroupID ");
				sb.AppendLine("WHERE UG.Title = '管理者' ");
				sb.AppendLine("  AND U.UserID = @UserID ");

				sb.AppendLine("SELECT M.* ");
				sb.AppendLine($"FROM {_TableName} AS M ");
				sb.AppendLine("WHERE 1=1 ");
				if (!string.IsNullOrEmpty(loginId))
				{
					sb.AppendLine("  AND ( ");
					sb.AppendLine("     [Index] IN (SELECT MenuIndex FROM MenuUser WHERE 1=1 AND UserID = @UserID) ");
					sb.AppendLine("     OR @IsAdmin = 1 ");
					sb.AppendLine("  ) ");
				}
				if (!showDisable)
					sb.AppendLine("  AND IsEnable = 1 ");
				sb.AppendLine("ORDER BY [Level], Sort; ");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = loginId;

				List<Menus> result = Get<Menus>(ConnectionString, sb.ToString(), parameters.ToArray());

				return result;
			}
			#endregion List<Menus> GetLeftMenu(bool showDisable, string loginId)

			#region int Add(string title, int parentIndex, int level, string route_Tableau, bool isEnable, string userId)
			public int Add(string title, int parentIndex, int level, string route_Tableau, bool isEnable, string userId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("IF (@Route_Tableau = '') ");
				sb.AppendLine("  BEGIN");
				sb.AppendLine("    SET @Route_Tableau = NULL");
				sb.AppendLine("  END");
				sb.AppendLine($"INSERT INTO {_TableName} ");
				sb.AppendLine("         ([Title], [Sort], [ParentIndex], [Level], [Route_Tableau], [IsEnable], [ModifyUserID]) ");
				sb.AppendLine("    VALUES (@Title, 0, @ParentIndex, @Level, @Route_Tableau, @IsEnable, @ModifyUserID)");

				sb.AppendLine("SELECT SCOPE_IDENTITY();");
				#endregion CommandText

				List <SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 50));
				parameters[parameterIndex++].Value = title;
				parameters.Add(new SqlParameter("@ParentIndex", SqlDbType.Int));
				parameters[parameterIndex++].Value = parentIndex;
				parameters.Add(new SqlParameter("@Level", SqlDbType.TinyInt));
				parameters[parameterIndex++].Value = level;
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
			#endregion int Add(string title, int parentIndex, int level, string route_Tableau, bool isEnable, string userId)

			#region int Update(int index, string newTitle, bool? isEnable, string userId, ChangeParent cp, int level, string route_Tableau)
			public int Update(int index, string newTitle, bool? isEnable, string userId, ChangeParent cp, int level, string route_Tableau)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"UPDATE {_TableName} ");
				sb.Append("    SET ");
				if (!string.IsNullOrEmpty(newTitle))
					sb.Append("Title = @Title, ");
				if (isEnable != null)
					sb.Append("IsEnable = @IsEnable, ");
				if (cp != null)
					sb.Append("ParentIndex = @NewParentIndex, ");
				if (!string.IsNullOrEmpty(route_Tableau))
				{
					sb.Append("Route_Tableau = @Route_Tableau, ");
				}
				sb.AppendLine(" ModifyDatetime = GETDATE(), ModifyUserID = @ModifyUserID ");
				sb.AppendLine("WHERE 1=1 ");
				sb.AppendLine("  AND [Index] = @Index ");
				sb.AppendLine("  AND [Level] = @Level ");
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
				parameters.Add(new SqlParameter("@Level", SqlDbType.TinyInt));
				parameters[parameterIndex++].Value = level;
				if (!string.IsNullOrEmpty(route_Tableau))
				{
					parameters.Add(new SqlParameter("@Route_Tableau", SqlDbType.VarChar, 500));
					parameters[parameterIndex++].Value = route_Tableau;
				}
				parameters.Add(new SqlParameter("@ModifyUserID", SqlDbType.VarChar, 50));
				parameters[parameterIndex++].Value = userId;

				int result = InsertUpdateDeleteData(ConnectionString, sb.ToString(), parameters.ToArray(), true);

				return result;
			}
			#endregion int Update(int index, string newTitle, bool? isEnable, string userId, ChangeParent cp, int level, string route_Tableau)

			#region DataTable AddUpdateMultiData(Menus[] menuses, string userId)
			public DataTable AddUpdateMultiData(Menus[] menuses, string userId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("IF (@Index = 0) ");
				sb.AppendLine("  BEGIN ");
				sb.AppendLine($"    INSERT INTO {_TableName} ");
				sb.AppendLine("                ([Title], [Sort], [ParentIndex], [Level], [Route_Tableau], [IsEnable], [ModifyUserID]) ");
				sb.AppendLine("        VALUES (@Title, @Sort, @ParentIndex, @Level, NULL, 0, @ModifyUserID) ");

				sb.AppendLine("    IF @@ROWCOUNT > 0 ");
				sb.AppendLine("        SELECT SCOPE_IDENTITY() ");
				sb.AppendLine("    ELSE ");
				sb.AppendLine("        SELECT -1 ");
				sb.AppendLine("  END ");
				sb.AppendLine("ELSE ");
				sb.AppendLine("  BEGIN ");
				sb.AppendLine($"    UPDATE {_TableName} ");
				sb.AppendLine("        SET Sort = @Sort, Title = @Title, ParentIndex = @ParentIndex, Level = @Level, ModifyUserID = @ModifyUserID ");
				sb.AppendLine("    WHERE 1=1 ");
				sb.AppendLine("      AND [Index] = @Index ");

				sb.AppendLine("    IF @@ROWCOUNT > 0 ");
				sb.AppendLine("        SELECT @Index ");
				sb.AppendLine("    ELSE ");
				sb.AppendLine("        SELECT -1 ");
				sb.AppendLine("  END ");
				#endregion CommandText

				DataTable result = null;
				List<SqlParameter[]> parameterss = new List<SqlParameter[]>();
				int failedIndex = 0;
				string failedMsg = "";
				foreach (var menus in menuses)
				{
					if (string.IsNullOrEmpty(menus.Title))
					{
						failedIndex = menus.Index;
						failedMsg = "Title 為空";
						break;
					}

					List<SqlParameter> parameters = new List<SqlParameter>();
					int parameterIndex = 0;

					parameters.Add(new SqlParameter("@Index", SqlDbType.Int));
					parameters[parameterIndex++].Value = menus.Index;
					parameters.Add(new SqlParameter("@Sort", SqlDbType.Int));
					parameters[parameterIndex++].Value = menus.Sort;
					parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 50));
					parameters[parameterIndex++].Value = menus.Title;
					parameters.Add(new SqlParameter("@ParentIndex", SqlDbType.Int));
					parameters[parameterIndex++].Value = menus.ParentIndex;
					parameters.Add(new SqlParameter("@Level", SqlDbType.TinyInt));
					parameters[parameterIndex++].Value = menus.Level;
					parameters.Add(new SqlParameter("@ModifyUserID", SqlDbType.VarChar, 50));
					parameters[parameterIndex++].Value = userId;

					parameterss.Add(parameters.ToArray());
				}

				if (string.IsNullOrEmpty(failedMsg))
					result = InsertUpdateDeleteDataThenSelectData(ConnectionString, sb.ToString(), parameterss);
				else
				{
					result = Globals.CreateResultTable();

					DataRow dr = result.NewRow();
					dr[0] = $"Index = {failedIndex} 的 {failedMsg}";
					result.Rows.Add(dr);
				}

				return result;
			}
			#endregion DataTable AddUpdateMultiData(Menus[] menuses, string userId)

			#region int Delete(int index, string userId)
			/// <summary>
			/// 刪除
			/// </summary>
			/// <param name="index"></param>
			/// <param name="userId"></param>
			/// <returns></returns>
			public int Delete(int index, string userId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"DELETE FROM {_TableName} ");
				sb.AppendLine("WHERE 1=1 ");
				sb.AppendLine("  AND [Index] = @Index ");

				sb.AppendLine($"DELETE FROM {_TableName} ");
				sb.AppendLine("WHERE 1=1 ");
				sb.AppendLine("  AND ParentIndex = @Index ");
				sb.AppendLine("  AND (Route_Tableau IS NULL OR LEN(TRIM(Route_Tableau)) = 0) ");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@Index", SqlDbType.Int));
				parameters[parameterIndex++].Value = index;

				int result = InsertUpdateDeleteData(ConnectionString, sb.ToString(), parameters.ToArray(), true);

				return result;
			}
			#endregion int Delete(int index, string userId)

			#region int Deletes(string indexes, string userId)
			/// <summary>
			/// 刪除
			/// </summary>
			/// <param name="indexes"></param>
			/// <param name="userId"></param>
			/// <returns></returns>
			public int Deletes(string indexes, string userId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"DELETE FROM {_TableName} ");
				sb.AppendLine("WHERE 1=1 ");
				sb.AppendLine("  AND [Index] IN (SELECT value FROM STRING_SPLIT(@Indexes, ',')) ");
				sb.AppendLine("  AND [Level] = 3 ");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@Indexes", SqlDbType.VarChar));
				parameters[parameterIndex++].Value = indexes;

				int result = InsertUpdateDeleteData(ConnectionString, sb.ToString(), parameters.ToArray(), true);

				return result;
			}
			#endregion int Deletes(string indexes, string userId)
		}
	}
}