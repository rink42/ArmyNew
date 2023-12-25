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

			#region List<Menus> GetLeftMenu(bool showDisable, string loginId)
			public List<Menus> GetLeftMenu(bool showDisable, string loginId)
			{
				#region CommandText
				string commText = $@"
DECLARE @IsAdmin BIT 
SET @IsAdmin = 0 

SELECT @IsAdmin = COUNT(UG.[Index]) 
FROM UserGroup AS UG 
  LEFT JOIN Users AS U ON UG.[Index] = U.GroupID 
WHERE UG.Title = '管理者' 
  AND U.UserID = @UserID 

SELECT M.* 
FROM {_TableName} AS M 
WHERE 1=1 
{(!string.IsNullOrEmpty(loginId) ? @"
  AND ( 
     [Index] IN (SELECT MenuIndex FROM MenuUser WHERE 1=1 AND UserID = @UserID) 
     OR @IsAdmin = 1 
  ) " : "")}
{(!showDisable ? "  AND IsEnable = 1" : "")}
ORDER BY [Level], Sort;
";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = loginId;

				List<Menus> result = Get<Menus>(ConnectionString, commText, parameters.ToArray());

				return result;
			}
			#endregion List<Menus> GetLeftMenu(bool showDisable, string loginId)

			#region List<Menus> GetAll(bool showDisable, string loginId)
			public List<Menus> GetAll(bool showDisable, string loginId)
			{
				#region CommandText
				string commText = $@"
SELECT DISTINCT M.* 
FROM {_TableName} M 
{(!string.IsNullOrEmpty(loginId) ? @"
  JOIN ( 
    SELECT [MenuIndex] FROM MenuUser WHERE UserID = @UserID 
    UNION 
    SELECT mug.[MenuIndex] 
    FROM MenuUserGroup mug 
      INNER JOIN UserGroup ug ON mug.UserGroupIndex = ug.[Index] 
    WHERE ug.[Index] = (SELECT GroupID FROM Users WHERE UserID = @UserID) 
  ) AS MenuIndexes ON M.[Index] = MenuIndexes.[MenuIndex] 
" : "")}
WHERE 1=1 
{(!showDisable ? "  AND M.[IsEnable] = 1 " : "")}
ORDER BY M.[Level], M.Sort; 
";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = loginId;

				List<Menus> result = Get<Menus>(ConnectionString, commText, parameters.ToArray());

				return result;
			}
			#endregion List<Menus> GetAll(bool showDisable, string loginId)

			#region List<Menus> GetWithoutFix(bool showDisable)
			public List<Menus> GetWithoutFix(bool showDisable)
			{
				#region CommandText
				string commText = $@"
SELECT * 
FROM {_TableName} 
WHERE 1=1
  AND IsFix = 0 
{(!showDisable ? "  AND IsEnable = 1" : "")}
ORDER BY [Level], Sort; 
";
				#endregion CommandText

				List<Menus> result = Get<Menus>(ConnectionString, commText, null);

				return result;
			}
			#endregion List<Menus> GetWithoutFix(bool showDisable)

			#region List<Menus> GetWithoutFix(bool showDisable, string loginId)
			public List<Menus> GetWithoutFix(bool showDisable, string loginId)
			{
				#region CommandText
				string commText = $@"
SELECT * 
FROM {_TableName} 
WHERE 1=1
{(!string.IsNullOrEmpty(loginId) ? @"
  AND [Index] IN ( 
    SELECT MenuIndex 
    FROM MenuUser 
    WHERE 1=1 
      AND UserID = @UserID
  ) " : "")}
  AND IsFix = 0 
{(!showDisable ? "  AND IsEnable = 1" : "")}
ORDER BY [Level], Sort; 
";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = loginId;

				List<Menus> result = Get<Menus>(ConnectionString, commText, parameters.ToArray());

				return result;
			}
			#endregion List<Menus> GetWithoutFix(bool showDisable, string loginId)

			#region int Add(string title, int parentIndex, int level, string route_Tableau, bool isEnable, string userId)
			public int Add(string title, int parentIndex, int level, string route_Tableau, bool isEnable, string userId)
			{
				#region CommandText
				string commText = $@"
IF (@Route_Tableau = '') 
  BEGIN
    SET @Route_Tableau = NULL
  END
INSERT INTO {_TableName} 
         ([Title], [Sort], [ParentIndex], [Level], [Route_Tableau], [IsEnable], [ModifyUserID]) 
    VALUES (@Title, 0, @ParentIndex, @Level, @Route_Tableau, @IsEnable, @ModifyUserID)

SELECT SCOPE_IDENTITY();
";
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

				InsertUpdateDeleteDataThenSelectData(ConnectionString, commText, parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion int Add(string title, int parentIndex, int level, string route_Tableau, bool isEnable, string userId)

			#region int Update(int index, string newTitle, bool? isEnable, string userId, ChangeParent cp, int level, string route_Tableau)
			public int Update(int index, string newTitle, bool? isEnable, string userId, ChangeParent cp, int level, string route_Tableau)
			{
				#region CommandText
				string commText = $@"
UPDATE {_TableName} 
    SET {(!string.IsNullOrEmpty(newTitle) ? "Title = @Title, " : "")}{(isEnable != null ? "IsEnable = @IsEnable, " : "")}{(cp != null ? "ParentIndex = @NewParentIndex, " : "")}{(!string.IsNullOrEmpty(route_Tableau) ? "Route_Tableau = @Route_Tableau, " : "")}ModifyDatetime = GETDATE(), ModifyUserID = @ModifyUserID 
WHERE 1=1 
  AND [Index] = @Index 
  AND [Level] = @Level 
  {(cp != null ? "AND ParentIndex = @OldParentIndex " : "")}
";
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

				int result = InsertUpdateDeleteData(ConnectionString, commText, parameters.ToArray(), true);

				return result;
			}
			#endregion int Update(int index, string newTitle, bool? isEnable, string userId, ChangeParent cp, int level, string route_Tableau)

			#region DataTable AddUpdateMultiData(Menus[] menuses, string userId)
			public DataTable AddUpdateMultiData(Menus[] menuses, string userId)
			{
				#region CommandText
				string commText = $@"
IF (@Index = 0) 
  BEGIN 
    INSERT INTO {_TableName} 
                ([Title], [Sort], [ParentIndex], [Level], [Route_Tableau], [IsEnable], [ModifyUserID]) 
        VALUES (@Title, @Sort, @ParentIndex, @Level, @Route_Tableau, @IsEnable, @ModifyUserID) 

    IF @@ROWCOUNT > 0 
        SELECT SCOPE_IDENTITY() 
    ELSE 
        SELECT -1 
  END 
ELSE 
  BEGIN 
    UPDATE {_TableName} 
        SET [Sort] = @Sort, [Title] = @Title, [ParentIndex] = @ParentIndex, [Level] = @Level, [Route_Tableau] = @Route_Tableau, [IsEnable] = @IsEnable, [ModifyUserID] = @ModifyUserID 
    WHERE 1=1 
      AND [Index] = @Index 

    IF @@ROWCOUNT > 0 
        SELECT @Index 
    ELSE 
        SELECT -1 
  END

DECLARE @MenuIndex INT
SELECT @MenuIndex = [Index]
FROM Menus
WHERE [Title] = @Title AND
  [Sort] = @Sort AND
  [ParentIndex] = @ParentIndex AND
  [Level] = @Level AND
  [Route_Tableau] = @Route_Tableau AND
  [IsEnable] = @IsEnable

IF NOT EXISTS (
  SELECT MenuIndex 
  FROM MenuUser
  WHERE UserID = @ModifyUserID
    AND MenuIndex = @MenuIndex)
BEGIN
  INSERT INTO MenuUser
    VALUES (@MenuIndex, 'A129278645')
END
";
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
					parameters.Add(new SqlParameter("@Route_Tableau", SqlDbType.VarChar, 500));
					parameters[parameterIndex++].Value = menus.Route_Tableau ?? "";
					parameters.Add(new SqlParameter("@IsEnable", SqlDbType.Bit));
					parameters[parameterIndex++].Value = menus.IsEnable;
					parameters.Add(new SqlParameter("@ModifyUserID", SqlDbType.VarChar, 50));
					parameters[parameterIndex++].Value = userId;

					parameterss.Add(parameters.ToArray());
				}

				if (string.IsNullOrEmpty(failedMsg))
					result = InsertUpdateDeleteDataThenSelectData(ConnectionString, commText, parameterss);
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
				#region CommandText
				string commText = $@"
DELETE FROM {_TableName} 
WHERE 1=1 
  AND [Index] = @Index 

DELETE FROM {_TableName} 
WHERE 1=1 
  AND ParentIndex = @Index 
  AND (Route_Tableau IS NULL OR LEN(TRIM(Route_Tableau)) = 0) 
";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@Index", SqlDbType.Int));
				parameters[parameterIndex++].Value = index;

				int result = InsertUpdateDeleteData(ConnectionString, commText, parameters.ToArray(), true);

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
				#region CommandText
				string commText = $@"
DELETE FROM {_TableName} 
WHERE 1=1 
  AND [Index] IN (SELECT value FROM STRING_SPLIT(@Indexes, ',')) 
  AND [Level] = 3 
";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@Indexes", SqlDbType.VarChar));
				parameters[parameterIndex++].Value = indexes;

				int result = InsertUpdateDeleteData(ConnectionString, commText, parameters.ToArray(), true);

				return result;
			}
			#endregion int Deletes(string indexes, string userId)
		}
	}
}