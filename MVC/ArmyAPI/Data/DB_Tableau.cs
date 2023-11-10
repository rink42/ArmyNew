#define DEBUG // 定义 DEBUG 符号
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using ArmyAPI.Commons;

namespace ArmyAPI.Data
{
	public partial class MsSqlDataProvider : IDisposable
	{
		public class DB_Tableau : MsSqlDataProvider
		{
			#region enum TableNames
			public enum TableNames
			{
				[Description("army0301")]
				army0301,
				[Description("army0401")]
				army0401,
				[Description("army0402")]
				army0402,
				[Description("army0403")]
				army0403,
				[Description("army040301")]
				army040301,
				[Description("army040302")]
				army040302,
				[Description("army040303")]
				army040303,
				[Description("army040304")]
				army040304,
				[Description("army040305")]
				army040305,
				[Description("army040307")]
				army040307,
				[Description("army040308")]
				army040308,
				[Description("army040309")]
				army040309,
				[Description("army04031001")]
				army04031001,
				[Description("army04031002")]
				army04031002,
				[Description("army040401")]
				army040401,
				[Description("army040402")]
				army040402,
				[Description("army040403")]
				army040403,
				[Description("army040404")]
				army040404,
				[Description("army040405")]
				army040405,
				[Description("army040406")]
				army040406,
				[Description("army040407")]
				army040407,
				[Description("army040408")]
				army040408,
				[Description("army040501")]
				army040501,
				[Description("army040502")]
				army040502,
				[Description("army040503")]
				army040503,
				[Description("army040504")]
				army040504,
				[Description("army0407,退伍日期 IS NOT NULL AND 退伍日期 != ''")]
				army0407_1,
				[Description("army0407,(退伍日期 IS NULL OR 退伍日期 = '')")]
				army0407_2,
				[Description("army0408")]
				army0408,
				[Description("army0409")]
				army0409
			}
			#endregion enum TableNames

			#region static DB_Tableau GetInstance ()
			public static DB_Tableau GetInstance()
			{
				return (new DB_Tableau());
			}
			#endregion static DB_Tableau GetInstance ()

			#region 建構子
			public DB_Tableau()
			{
			}
			public DB_Tableau(string connectionString) : base(connectionString, typeof(DB_Tableau))
			{
			}
			#endregion 建構子

			#region public int Gets(TableNames tableName, string unit = "")
			public int Gets(TableNames tableName, string unit = "")
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				string[] descs = Globals.GetEnumDesc(tableName).Split(',');
				#region CommandText
				sb.AppendLine("SELECT COUNT(*) ");
				sb.AppendLine($"FROM Tableau.dbo.{descs[0]} ");
				sb.AppendLine("WHERE 1=1 ");
				if (!string.IsNullOrEmpty(unit))
				{
					sb.AppendLine("  AND 單位 LIKE @Unit + '%' ");
				}
				if (descs.Length > 1)
				{
					sb.AppendLine($"  AND {descs[1]}");
				}
				#endregion CommandText

				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;

				parameters.Add(new SqlParameter("@Unit", SqlDbType.VarChar));
				parameters[parameterIndex++].Value = unit;

				GetDataReturnObject(ConnectionString, CommandType.Text, sb.ToString(), parameters.ToArray());

				int result = 0;
				if (_ResultObject != null)
					int.TryParse(_ResultObject.ToString(), out result);

				return result;
			}
			#endregion public int Gets(TableNames tableName, string unit = "")
		}
	}
}