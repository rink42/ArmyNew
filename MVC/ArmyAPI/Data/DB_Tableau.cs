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
				[Description("常備兵退伍,0104/3,army0104")]
				army0104,
				[Description("重複佔缺,03-01/03-02-a,army0301")]
				army0301,
				[Description("晉任退伍,0401/040101,army0401")]
				army0401,
				[Description("晉任預警,0402/040201,army0402")]
				army0402,
				[Description("獎點溢滿,_0/040301,army0403")]
				army0403,
				[Description("獎點事由,040301/040301,army040301")]
				army040301,
				[Description("績學獎章,040302/2,army040302")]
				army040302,
				[Description("另予考績,040303/040303,army040303")]
				army040303,
				[Description("預備役管,04-03-04N/04-03-04-a,army040304")]
				army040304,
				[Description("聘雇屆退,04-03-05N/04-03-05-a,army040305")]
				army040305,
				[Description("教師任職,04-03-07N/04-03-07-,army040307")]
				army040307,
				[Description("育嬰留停,04-03-08N/04050101,army040308")]
				army040308,
				[Description("聯戰軍官,04-03-09N/04050101,army040309")]
				army040309,
				[Description("教測人力異動,04-03-10N_16968318166300/0403100101,army04031001")]
				army04031001,
				[Description("教測人力空缺,04-03-10N/04050101,army04031002")]
				army04031002,
				[Description("民間學歷,040401/04040101,army040401")]
				army040401,
				[Description("智測成績,040402/04040101,army040402")]
				army040402,
				[Description("轉服役期,040403/04040101,army040403")]
				army040403,
				[Description("年度考績,040404/04040101,army040404")]
				army040404,
				[Description("電子照片,040405/04040101,army040405")]
				army040405,
				[Description("役期管制,040406/04040101,army040406")]
				army040406,
				[Description("晉支人令,040407/04040101,army040407")]
				army040407,
				[Description("管制役期,040408/04040101,army040408")]
				army040408,
				[Description("編裝錯誤,040501/04040101,army040501")]
				army040501,
				[Description("低階高佔,040502/04050101,army040502")]
				army040502,
				[Description("組織調整,040503/04050101,army040503")]
				army040503,
				[Description("人薪不符,04-05-04/04050301,army040504")]
				army040504,
				[Description("公餘進修,04-N07/04050101,army0407,退伍日期 IS NOT NULL AND 退伍日期 != ''")]
				army0407_1,
				[Description("公餘退伍,04-N07/040705,army0407,(退伍日期 IS NULL OR 退伍日期 = '')")]
				army0407_2,
				[Description("招募人員,04-N08/04050101,army0408")]
				army0408,
				[Description("轉服常備,04-N09/04050101,army0409")]
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
				sb.AppendLine($"FROM Tableau.dbo.{descs[2]} ");
				sb.AppendLine("WHERE 1=1 ");
				if (!string.IsNullOrEmpty(unit))
				{
					sb.AppendLine("  AND 單位 LIKE @Unit + '%' ");
				}
				if (descs.Length > 3)
				{
					sb.AppendLine($"  AND {descs[3]}");
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