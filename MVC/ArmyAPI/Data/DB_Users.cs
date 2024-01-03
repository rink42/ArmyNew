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
		public class DB_Users : MsSqlDataProvider
		{
			#region static DB_Users GetInstance ()
			public static DB_Users GetInstance()
			{
				return (new DB_Users());
			}
			#endregion static DB_Users GetInstance ()

			#region 建構子
			public DB_Users()
			{
				_TableName = "Users";
			}
			public DB_Users(string connectionString) : base(connectionString, typeof(DB_Users))
			{
				_TableName = "Users";
			}
			#endregion 建構子

			#region List<User> GetAll()
			public List<Users> GetAll()
			{
				#region CommandText
				string commText = $@"
SELECT * 
FROM {_TableName} 
ORDER BY [Index]
";
				#endregion CommandText

				List<Users> result = Get1<Users>(ConnectionString, commText, null);

				return result;
			}
			#endregion List<User> GetAll()

			#region int Add(User user, bool isAD)
			public int Add(Users user, bool isAD)
			{
				#region CommandText
				string commText = $@"
IF EXISTS (SELECT 1 FROM {_TableName} WHERE UserID = @UserID) 
BEGIN 
	SELECT -1
	RETURN 
END 

INSERT INTO {_TableName} 
			([UserID], [Password], [Name], [Status] ) 
	VALUES (@UserID, @Password, @Name, NULL) 

SELECT @@ROWCOUNT 
";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = user.UserID;
				parameters.Add(new SqlParameter("@Password", SqlDbType.VarChar, 32));
				parameters[parameterIndex++].Value = !isAD ? user.Password : "";
				parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 128));
				parameters[parameterIndex++].Value = user.Name;

				InsertUpdateDeleteDataThenSelectData(ConnectionString, commText, parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion int Add(User user, bool isAD)

			#region int UpdateFull(User user)
			public int UpdateFull(Users user)
			{
				#region CommandText
				string commText = @"
IF NOT EXISTS (SELECT 1 FROM {_TableName} WHERE UserID = @UserID) 
BEGIN 
  SELECT -1
  RETURN 
END 

DECLARE @Rank1 NVARCHAR(50) 
DECLARE @Title1 NVARCHAR(30) 
DECLARE @Skill1 NVARCHAR(30) 
SET @Rank1 = @Rank 
SET @Title1 = @Title 
SET @Skill1 = @Skill 

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

UPDATE {_TableName} 
    SET [Name] = @Name, [Rank] = @Rank, [Title] = @Title, [Skill] = @Skill, [Status] = @Status, [IPAddr1] = @IPAddr1, [IPAddr2] = @IPAddr2, [Email] = @Email, [PhoneMil] = @PhoneMil, [Phone] = @Phone 
WHERE [UserID] = @UserID 

SELECT @@ROWCOUNT 
";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = user.UserID;
				parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 128));
				parameters[parameterIndex++].Value = user.Name;
				parameters.Add(new SqlParameter("@Rank", SqlDbType.NVarChar, 50));
				parameters[parameterIndex++].Value = user.Rank;
				parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 30));
				parameters[parameterIndex++].Value = user.Title;
				parameters.Add(new SqlParameter("@Skill", SqlDbType.NVarChar, 30));
				parameters[parameterIndex++].Value = user.Skill;
				parameters.Add(new SqlParameter("@Status", SqlDbType.SmallInt));
				parameters[parameterIndex++].Value = user.Status;
				parameters.Add(new SqlParameter("@IPAddr1", SqlDbType.NVarChar, 40));
				parameters[parameterIndex++].Value = user.IPAddr1;
				parameters.Add(new SqlParameter("@IPAddr2", SqlDbType.NVarChar, 40));
				parameters[parameterIndex++].Value = user.IPAddr2;
				parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 128));
				parameters[parameterIndex++].Value = user.Email;
				parameters.Add(new SqlParameter("@PhoneMil", SqlDbType.NVarChar, 50));
				parameters[parameterIndex++].Value = user.PhoneMil;
				parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 50));
				parameters[parameterIndex++].Value = user.Phone;

				InsertUpdateDeleteDataThenSelectData(ConnectionString, commText, parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion int UpdateFull(User user)

			#region int Delete(string userId, string loginId)
			/// <summary>
			/// 刪除
			/// </summary>
			/// <param name="userId"></param>
			/// <returns></returns>
			public int Delete(string userId, string loginId)
			{
				#region CommandText
				string commText =$@"
DELETE FROM ArmyWeb.dbo.MenuUser
WHERE [UserID] = @UserID 

DELETE FROM {_TableName} 
WHERE 1=1 
  AND [UserID] = @UserID 
";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 50));
				parameters[parameterIndex++].Value = userId;

				int result = InsertUpdateDeleteData(ConnectionString, commText, parameters.ToArray(), true);

				return result;
			}
			#endregion int Delete(string userId, string loginId)

			#region int Deletes(string userIds, string loginId)
			/// <summary>
			/// 刪除
			/// </summary>
			/// <param name="userIds"></param>
			/// <returns></returns>
			public int Deletes(string userIds, string loginId)
			{
				#region CommandText
				string commText = $@"
DELETE FROM ArmyWeb.dbo.MenuUser
WHERE [UserID] IN (SELECT value FROM STRING_SPLIT(@UserIDs, ',')) 

DELETE FROM {_TableName} 
WHERE 1=1 
  AND [UserID] IN (SELECT value FROM STRING_SPLIT(@UserIDs, ',')) 
";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserIDs", SqlDbType.VarChar));
				parameters[parameterIndex++].Value = userIds;

				int result = InsertUpdateDeleteData(ConnectionString, commText, parameters.ToArray(), true);

				return result;
			}
			#endregion int Deletes(string userIds, string loginId)

			#region //Users Check(string userId, string md5pw, bool isAD)
