#define DEBUG // 定义 DEBUG 符号
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ArmyAPI.Commons;
using ArmyAPI.Models;
using NPOI.OpenXmlFormats.Vml.Office;

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
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("SELECT * ");
				sb.AppendLine($"FROM {_TableName} ");
				sb.AppendLine("ORDER BY [Index]; ");
				#endregion CommandText

				List<Users> result = Get1<Users>(ConnectionString, sb.ToString(), null);

				return result;
			}
			#endregion List<User> GetAll()

			#region int Add(User user, bool isAD)
			public int Add(Users user, bool isAD)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"IF EXISTS (SELECT 1 FROM {_TableName} WHERE UserID = @UserID) ");
				sb.AppendLine("BEGIN ");
				sb.AppendLine("  SELECT -1");
				sb.AppendLine("  RETURN ");
				sb.AppendLine("END ");

				sb.AppendLine($"INSERT INTO {_TableName} ");
				sb.AppendLine("         ([UserID], [Password], [Name], [Status] ) ");
				sb.AppendLine("    VALUES (@UserID, @Password, @Name, -1) ");

				sb.AppendLine("SELECT @@ROWCOUNT ");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = user.UserID;
				parameters.Add(new SqlParameter("@Password", SqlDbType.VarChar, 32));
				parameters[parameterIndex++].Value = !isAD ? user.Password : "";
				parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 128));
				parameters[parameterIndex++].Value = user.Name;

				InsertUpdateDeleteDataThenSelectData(ConnectionString, sb.ToString(), parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion int Add(User user, bool isAD)

			#region int UpdateFull(User user)
			public int UpdateFull(Users user)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"IF NOT EXISTS (SELECT 1 FROM {_TableName} WHERE UserID = @UserID) ");
				sb.AppendLine("BEGIN ");
				sb.AppendLine("  SELECT -1");
				sb.AppendLine("  RETURN ");
				sb.AppendLine("END ");

				sb.AppendLine("DECLARE @Rank1 NVARCHAR(50) ");
				sb.AppendLine("DECLARE @Title1 NVARCHAR(30) ");
				sb.AppendLine("DECLARE @Skill1 NVARCHAR(30) ");
				sb.AppendLine("SET @Rank1 = @Rank ");
				sb.AppendLine("SET @Title1 = @Title ");
				sb.AppendLine("SET @Skill1 = @Skill ");

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

				sb.AppendLine($"UPDATE {_TableName} ");
				sb.AppendLine("    SET [Name] = @Name, [Rank] = @Rank, [Title] = @Title, [Skill] = @Skill, [Status] = @Status, [IPAddr1] = @IPAddr1, [IPAddr2] = @IPAddr2, [Email] = @Email, [PhoneMil] = @PhoneMil, [Phone] = @Phone ");
				sb.AppendLine("WHERE [UserID] = @UserID ");

				sb.AppendLine("SELECT @@ROWCOUNT ");
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

				InsertUpdateDeleteDataThenSelectData(ConnectionString, sb.ToString(), parameters.ToArray(), ReturnType.Int, true);

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
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"DELETE FROM {_TableName} ");
				sb.AppendLine("WHERE 1=1 ");
				sb.AppendLine("  AND [UserID] = @UserID ");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 50));
				parameters[parameterIndex++].Value = userId;

				int result = InsertUpdateDeleteData(ConnectionString, sb.ToString(), parameters.ToArray(), true);

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
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"DELETE FROM {_TableName} ");
				sb.AppendLine("WHERE 1=1 ");
				sb.AppendLine("  AND [UserID] IN (SELECT value FROM STRING_SPLIT(@UserIDs, ',')) ");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserIDs", SqlDbType.VarChar));
				parameters[parameterIndex++].Value = userIds;

				int result = InsertUpdateDeleteData(ConnectionString, sb.ToString(), parameters.ToArray(), true);

				return result;
			}
			#endregion int Deletes(string userIds, string loginId)

			#region //DataTable Check(string userId, string md5pw)
			//public DataTable Check(string userId, string md5pw)
			//{
			//	System.Text.StringBuilder sb = new System.Text.StringBuilder();

			//	#region CommandText
			//	sb.AppendLine($"SELECT [Name], [Status] FROM {_TableName} WITH (NOLOCK) WHERE UserID = @UserID AND [Password] = @Password");
			//	#endregion CommandText

			//	List<SqlParameter> parameters = new List<SqlParameter>();
			//	int parameterIndex = 0;

			//	parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 50));
			//	parameters[parameterIndex++].Value = userId;
			//	parameters.Add(new SqlParameter("@Password", SqlDbType.VarChar, 32));
			//	parameters[parameterIndex++].Value = md5pw;

			//	DataTable result = GetDataTable(ConnectionString, sb.ToString(), parameters.ToArray());

			//	return result;
			//}
			#endregion //DataTable Check(string userId, string md5pw)

			#region Users Check(string userId, string md5pw, bool isAD)
			public Users Check(string userId, string md5pw, bool isAD)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
                sb.AppendLine($"SELECT * FROM {_TableName} WITH (NOLOCK) ");
				sb.AppendLine("WHERE 1=1 ");
				sb.AppendLine("  AND UserID = @UserID ");
				// 非 AD 要驗証密碼， AD帳號則再驗姓名
				if (!isAD)
					sb.AppendLine("  AND [Password] = @Password ");
                #endregion CommandText

                List<SqlParameter> parameters = new List<SqlParameter>();
                int parameterIndex = 0;

                parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
                parameters[parameterIndex++].Value = userId;
                parameters.Add(new SqlParameter("@Password", SqlDbType.VarChar, 32));
                parameters[parameterIndex++].Value = md5pw ?? "";

                GetDataReturnDataTable(ConnectionString, sb.ToString(), parameters.ToArray());

                Users user = null;

				if (_ResultDataTable != null && _ResultDataTable.Rows.Count == 1)
				{
					user = new Users();
					user.UserID = _ResultDataTable.Rows[0]["UserID"].ToString();
					if (!isAD)
						user.Name = _ResultDataTable.Rows[0]["Name"].ToString();
					user.Rank = _ResultDataTable.Rows[0]["Rank"].ToString();
					user.Title = _ResultDataTable.Rows[0]["Title"].ToString();
					user.Skill = _ResultDataTable.Rows[0]["Skill"].ToString();
					user.Status = _ResultDataTable.Rows[0]["Status"] != DBNull.Value ? (short?)short.Parse(_ResultDataTable.Rows[0]["Status"].ToString()) : null;
					user.IPAddr1 = _ResultDataTable.Rows[0]["IPAddr1"].ToString();
					user.IPAddr2 = _ResultDataTable.Rows[0]["IPAddr2"].ToString();
					user.Email = _ResultDataTable.Rows[0]["Email"].ToString();
					user.PhoneMil = _ResultDataTable.Rows[0]["PhoneMil"].ToString();
					user.Phone = _ResultDataTable.Rows[0]["Phone"].ToString();
					user.ApplyDate = (_ResultDataTable.Rows[0]["ApplyDate"] != DBNull.Value) ? (DateTime?)DateTime.Parse(_ResultDataTable.Rows[0]["ApplyDate"].ToString()) : null;
					user.LastLoginDate = (_ResultDataTable.Rows[0]["LastLoginDate"] != DBNull.Value) ? (DateTime?)DateTime.Parse(_ResultDataTable.Rows[0]["LastLoginDate"].ToString()) : null;
                    user.GroupID = int.Parse(_ResultDataTable.Rows[0]["GroupID"].ToString());
					user.Process = _ResultDataTable.Rows[0]["Process"] != DBNull.Value ? (byte?)byte.Parse(_ResultDataTable.Rows[0]["Process"].ToString()) : null;
					user.Reason = _ResultDataTable.Rows[0]["Reason"].ToString();
					user.Review = _ResultDataTable.Rows[0]["Review"].ToString();
					user.Outcome = _ResultDataTable.Rows[0]["Outcome"] != DBNull.Value ? (byte?)byte.Parse(_ResultDataTable.Rows[0]["Outcome"].ToString()) : null;
				}

				return user;
			}
			#endregion DataTable Check(string userId, string md5pw, bool isAD)#region Users Check(string userId, string md5pw, bool isAD)

			#region bool CheckLastLoginDate(string userId)
			public bool CheckLastLoginDate(string userId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("DECLARE @IsOK BIT");
				sb.AppendLine("SELECT ");
				sb.AppendLine("    @IsOK = CASE ");
				sb.AppendLine("        WHEN DATEDIFF(MONTH, LastLoginDate, GETDATE()) > 2 THEN 0 ");
				sb.AppendLine("        ELSE 1  -- 如果未超過2個月，可以回傳其他值或保持原樣 ");
				sb.AppendLine("    END ");
				sb.AppendLine($"FROM ArmyWeb.dbo.{_TableName} ");
				sb.AppendLine("WHERE 1=1 ");
				sb.AppendLine("  AND UserID = @UserID ");
				
				sb.AppendLine("IF @IsOK = 0 ");
				sb.AppendLine("BEGIN ");
				sb.AppendLine($"  UPDATE ArmyWeb.dbo.{_TableName} ");
				sb.AppendLine("      SET Status = -2 ");
				sb.AppendLine("  WHERE DATEDIFF(MONTH, LastLoginDate, GETDATE()) > 2; ");
				sb.AppendLine("END ");
				sb.AppendLine("SELECT @IsOK ");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = userId;

				InsertUpdateDeleteDataThenSelectData(ConnectionString, sb.ToString(), parameters.ToArray(), ReturnType.Byte, true);

				bool result = false;
				if (_ResultObject != null)
					result = bool.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion bool CheckLastLoginDate(string userId)

			#region bool CheckLoginIP(string userId, string ip)
			public bool CheckLoginIP(string userId, string ip)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText				
				sb.AppendLine("SELECT  ");
				sb.AppendLine("    CASE  ");
				sb.AppendLine("        WHEN EXISTS ( ");
				sb.AppendLine("            SELECT 1 ");
				sb.AppendLine($"            FROM ArmyWeb.dbo.{_TableName} ");
				sb.AppendLine("            WHERE ([IPAddr1] = @IP OR [IPAddr2] = @IP) AND UserId = @UserId ");
				sb.AppendLine("        ) THEN 1 ");
				sb.AppendLine("        ELSE 0 ");
				sb.AppendLine("    END AS Result ");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = userId;
				parameters.Add(new SqlParameter("@IP", SqlDbType.NVarChar, 40));
				parameters[parameterIndex++].Value = ip;

				GetDataReturnObject(ConnectionString, CommandType.Text, sb.ToString(), parameters.ToArray());
				
				bool result = false;
				if (_ResultObject != null)
				{
					result = _ResultObject.ToString() == "1";
				}

				return result;
			}
			#endregion bool CheckLoginIP(string userId, string ip)

			#region Users.Statuses GetStatus(string userId)
			public Users.Statuses GetStatus(string userId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"SELECT [Status] ");
				sb.AppendLine($"FROM ArmyWeb.dbo.{_TableName} ");
				sb.AppendLine("WHERE 1=1 ");
				sb.AppendLine("  AND [UserID] = @UserID");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = userId;

				GetDataReturnObject(ConnectionString, CommandType.Text, sb.ToString(), parameters.ToArray());

				Users.Statuses result = Users.Statuses.Disable;
				if (_ResultObject != null)
				{
					if (!Enum.TryParse(_ResultObject.ToString(), out result))
						result = Users.Statuses.Disable;
				}

				return result;
			}
			#endregion Users.Statuses GetStatus(string userId)

			#region int Update(User user)
			public int Update(Users user)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"IF NOT EXISTS (SELECT 1 FROM {_TableName} WHERE UserID = @UserID) ");
				sb.AppendLine("BEGIN ");
				sb.AppendLine("  SELECT -1");
				sb.AppendLine("  RETURN ");
				sb.AppendLine("END ");

				sb.AppendLine($"UPDATE {_TableName} ");
				sb.AppendLine("    SET [Name] = @Name, IPAddr1 = @IPAddr1, IPAddr2 = @IPAddr2, Email = @Email, PhoneMil = @PhoneMil, Phone = @Phone ");
				sb.AppendLine("WHERE UserID = @UserID ");

				sb.AppendLine("SELECT @@ROWCOUNT ");
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

				InsertUpdateDeleteDataThenSelectData(ConnectionString, sb.ToString(), parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion int Update(User user)

			#region int UpdateStatus(User user)
			public int UpdateStatus(Users user)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"IF NOT EXISTS (SELECT 1 FROM {_TableName} WHERE UserID = @UserID) ");
				sb.AppendLine("BEGIN ");
				sb.AppendLine("  SELECT -1");
				sb.AppendLine("  RETURN ");
				sb.AppendLine("END ");

				sb.AppendLine($"UPDATE {_TableName} ");
				sb.AppendLine("    SET [Status] = @Status, LastLoginDate = GETDATE() ");
				sb.AppendLine("WHERE UserID = @UserID ");

				sb.AppendLine("SELECT @@ROWCOUNT ");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = user.UserID;
				parameters.Add(new SqlParameter("@Status", SqlDbType.SmallInt));
				parameters[parameterIndex++].Value = user.Status;

				InsertUpdateDeleteDataThenSelectData(ConnectionString, sb.ToString(), parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion int UpdateStatus(User user)

			#region int UpdateStatuses(string userIds, Users.Statuses status)
			public int UpdateStatuses(string userIds, Users.Statuses status)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"UPDATE {_TableName} ");
				sb.AppendLine("    SET [Status] = @Status ");
				sb.AppendLine("WHERE UserID IN (SELECT value FROM STRING_SPLIT(@UserIDs, ',')) ");

				sb.AppendLine("SELECT @@ROWCOUNT ");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserIDs", SqlDbType.VarChar));
				parameters[parameterIndex++].Value = userIds;
				parameters.Add(new SqlParameter("@Status", SqlDbType.SmallInt));
				parameters[parameterIndex++].Value = (short)status;

				int result = InsertUpdateDeleteData(ConnectionString, sb.ToString(), parameters.ToArray(), true);

				return result;
			}
			#endregion int UpdateStatuses(string userIds, Users.Statuses status)

			#region int UpdateGroupID(User user)
			public int UpdateGroupID(Users user)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"IF NOT EXISTS (SELECT 1 FROM {_TableName} WHERE UserID = @UserID) ");
				sb.AppendLine("BEGIN ");
				sb.AppendLine("  SELECT -1");
				sb.AppendLine("  RETURN ");
				sb.AppendLine("END ");

				sb.AppendLine($"UPDATE {_TableName} ");
				sb.AppendLine("    SET [GroupID] = @GroupID ");
				sb.AppendLine("WHERE UserID = @UserID ");

				sb.AppendLine("SELECT @@ROWCOUNT ");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = user.UserID;
				parameters.Add(new SqlParameter("@GroupID", SqlDbType.Int));
				parameters[parameterIndex++].Value = user.GroupID;

				InsertUpdateDeleteDataThenSelectData(ConnectionString, sb.ToString(), parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion int UpdateGroupID(User user)

			#region int UpdatePW(User user)
			public int UpdatePW(Users user)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"UPDATE {_TableName} ");
				sb.AppendLine("    SET [Password] = @Password ");
				sb.AppendLine("WHERE [UserID] = @UserID ");

				sb.AppendLine("SELECT @@ROWCOUNT ");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = user.UserID;
				parameters.Add(new SqlParameter("@Password", SqlDbType.VarChar, 32));
				parameters[parameterIndex++].Value = user.Password;

				InsertUpdateDeleteDataThenSelectData(ConnectionString, sb.ToString(), parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion int UpdatePW(User user)

			#region UserDetail GetDetail(string userId, bool isAdmin)
			public UserDetail GetDetail(string userId, bool isAdmin)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("SELECT U.UserID, ");
				sb.AppendLine("       U.[Name], ");
				sb.AppendLine("       un.unit_title AS Unit, ");
				sb.AppendLine("       ISNULL(U.[Rank], m.[rank_code]) AS RankCode,  ");
				sb.AppendLine("       TRIM(r.rank_title) AS RankTitle, ");
				sb.AppendLine("       ISNULL(U.[Title], m.[title_code]) AS TitleCode,  ");
				sb.AppendLine("       TRIM(t.title_Name) AS TitleName, ");
				sb.AppendLine("       ISNULL(U.[Skill], m.[es_skill_code]) AS SkillCode,  ");
				sb.AppendLine("       TRIM(s.skill_desc) AS SkillDesc, ");
				sb.AppendLine("       U.[Status], ");
				sb.AppendLine("       U.[IPAddr1], ");
				sb.AppendLine("       U.[IPAddr2], ");
				sb.AppendLine("       U.[Email], ");
				sb.AppendLine("       U.[Phone], ");
				sb.AppendLine("       U.[PhoneMil], ");
				sb.AppendLine("       U.[ApplyDate] ");
				if (isAdmin)
				{
					sb.AppendLine("       , U.[Process], ");
					sb.AppendLine("       U.[Reason], ");
					sb.AppendLine("       U.[Review], ");
					sb.AppendLine("       U.[Outcome] ");
				}
				sb.AppendLine("FROM ArmyWeb.dbo.Users AS U ");
				sb.AppendLine("  LEFT JOIN Army.dbo.v_member_data m ON U.UserID = m.member_id ");
				sb.AppendLine("  LEFT JOIN Army.dbo.rank r ON r.rank_code = ISNULL(U.[Rank], m.[rank_code]) ");
				sb.AppendLine("  LEFT JOIN Army.dbo.title t ON t.title_code = ISNULL(U.[Title], m.[title_code]) ");
				sb.AppendLine("  LEFT JOIN Army.dbo.skill s ON s.skill_code = ISNULL(U.[Skill], m.[es_skill_code]) ");
				sb.AppendLine("  LEFT JOIN Army.dbo.v_mu_unit un ON un.unit_code = m.unit_code ");
				sb.AppendLine("WHERE 1=1 ");
				sb.AppendLine("  AND U.UserID = @UserID ");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = userId;

				UserDetail result = GetOne<UserDetail>(ConnectionString, sb.ToString(), parameters.ToArray());

				return result;
			}
			#endregion UserDetail GetDetail(string userId, bool isAdmin)

			#region List<UserDetail> GetDetails(bool isAdmin)
			public List<UserDetail> GetDetails(bool isAdmin)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.Append("SELECT U.UserID, U.Name, un.unit_title AS Unit, r.rank_title AS Rank, t.title_Name AS Title, s.skill_desc AS Skill, U.Status, U.IPAddr1, U.IPAddr2, U.Email, U.Phone, U.PhoneMil, U.LastLoginDate ");
				if (isAdmin)
					sb.AppendLine(", U.Process, U.Reason, U.Review, U.Outcome ");
				sb.AppendLine($"FROM {_TableName} AS U ");
				sb.AppendLine("  LEFT JOIN Army.dbo.v_member_data m ON U.UserID = m.member_id ");
				sb.AppendLine("  LEFT JOIN Army.dbo.rank r ON r.rank_code = m.rank_code ");
				sb.AppendLine("  LEFT JOIN Army.dbo.title t ON t.title_code = m.title_code ");
				sb.AppendLine("  LEFT JOIN Army.dbo.skill s ON s.skill_code = m.es_skill_code ");
				sb.AppendLine("  LEFT JOIN Army.dbo.v_mu_unit un ON un.unit_code = m.unit_code ");
				#endregion CommandText

				List<UserDetail> result = Get1<UserDetail>(ConnectionString, sb.ToString(), null);

				return result;
			}
			#endregion List<UserDetail> GetDetail(bool isAdmin)

			#region int UpdateDetail(UserDetail user, bool isAdmin)
			public int UpdateDetail(UserDetail user, bool isAdmin)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"IF NOT EXISTS (SELECT 1 FROM {_TableName} WHERE UserID = @UserID) ");
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

				sb.AppendLine($"UPDATE {_TableName} ");
				sb.Append("    SET [Name] = @Name, [Rank] = @RankCode, [Title] = @TitleCode, [Skill] = @SkillCode, [IPAddr1] = @IPAddr1, [Email] = @Email, [PhoneMil] = @PhoneMil, [Phone] = @Phone, [Reason] = @Reason ");
				if (isAdmin)
					sb.AppendLine(", [IPAddr2] = @IPAddr2, [Process] = @Process, [Review] = @Review, [Outcome] = @Outcome ");
				else
					sb.Append("\n ");
				sb.AppendLine("WHERE [UserID] = @UserID ");

				sb.AppendLine("SELECT @@ROWCOUNT ");
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
				InsertUpdateDeleteDataThenSelectData(ConnectionString, sb.ToString(), parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion int UpdateDetail(UserDetail user, bool isAdmin)

			#region List<User> GetByStatus(Users.Statuses status)
			public List<Users> GetByStatus(Users.Statuses status)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("SELECT * ");
				sb.AppendLine($"FROM {_TableName} ");
				sb.AppendLine("WHERE 1=1 ");
				sb.AppendLine("  AND [Status] = @Status ");
				sb.AppendLine("ORDER BY [Index]; ");
				#endregion CommandText

				//List<SqlParameter> parameters = new List<SqlParameter>();
				//int parameterIndex = 0;

				//parameters.Add(new SqlParameter("@Status", SqlDbType.SmallInt));
				//parameters[parameterIndex++].Value = (short)status;

				var parameters = new { Status = (short)status };

				List<Users> result = Get1<Users>(ConnectionString, sb.ToString(), parameters);

				return result;
			}
            #endregion List<User> GetByStatus(Users.Statuses status)

            #region int UpdateLastLoginDate(User user)
            public int UpdateLastLoginDate(Users user)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                #region CommandText
                sb.AppendLine($"IF NOT EXISTS (SELECT 1 FROM {_TableName} WHERE UserID = @UserID) ");
                sb.AppendLine("BEGIN ");
                sb.AppendLine("  SELECT -1");
                sb.AppendLine("  RETURN ");
                sb.AppendLine("END ");

                sb.AppendLine($"UPDATE {_TableName} ");
                sb.AppendLine("    SET [LastLoginDate] = GETDATE() ");
                sb.AppendLine("WHERE UserID = @UserID ");

                sb.AppendLine("SELECT @@ROWCOUNT ");
                #endregion CommandText

                List<SqlParameter> parameters = new List<SqlParameter>();
                int parameterIndex = 0;

                parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
                parameters[parameterIndex++].Value = user.UserID;

                InsertUpdateDeleteDataThenSelectData(ConnectionString, sb.ToString(), parameters.ToArray(), ReturnType.Int, true);

                int result = int.Parse(_ResultObject.ToString());

                return result;
            }
            #endregion int UpdateLastLoginDate(User user)
        }
    }
}