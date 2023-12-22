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
		public class DB_s_User_ChangeIP1 : MsSqlDataProvider
		{
			#region static DB_s_User_ChangeIP1 GetInstance ()
			public static DB_s_User_ChangeIP1 GetInstance()
			{
				return (new DB_s_User_ChangeIP1());
			}
			#endregion static DB_s_User_ChangeIP1 GetInstance ()

			#region 建構子
			public DB_s_User_ChangeIP1()
			{
				_TableName = "s_User_ChangeIP1";
			}
			public DB_s_User_ChangeIP1(string connectionString) : base(connectionString, typeof(DB_Menus))
			{
				_TableName = "s_User_ChangeIP1";
			}
			#endregion 建構子

			#region List<s_User_ChangeIP1> GetAll()
			/// <summary>
			/// 取得所有使用者的變更記錄
			/// </summary>
			/// <returns></returns>
			public List<s_User_ChangeIP1> GetAll()
			{
				#region CommandTexts
				string commText= $@"
SELECT * 
FROM {_TableName} 
";
				#endregion CommandText

				List<s_User_ChangeIP1> result = Get<s_User_ChangeIP1>(ConnectionString, commText, null);

				return result;
			}
			#endregion List<s_User_ChangeIP1> GetAll()

			#region s_User_ChangeIP1 Get(string userId)
			/// <summary>
			/// 取得指定使用者的變更記錄
			/// </summary>
			/// <param name="userId"></param>
			/// <returns></returns>
			public s_User_ChangeIP1 Get(string userId)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandTexts
				sb.AppendLine("SELECT * ");
				sb.AppendLine($"FROM {_TableName} ");
				sb.AppendLine("WHERE 1=1 ");
				sb.AppendLine("  AND UserID = @UserID ");
				#endregion CommandText

				s_User_ChangeIP1 result = GetOne<s_User_ChangeIP1>(ConnectionString, sb.ToString(), null);

				return result;
			}
			#endregion s_User_ChangeIP1 Get(string userId)

			#region public long ApplyNewIP1(Users user, s_User_ChangeIP1 changeIP1, string adminId)
			/// <summary>
			/// 申請變更 IP
			/// </summary>
			/// <param name="user"></param>
			/// <param name="changeIP1"></param>
			/// <returns></returns>
			public long ApplyNewIP1(Users user, s_User_ChangeIP1 changeIP1, string adminId)
			{
				#region CommandTexts
				string commText = $@"
DECLARE @UserID VARCHAR(10)
SET @UserID = '{user.UserID}'

IF NOT EXISTS(SELECT 1 FROM {_TableName} WHERE ApplyDate = CAST(GETDATE() AS DATE) AND UserID = @UserID)
BEGIN
  --DECLARE @NewIP VARCHAR(40)
  --SET @NewIP = '{changeIP1}'

  --DECLARE @OldIP VARCHAR(40)
  --SET @OldIP = '{user.IPAddr1}'

  DECLARE @NewApplyNO BIGINT;
  DECLARE @Prefix BIGINT;

  -- 取得民國年（ROC year）
  SET @Prefix = CAST(CONVERT(NVARCHAR(8), GETDATE(), 112) AS INT) - 19110000
  DECLARE @MaxApplyNO BIGINT;

  -- 取得目前最大的 ApplyNO
  SELECT @MaxApplyNO = MAX(ApplyNO)
  FROM {_TableName}
  WHERE ApplyNO LIKE CAST(@Prefix AS VARCHAR) + '%';

  -- 如果不存在以當天日期為前綴的 ApplyNO，則設定流水號為 001，否則在最大流水號基礎上加一
  IF @MaxApplyNO IS NULL
      SET @NewApplyNO = @Prefix * 1000 + 1;
  ELSE
      SET @NewApplyNO = @MaxApplyNO + 1;

  SELECT @NewApplyNO
  INSERT INTO {_TableName}
      VALUES (GETDATE(), @NewApplyNO, @UserID, @NewIP, @OldIP, 0, '', '', @AdminID)

  SELECT @NewApplyNO
END
ELSE
BEGIN
  SELECT -1
END
";
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = user.UserID;
				parameters.Add(new SqlParameter("@NewIP", SqlDbType.VarChar, 40));
				parameters[parameterIndex++].Value = changeIP1;
				parameters.Add(new SqlParameter("@OldIP", SqlDbType.VarChar, 40));
				parameters[parameterIndex++].Value = user.IPAddr1;
				parameters.Add(new SqlParameter("@AdminID", SqlDbType.VarChar, 10));
				parameters[parameterIndex++].Value = adminId;

				InsertUpdateDeleteDataThenSelectData(ConnectionString, commText, parameters.ToArray(), ReturnType.Int, true);

				long result = 0;
				if (_ResultObject != null)
					long.TryParse(_ResultObject.ToString(), out result);

				return result;
			}
			#endregion public long ApplyNewIP1(Users user, s_User_ChangeIP1 changeIP1, string adminId)

			#region int DeleteAll__s_Unit()
			public int DeleteAll__s_Unit()
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				List<string> queries = new List<string>();
				List<object> parametersList = new List<object>();
				string tableName = "s_Unit";
				#region CommandText
				sb.AppendLine($"TRUNCATE TABLE {tableName} ");
				queries.Add(sb.ToString());
				sb.Length = 0;
				#endregion CommandText

				parametersList.Add(null);

				int result = (new DapperHelper(BaseController._ConnectionString)).ExecuteTransaction(queries, parametersList);

				return result;
			}
			#endregion int DeleteAll__s_Unit()
		}
	}
}