//			public Users Check(string userId, string md5pw, bool isAD)
//			{
//				#region CommandText
//				string commText = $@"
//SELECT * FROM {_TableName} WITH (NOLOCK) 
//WHERE 1=1 
//  AND UserID = @UserID 
//";
//				// 非 AD 要驗証密碼
//				if (!isAD)
//					commText += ("  AND [Password] = @Password ");
//                #endregion CommandText

//                List<SqlParameter> parameters = new List<SqlParameter>();
//                int parameterIndex = 0;

//                parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
//                parameters[parameterIndex++].Value = userId;
//                parameters.Add(new SqlParameter("@Password", SqlDbType.VarChar, 32));
//                parameters[parameterIndex++].Value = md5pw ?? "";

//                GetDataReturnDataTable(ConnectionString, commText, parameters.ToArray());

//                Users user = null;

//				if (_ResultDataTable != null && _ResultDataTable.Rows.Count == 1)
//				{
//					user = new Users();
//					user.UserID = _ResultDataTable.Rows[0]["UserID"].ToString();
//					if (!isAD)
//						user.Name = _ResultDataTable.Rows[0]["Name"].ToString();
//					user.Rank = _ResultDataTable.Rows[0]["Rank"].ToString();
//					user.Title = _ResultDataTable.Rows[0]["Title"].ToString();
//					user.Skill = _ResultDataTable.Rows[0]["Skill"].ToString();
//					user.Status = _ResultDataTable.Rows[0]["Status"] != DBNull.Value ? (short?)short.Parse(_ResultDataTable.Rows[0]["Status"].ToString()) : null;
//					user.IPAddr1 = _ResultDataTable.Rows[0]["IPAddr1"].ToString();
//					user.IPAddr2 = _ResultDataTable.Rows[0]["IPAddr2"].ToString();
//					user.Email = _ResultDataTable.Rows[0]["Email"].ToString();
//					user.PhoneMil = _ResultDataTable.Rows[0]["PhoneMil"].ToString();
//					user.Phone = _ResultDataTable.Rows[0]["Phone"].ToString();
//					user.ApplyDate = (_ResultDataTable.Rows[0]["ApplyDate"] != DBNull.Value) ? (DateTime?)DateTime.Parse(_ResultDataTable.Rows[0]["ApplyDate"].ToString()) : null;
//					user.LastLoginDate = (_ResultDataTable.Rows[0]["LastLoginDate"] != DBNull.Value) ? (DateTime?)DateTime.Parse(_ResultDataTable.Rows[0]["LastLoginDate"].ToString()) : null;
//                    user.GroupID = int.Parse(_ResultDataTable.Rows[0]["GroupID"].ToString());
//					user.Process = _ResultDataTable.Rows[0]["Process"] != DBNull.Value ? (byte?)byte.Parse(_ResultDataTable.Rows[0]["Process"].ToString()) : null;
//					user.Reason = _ResultDataTable.Rows[0]["Reason"].ToString();
//					user.Review = _ResultDataTable.Rows[0]["Review"].ToString();
//					user.Outcome = _ResultDataTable.Rows[0]["Outcome"] != DBNull.Value ? (byte?)byte.Parse(_ResultDataTable.Rows[0]["Outcome"].ToString()) : null;
//				}

