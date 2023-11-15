#define DEBUG // 定义 DEBUG 符号
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ArmyAPI.Models;

namespace ArmyAPI.Data
{
	public partial class MsSqlDataProvider : IDisposable
	{
		public class DB_Users : MsSqlDataProvider
		{
			#region static DB_Account GetInstance ()
			public static DB_Users GetInstance()
			{
				return (new DB_Users());
			}
			#endregion static DB_Account GetInstance ()

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

			#region int Add(User user)
			public int Add(Users user)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"IF EXISTS (SELECT 1 FROM {_TableName} WHERE UserID = @UserID) ");
				sb.AppendLine("BEGIN ");
				sb.AppendLine("  SELECT -1");
				sb.AppendLine("  RETURN ");
				sb.AppendLine("END ");

				sb.AppendLine($"INSERT INTO {_TableName} ");
				sb.AppendLine("         ([UserID], [Password], [Name] ) ");
				sb.AppendLine("    VALUES (@UserID, @Password, @Name) ");

				sb.AppendLine("SELECT @@ROWCOUNT ");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = user.UserID;
				parameters.Add(new SqlParameter("@Password", SqlDbType.VarChar, 32));
				parameters[parameterIndex++].Value = user.Password;
				parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 128));
				parameters[parameterIndex++].Value = user.Name;

				InsertUpdateDeleteDataThenSelectData(ConnectionString, sb.ToString(), parameters.ToArray(), ReturnType.Int, true);

				int result = int.Parse(_ResultObject.ToString());

				return result;
			}
			#endregion int Add(User user)

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
				sb.AppendLine("           FROM v_member_data AS vm ");
				sb.AppendLine("             LEFT JOIN rank r ON r.rank_code = vm.rank_code ");
				sb.AppendLine("             LEFT JOIN skill s ON s.skill_code = vm.ed_skill_code ");
				sb.AppendLine("             LEFT JOIN title t ON t.title_code = vm.title_code ");
				sb.AppendLine("           WHERE vm.member_id = @UseID ");
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

			#region string Check(string userId, string md5pw)
			public string Check(string userId, string md5pw)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine($"SELECT [Name] FROM {_TableName} WHERE UserID = @UserID AND [Password] = @Password ");
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 50));
				parameters[parameterIndex++].Value = userId;
				parameters.Add(new SqlParameter("@Password", SqlDbType.VarChar, 32));
				parameters[parameterIndex++].Value = md5pw;

				GetDataReturnObject(ConnectionString, CommandType.Text, sb.ToString(), parameters.ToArray());

				string result = "";

				if (_ResultObject != null)
					result = _ResultObject.ToString();

				return result;
			}
			#endregion string Check(string userId, string md5pw)

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
				sb.AppendLine("    SET [Status] = @Status ");
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
				sb.Append("SELECT U.UserID, U.Name, un.unit_title AS Unit, r.rank_title AS Rank, t.title_Name AS Title, s.skill_desc AS Skill, U.Status, U.IPAddr1, U.IPAddr2, U.Email, U.Phone, U.PhoneMil, U.ApplyDate ");
				if (isAdmin)
					sb.AppendLine(", U.Process, U.Reason, U.Review, U.Outcome ");
				sb.AppendLine($"FROM {_TableName} AS U ");
				sb.AppendLine("  LEFT JOIN army.dbo.v_member_data m ON U.UserID = m.member_id ");
				sb.AppendLine("  LEFT JOIN army.dbo.rank r ON r.rank_code = m.rank_code ");
				sb.AppendLine("  LEFT JOIN army.dbo.title t ON t.title_code = m.title_code ");
				sb.AppendLine("  LEFT JOIN army.dbo.skill s ON s.skill_code = m.es_skill_code ");
				sb.AppendLine("  LEFT JOIN army.dbo.v_mu_unit un ON un.unit_code = m.unit_code ");
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
				sb.Append("SELECT U.UserID, U.Name, un.unit_title AS Unit, r.rank_title AS Rank, t.title_Name AS Title, s.skill_desc AS Skill, U.Status, U.IPAddr1, U.IPAddr2, U.Email, U.Phone, U.PhoneMil ");
				if (isAdmin)
					sb.AppendLine(", U.Process, U.Reason, U.Review, U.Outcome ");
				sb.AppendLine($"FROM {_TableName} AS U ");
				sb.AppendLine("  LEFT JOIN army.dbo.v_member_data m ON U.UserID = m.member_id ");
				sb.AppendLine("  LEFT JOIN army.dbo.rank r ON r.rank_code = m.rank_code ");
				sb.AppendLine("  LEFT JOIN army.dbo.title t ON t.title_code = m.title_code ");
				sb.AppendLine("  LEFT JOIN army.dbo.skill s ON s.skill_code = m.es_skill_code ");
				sb.AppendLine("  LEFT JOIN army.dbo.v_mu_unit un ON un.unit_code = m.unit_code ");
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

				sb.AppendLine($"UPDATE {_TableName} ");
				sb.Append("    SET [Name] = @Name, IPAddr1 = @IPAddr1, Email = @Email, PhoneMil = @PhoneMil, Phone = @Phone");
				if (isAdmin)
					sb.AppendLine(", IPAddr2 = @IPAddr2, Process = @Process, Reason = @Reason, Review = @Review, Outcome = @Outcome ");
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
				parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 128));
				parameters[parameterIndex++].Value = user.Email;
				parameters.Add(new SqlParameter("@PhoneMil", SqlDbType.NVarChar, 50));
				parameters[parameterIndex++].Value = user.PhoneMil;
				parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 50));
				parameters[parameterIndex++].Value = user.Email;
				if (isAdmin)
				{
					parameters.Add(new SqlParameter("@IPAddr2", SqlDbType.NVarChar, 40));
					parameters[parameterIndex++].Value = user.IPAddr2;
					parameters.Add(new SqlParameter("@Process", SqlDbType.TinyInt));
					parameters[parameterIndex++].Value = user.Process;
					parameters.Add(new SqlParameter("@Reason", SqlDbType.NVarChar, 500));
					parameters[parameterIndex++].Value = user.Email;
					parameters.Add(new SqlParameter("@Review", SqlDbType.NVarChar, 500));
					parameters[parameterIndex++].Value = user.Email;
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
		}
	}
}