#define DEBUG // 定义 DEBUG 符号
using System;
using System.Collections.Generic;
using ArmyAPI.Models;

namespace ArmyAPI.Data
{
	public partial class MsSqlDataProvider : IDisposable
	{
		public class DB_Army_Rank : MsSqlDataProvider
		{
			#region static DB_Army_Rank GetInstance ()
			public static DB_Army_Rank GetInstance()
			{
				return (new DB_Army_Rank());
			}
			#endregion static DB_Army_Rank GetInstance ()

			#region 建構子
			public DB_Army_Rank()
			{
				_TableName = "Rank";
			}
			public DB_Army_Rank(string connectionString) : base(connectionString, typeof(DB_Army_Rank))
			{
				_TableName = "Rank";
			}
			#endregion 建構子

			#region List<Rank> GetAll()
			public List<Rank> GetAll()
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("SELECT TRIM(rank_code) as rank_code, TRIM(rank_title) as rank_title ");
				sb.AppendLine($"FROM Army.dbo.{_TableName} ");
				sb.AppendLine("WHERE rank_code BETWEEN '01' AND '92' ");
				sb.AppendLine("ORDER BY rank_code; ");
				#endregion CommandText

				List<Rank> result = Get<Rank>(ConnectionString, sb.ToString(), null);

				return result;
			}
			#endregion List<Army_Rank> GetAll()
		}
	}
}