//				return user;
//			}
			#endregion //DataTable Check(string userId, string md5pw, bool isAD)#region Users Check(string userId, string md5pw, bool isAD)

			#region bool CheckLastLoginDate(string userId)
			public bool CheckLastLoginDate(string userId)
			{
				#region CommandText
				string commText = $@"
DECLARE @IsOK BIT
SELECT 
    @IsOK = CASE 
        WHEN DATEDIFF(MONTH, LastLoginDate, GETDATE()) > 2 THEN 0 
        ELSE 1  -- 如果未超過2個月，可以回傳其他值或保持原樣 
    END 
FROM ArmyWeb.dbo.{_TableName} 
WHERE 1=1 
  AND UserID = @UserID 

IF @IsOK = 0 
BEGIN 
  UPDATE ArmyWeb.dbo.{_TableName} 
      SET Status = -2 
  WHERE DATEDIFF(MONTH, LastLoginDate, GETDATE()) > 2; 
END 
SELECT @IsOK 
";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = userId;

				InsertUpdateDeleteDataThenSelectData(ConnectionString, commText, parameters.ToArray(), ReturnType.Byte, true);

				bool result = false;
				if (_ResultObject != null)
					bool.TryParse(_ResultObject.ToString(), out result);

				return result;
			}
			#endregion bool CheckLastLoginDate(string userId)

			#region bool CheckLoginIP(string userId, string ip)
			public bool CheckLoginIP(string userId, string ip)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				string commText = $@"
SELECT  
    CASE  
        WHEN EXISTS ( 
            SELECT 1 
            FROM ArmyWeb.dbo.{_TableName} 
            WHERE ([IPAddr1] = @IP OR [IPAddr2] = @IP) AND UserId = @UserId 
        ) THEN 1 
        ELSE 0 
    END AS Result 
