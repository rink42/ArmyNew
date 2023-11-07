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
				sb.AppendLine("ORDER BY ApplyDate; ");
				#endregion CommandText

				List<Users> result = Get<Users>(ConnectionString, sb.ToString(), null);

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
				parameters.Add(new SqlParameter("@Status", SqlDbType.Int));
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

			#region int Delete(string userId, string adminID)
			/// <summary>
			/// 刪除
			/// </summary>
			/// <param name="userId"></param>
			/// <returns></returns>
			public int Delete(string userId, string adminID)
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
			#endregion int Delete(string userId, string adminID)

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
				parameters.Add(new SqlParameter("@Status", SqlDbType.Int));
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
		}
	}
}