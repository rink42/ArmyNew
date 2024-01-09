#define DEBUG // 定义 DEBUG 符号
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using ArmyAPI.Commons;
using ArmyAPI.Models;

namespace ArmyAPI.Data
{
	public partial class MsSqlDataProvider : IDisposable
	{
		public class DB_Tableau : MsSqlDataProvider
		{
			#region enum TableNames
			public enum TableNames
			{
				[Description("常備兵退伍,01-04/0104D,army0104")]
				army0104,
				[Description("重複佔缺,03-01/1,army0301")]
				army0301,
				[Description("晉任退伍,04-01/0401F,army0401")]
				army0401,
				[Description("晉任預警,-02/0402F,army0402")]
				army0402,
				[Description("獎點溢滿,04-03-00/040300F,army0403")]
				army0403,
				[Description("獎點事由,04-03-01N/040301D,army040301")]
				army040301,
				[Description("績學獎章,04-03-02N/040302D,army040302")]
				army040302,
				[Description("另予考績,04-03-03N/040303D,army040303")]
				army040303,
				[Description("預備役管,04-03-04N/1,army040304")]
				army040304,
				[Description("聘雇屆退,04-03-05N/1,army040305")]
				army040305,
				[Description("教師任職,04-03-07N/1,army040307")]
				army040307,
				[Description("育嬰留停,04-03-08N/040308F,army040308")]
				army040308,
				[Description("聯戰軍官,04-03-09N/040309F,army040309")]
				army040309,
				[Description("教測人力異動,04-03-10N_17002040702430/04031002F,army04031001")]
				army04031001,
				[Description("教測人力空缺,04-03-10N/04031001F,army04031002")]
				army04031002,
				[Description("民間學歷,04-04-01/040401F,army040401")]
				army040401,
				[Description("智測成績,04-04-02/040402F,army040402")]
				army040402,
				[Description("轉服役期,04-04-03/040403F,army040403")]
				army040403,
				[Description("年度考績,04-04-04/040404F,army040404")]
				army040404,
				[Description("電子照片,04-04-05/040405F,army040405")]
				army040405,
				[Description("役期管制,04-04-06/040406F,army040406")]
				army040406,
				[Description("晉支人令,04-04-07/040407F,army040407")]
				army040407,
				[Description("管制役期,04-04-08/040408F,army040408")]
				army040408,
				[Description("編裝錯誤,04-05-01/040501F,army040501")]
				army040501,
				[Description("低階高佔,04-05-02/040502F,army040502")]
				army040502,
				[Description("組織調整,04-05-03/040503F,army040503")]
				army040503,
				[Description("人薪不符,04-05-04/040504F,army040504")]
				army040504,
				[Description("公餘進修,04-N07/0407F,army0407,退伍日期 IS NOT NULL AND 退伍日期 != ''")]
				army0407_1,
				[Description("公餘退伍,04-N07/0407F2,army0407,(退伍日期 IS NULL OR 退伍日期 = '')")]
				army0407_2,
				[Description("招募人員,04-N08/0408F,army0408")]
				army0408,
				[Description("轉服常備,04-N09/0409F,army0409")]
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
				sb.AppendLine($"FROM Tableau.dbo.{descs[2]} WITH (NOLOCK) ");
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

			#region public DataSet Gets(string unit = "")
			public DataSet Gets(string unit = "")
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				int index = 1;
				List<SqlParameter> parameters = new List<SqlParameter>();
				int parameterIndex = 0;
				XML_TableauConfig xmlTableau = new XML_TableauConfig();
				List<TableauConfig_Item> all = xmlTableau.GetAll();
				
				foreach (DB_Tableau.TableNames tableName in System.Enum.GetValues(typeof(DB_Tableau.TableNames)))
				{
					bool isSql = false;
					string[] descs = Globals.GetEnumDesc(tableName).Split(',');
					switch (tableName)
					{
						case TableNames.army040503:
							descs[2] = all[0].Table;
							break;
						case TableNames.army0301:
							descs[2] = all[1].SQL;
							isSql = true;
							break;
					}
					#region CommandText

					if (!isSql)
					{
						if (descs[2].IndexOf(".dbo.") == -1)
							descs[2] = $"Tableau.dbo.{descs[2]}";

						sb.AppendLine($"IF OBJECT_ID('{descs[2]}', 'U') IS NOT NULL ");
						sb.AppendLine("BEGIN ");
						sb.AppendLine($"  SELECT '{descs[0]}' AS c, '{descs[1]}' AS n, COUNT(*) AS v ");
						sb.AppendLine($"  FROM {descs[2]} WITH (NOLOCK) ");
						sb.AppendLine("  WHERE 1=1 ");
						if (!string.IsNullOrEmpty(unit))
						{
							sb.AppendLine($"    AND 單位 LIKE @Unit_{index} + '%' ");
						}
						if (descs.Length > 3)
						{
							sb.AppendLine($"    AND {descs[3]}");
						}
						sb.AppendLine("END ");
						sb.AppendLine("ELSE ");
						sb.AppendLine("BEGIN ");
						sb.AppendLine($"  SELECT '{descs[0]}' AS c, '{descs[1]}' AS n, -1 AS v ");
						sb.AppendLine("END ");
					}
					else
					{
						sb.AppendLine($"USE [Army];\n SELECT '{descs[0]}' AS c, '{descs[1]}' AS n, COUNT(*) AS v FROM ( {descs[2]} ) AS [COUNT]\n");
					}
					#endregion CommandText

					parameters.Add(new SqlParameter($"@Unit_{index++}", SqlDbType.VarChar));
					parameters[parameterIndex++].Value = unit;
				}

				DataSet ds = GetDataSet(ConnectionString, sb.ToString(), parameters.ToArray());

				return ds;
			}
			#endregion public DataSet Gets(string unit = "")
		}
	}
}