";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = userId;
				parameters.Add(new SqlParameter("@IP", SqlDbType.NVarChar, 40));
				parameters[parameterIndex++].Value = ip;

				GetDataReturnObject(ConnectionString, CommandType.Text, commText, parameters.ToArray());
				
				bool result = false;
				if (_ResultObject != null)
				{
					result = _ResultObject.ToString() == "1";
				}

				return result;
			}
			#endregion bool CheckLoginIP(string userId, string ip)

			#region Users.Statuses? GetStatus(string userId)
			public Users.Statuses? GetStatus(string userId)
			{
				#region CommandText
				string commText = $@"
SELECT [Status] 
FROM ArmyWeb.dbo.{_TableName} 
WHERE 1=1 
  AND [UserID] = @UserID
";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = userId;

				GetDataReturnObject(ConnectionString, CommandType.Text, commText, parameters.ToArray());

				Users.Statuses? result = null;
				if (_ResultObject != null)
				{
                    if (Enum.TryParse(_ResultObject.ToString(), out Users.Statuses parseResult))
						result = parseResult;
				}

				return result;
			}
			#endregion Users.Statuses? GetStatus(string userId)

			#region int Update(User user)
			public int Update(Users user)
			{
				#region CommandText
				string commText= $@"
IF NOT EXISTS (SELECT 1 FROM {_TableName} WHERE UserID = @UserID) 
BEGIN 
  SELECT -1
  RETURN 
END 

UPDATE {_TableName} 
    SET [Name] = @Name, IPAddr1 = @IPAddr1, IPAddr2 = @IPAddr2, Email = @Email, PhoneMil = @PhoneMil, Phone = @Phone 
WHERE UserID = @UserID 

SELECT @@ROWCOUNT 
";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = user.UserID;
				parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 128));
				parameters[parameterIndex++].Value = user.Name;
				parameters.Add(new SqlParameter("@IPAddr1", SqlDbType.NVarChar, 40));
				parameters[parameterIndex++].Value = user.IPAddr1;
				parameters.Add(new SqlParameter("@IPAddr2", SqlDbType.NVarChar, 40));
				parameters[parameterIndex++].Value = user.IPAddr2;
				parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 128));
				parameters[parameterIndex++].Value = user.Email;
				parameters.Add(new SqlParameter("@PhoneMil", SqlDbType.NVarChar, 50));
				parameters[parameterIndex++].Value = user.PhoneMil;
				parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 50));
				parameters[parameterIndex++].Value = user.Email;

				InsertUpdateDeleteDataThenSelectData(ConnectionString, commText, parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion int Update(User user)

			#region int UpdateStatus(User user)
			public int UpdateStatus(Users user)
			{
				#region CommandText
				string commText = $@"
IF NOT EXISTS (SELECT 1 FROM {_TableName} WHERE UserID = @UserID) 
BEGIN 
  SELECT -1
  RETURN 
END 

UPDATE {_TableName} 
    SET [Status] = @Status, LastLoginDate = GETDATE() 
WHERE UserID = @UserID 

SELECT @@ROWCOUNT 
";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = user.UserID;
				parameters.Add(new SqlParameter("@Status", SqlDbType.SmallInt));
				if (user.Status != null)
					parameters[parameterIndex++].Value = user.Status;
				else
					parameters[parameterIndex++].Value = DBNull.Value;


				InsertUpdateDeleteDataThenSelectData(ConnectionString, commText, parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion int UpdateStatus(User user)

			#region int UpdateStatuses(string userIds, Users.Statuses? status)
			public int UpdateStatuses(string userIds, Users.Statuses? status)
			{
				#region CommandText
				string commText = $@"
UPDATE {_TableName} 
    SET [Status] = @Status 
WHERE UserID IN (SELECT value FROM STRING_SPLIT(@UserIDs, ',')) 

SELECT @@ROWCOUNT 
";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserIDs", SqlDbType.VarChar));
				parameters[parameterIndex++].Value = userIds;
				parameters.Add(new SqlParameter("@Status", SqlDbType.SmallInt));
				if (status != null)
					parameters[parameterIndex++].Value = (short)status;
				else
					parameters[parameterIndex++].Value = DBNull.Value;

				int result = InsertUpdateDeleteData(ConnectionString, commText, parameters.ToArray(), true);

				return result;
			}
			#endregion int UpdateStatuses(string userIds, Users.Statuses? status)

			#region int UpdateGroupID(User user)
			public int UpdateGroupID(Users user)
			{
				#region CommandText
				string commText = $@"
IF NOT EXISTS (SELECT 1 FROM {_TableName} WHERE UserID = @UserID) 
BEGIN 
  SELECT -1
  RETURN 
END 

UPDATE {_TableName} 
    SET [GroupID] = @GroupID 
WHERE UserID = @UserID 

SELECT @@ROWCOUNT 
";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = user.UserID;
				parameters.Add(new SqlParameter("@GroupID", SqlDbType.Int));
				parameters[parameterIndex++].Value = user.GroupID;

				InsertUpdateDeleteDataThenSelectData(ConnectionString, commText, parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion int UpdateGroupID(User user)

			#region int UpdatePW(User user)
			public int UpdatePW(Users user)
			{
				#region CommandText
				string commText = $@"
UPDATE {_TableName} 
    SET [Password] = @Password 
WHERE [UserID] = @UserID 

SELECT @@ROWCOUNT 
";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = user.UserID;
				parameters.Add(new SqlParameter("@Password", SqlDbType.VarChar, 32));
				parameters[parameterIndex++].Value = user.Password;

				InsertUpdateDeleteDataThenSelectData(ConnectionString, commText, parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion int UpdatePW(User user)

			#region int UpdateIP1(User user)
			public int UpdateIP1(Users user)
			{
				#region CommandText
				string commText = $@"
IF NOT EXISTS (SELECT 1 FROM {_TableName} WHERE UserID = @UserID) 
BEGIN 
  SELECT -1
  RETURN 
END 

UPDATE {_TableName} 
    SET [IPAddr1] = @IPAddr1, LastLoginDate = GETDATE() 
WHERE UserID = @UserID 

SELECT @@ROWCOUNT 
";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = user.UserID;
				parameters.Add(new SqlParameter("@IPAddr1", SqlDbType.NVarChar, 40));
				parameters[parameterIndex++].Value = user.Status;

				InsertUpdateDeleteDataThenSelectData(ConnectionString, commText, parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion int UpdateIP1(User user)

			#region UserDetail GetDetail(string userId, bool isAdmin)
			public UserDetail GetDetail(string userId, bool isAdmin)
			{
				#region CommandText
					string ifAdmin = @"
       , U.[Process], 
       U.[Outcome] 
";

				string commText = $@"
SELECT U.UserID, 
       RTRIM(LTRIM(U.[Name])) AS Name, 
       U.[Password] AS PP,
	   RTRIM(LTRIM(m.unit_code)) AS UnitCode,
       RTRIM(LTRIM(un.unit_title)) AS Unit, 
       ISNULL(U.[Rank], m.[rank_code]) AS RankCode,  
       RTRIM(LTRIM(r.rank_title)) AS RankTitle, 
       ISNULL(U.[Title], m.[title_code]) AS TitleCode,  
       RTRIM(LTRIM(t.title_Name)) AS TitleName, 
       ISNULL(U.[Skill], m.[es_skill_code]) AS SkillCode,  
       RTRIM(LTRIM(s.skill_desc)) AS SkillDesc, 
       U.[Status], 
       U.[IPAddr1], 
       U.[IPAddr2], 
       U.[Email], 
       U.[Phone], 
       U.[PhoneMil], 
       U.[ApplyDate], 
       U.[TGroups], 
       U.[Reason],
       U.[Review], 
       U.[GroupID],
       UG.[Title] AS GroupTitle
{(isAdmin ? ifAdmin : "")}
FROM ArmyWeb.dbo.Users AS U 
  LEFT JOIN Army.dbo.v_member_data m ON U.UserID = m.member_id 
  LEFT JOIN Army.dbo.rank r ON r.rank_code = ISNULL(U.[Rank], m.[rank_code]) 
  LEFT JOIN Army.dbo.title t ON t.title_code = ISNULL(U.[Title], m.[title_code]) 
  LEFT JOIN Army.dbo.skill s ON s.skill_code = ISNULL(U.[Skill], m.[es_skill_code]) 
  LEFT JOIN Army.dbo.v_mu_unit un ON un.unit_code = m.unit_code 
  LEFT JOIN ArmyWeb.dbo.UserGroup UG ON U.GroupID = UG.[Index]
WHERE 1=1 
  AND U.UserID = @UserID 
";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = userId;

				UserDetail result = GetOne<UserDetail>(ConnectionString, commText, parameters.ToArray());

				return result;
			}
			#endregion UserDetail GetDetail(string userId, bool isAdmin)

			#region List<UserDetail> GetDetails(bool isAdmin)
			public List<UserDetail> GetDetails(bool isAdmin)
			{
				#region CommandText
				string commText = $@"
SELECT  U.UserID,
        RTRIM(LTRIM(U.[Name])) AS Name,

        RTRIM(LTRIM(m.unit_code)) AS UnitCode,
        RTRIM(LTRIM(un.unit_title)) AS Unit, 
        ISNULL(U.[Rank], m.[rank_code]) AS RankCode,  
        RTRIM(LTRIM(r.rank_title)) AS RankTitle, 
        ISNULL(U.[Title], m.[title_code]) AS TitleCode,  
        RTRIM(LTRIM(t.title_Name)) AS TitleName, 
        ISNULL(U.[Skill], m.[es_skill_code]) AS SkillCode,  
        RTRIM(LTRIM(s.skill_desc)) AS SkillDesc, 

		U.[Status],
		U.IPAddr1,
		U.IPAddr2,
		U.[Email],
		U.Phone,
		U.PhoneMil,
		U.LastLoginDate,
        U.Reason,
        U.Review,
		U.TGroups
		{(isAdmin ? (", U.Process, U.Outcome ") : "")}
FROM {_TableName} AS U 
  LEFT JOIN Army.dbo.v_member_data m ON U.UserID = m.member_id 
  LEFT JOIN Army.dbo.rank r ON r.rank_code = ISNULL(U.[Rank], m.[rank_code]) 
  LEFT JOIN Army.dbo.title t ON t.title_code = ISNULL(U.[Title], m.[title_code]) 
  LEFT JOIN Army.dbo.skill s ON s.skill_code = ISNULL(U.[Skill], m.[es_skill_code]) 
  LEFT JOIN Army.dbo.v_mu_unit un ON un.unit_code = m.unit_code 
";
				#endregion CommandText

				List<UserDetail> result = Get1<UserDetail>(ConnectionString, commText, null);

				return result;
			}
			#endregion List<UserDetail> GetDetail(bool isAdmin)

			#region int UpdateDetail(UserDetail user, bool isAdmin)
			public int UpdateDetail(UserDetail user, bool isAdmin)
			{
				#region CommandText
				string commText = $@"
IF NOT EXISTS (SELECT 1 FROM {_TableName} WHERE UserID = @UserID) 
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

UPDATE {_TableName} 
    SET [Name] = @Name, [Rank] = @RankCode, [Title] = @TitleCode, [Skill] = @SkillCode, [IPAddr1] = @IPAddr1, [Email] = @Email, [PhoneMil] = @PhoneMil, [Phone] = @Phone, [Reason] = @Reason --ifAdmin
WHERE [UserID] = @UserID 

SELECT @@ROWCOUNT 
";
				commText = commText.Replace("--ifAdmin", isAdmin ? ", [IPAddr2] = @IPAddr2, [Process] = @Process, [Review] = @Review, [Outcome] = @Outcome " : "");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = user.UserID;
				parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 128));
				parameters[parameterIndex++].Value = user.Name;
				parameters.Add(new SqlParameter("@RankCode", SqlDbType.VarChar, 2));
				parameters[parameterIndex++].Value = user.RankCode;
				parameters.Add(new SqlParameter("@TitleCode", SqlDbType.VarChar, 4));
				parameters[parameterIndex++].Value = user.TitleCode;
				parameters.Add(new SqlParameter("@SkillCode", SqlDbType.VarChar, 6));
				parameters[parameterIndex++].Value = user.SkillCode;
				parameters.Add(new SqlParameter("@IPAddr1", SqlDbType.NVarChar, 40));
				parameters[parameterIndex++].Value = user.IPAddr1;
				parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 128));
				parameters[parameterIndex++].Value = user.Email;
				parameters.Add(new SqlParameter("@PhoneMil", SqlDbType.NVarChar, 50));
				parameters[parameterIndex++].Value = user.PhoneMil;
				parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 50));
				parameters[parameterIndex++].Value = user.Email;
				parameters.Add(new SqlParameter("@Reason", SqlDbType.NVarChar, 500));
				parameters[parameterIndex++].Value = user.Reason;
				if (isAdmin)
				{
					parameters.Add(new SqlParameter("@IPAddr2", SqlDbType.NVarChar, 40));
					parameters[parameterIndex++].Value = user.IPAddr2;
					parameters.Add(new SqlParameter("@Process", SqlDbType.TinyInt));
					parameters[parameterIndex++].Value = user.Process;
					parameters.Add(new SqlParameter("@Review", SqlDbType.NVarChar, 500));
					parameters[parameterIndex++].Value = user.Review;
					parameters.Add(new SqlParameter("@Outcome", SqlDbType.TinyInt));
					parameters[parameterIndex++].Value = user.Outcome;
				}
				InsertUpdateDeleteDataThenSelectData(ConnectionString, commText, parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion int UpdateDetail(UserDetail user, bool isAdmin)

			#region List<User> GetByStatus(Users.Statuses status)
			public List<Users> GetByStatus(Users.Statuses status)
			{
				#region CommandText
				string commText = $@"
SELECT * 
FROM {_TableName} 
WHERE 1=1 
  AND [Status] = @Status 
ORDER BY [Index]; 
";
				#endregion CommandText

				//List<SqlParameter> parameters = new List<SqlParameter>();
				//int parameterIndex = 0;

				//parameters.Add(new SqlParameter("@Status", SqlDbType.SmallInt));
				//parameters[parameterIndex++].Value = (short)status;

				var parameters = new { Status = (short)status };

				List<Users> result = Get1<Users>(ConnectionString, commText, parameters);

				return result;
			}
            #endregion List<User> GetByStatus(Users.Statuses status)

            #region int UpdateLastLoginDate(UserDetail user)
            public int UpdateLastLoginDate(UserDetail user)
            {
				#region CommandText
				string commText = $@"
IF NOT EXISTS (SELECT 1 FROM {_TableName} WHERE UserID = @UserID) 
BEGIN 
  SELECT -1
  RETURN 
END 

UPDATE {_TableName} 
    SET [LastLoginDate] = GETDATE() 
WHERE UserID = @UserID 

SELECT @@ROWCOUNT 
";
                #endregion CommandText

                List<SqlParameter> parameters = new List<SqlParameter>();
                int parameterIndex = 0;

                parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
                parameters[parameterIndex++].Value = user.UserID;

                InsertUpdateDeleteDataThenSelectData(ConnectionString, commText, parameters.ToArray(), ReturnType.Int, true);

                int result = int.Parse(_ResultObject.ToString());

                return result;
            }
            #endregion int UpdateLastLoginDate(UserDetail user)
        }
    }
}