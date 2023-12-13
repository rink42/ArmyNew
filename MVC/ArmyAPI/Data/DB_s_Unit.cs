#define DEBUG // 定义 DEBUG 符号
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using ArmyAPI.Commons;
using ArmyAPI.Models;

namespace ArmyAPI.Data
{
	public partial class MsSqlDataProvider : IDisposable
	{
		public class DB_s_Unit : MsSqlDataProvider
		{
			#region static DB_s_Unit GetInstance ()
			public static DB_s_Unit GetInstance()
			{
				return (new DB_s_Unit());
			}
			#endregion static DB_s_Unit GetInstance ()

			#region 建構子
			public DB_s_Unit()
			{
				_TableName = "s_Unit";
			}
			public DB_s_Unit(string connectionString) : base(connectionString, typeof(DB_Menus))
			{
				_TableName = "s_Unit";
			}
			#endregion 建構子

			#region s_Unit GetAll()
			public s_Unit GetAll()
			{
				s_Unit result = new s_Unit();
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				int getlevel = int.Parse(ConfigurationManager.AppSettings.Get("GetArmyUnitLevel"));

				string tableName = "s_Unit AS su";
				#region CommandText
				for (int i = 0; i < getlevel; i++)
				{
					sb.AppendLine($"DECLARE @Level{i} TABLE (UnitCode VARCHAR(14), UnitTitle VARCHAR(128), Level INT, Sort INT, ParentUnitCode VARCHAR(14))");
				}

				for (int i = 0; i < getlevel; i++)
				{
					sb.AppendLine($"--第{i}層 ");
					sb.AppendLine($"INSERT INTO @Level{i} ");
					sb.AppendLine("    SELECT su.UnitCode, su.UnitTitle, su.[Level], su.[Sort], su.ParentUnitCode ");
					sb.AppendLine($"	FROM {tableName} ");
					sb.AppendLine("	WHERE 1=1 ");
					sb.AppendLine($"	  AND su.[Level] = '{i + 1}' ");

					sb.AppendLine($"SELECT * FROM @Level{i} ORDER BY [Sort]");
				}
				#endregion CommandText

				DataSet ds = GetDataSet(ConnectionString, sb.ToString(), null);

				result = ConvertToUnits(ds.Tables);

				return result;
			}
			#endregion s_Unit GetAll()

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

			#region private s_Unit ConvertToUnits(DataTableCollection dataTables)
			private s_Unit ConvertToUnits(DataTableCollection dataTables)
			{
				Dictionary<string, s_Unit> unitDictionary = new Dictionary<string, s_Unit>();

				foreach (DataTable dataTable in dataTables)
				{
					foreach (DataRow row in dataTable.Rows)
					{
						string code = row["UnitCode"].ToString().Trim();
						string title = row["UnitTitle"].ToString().Trim();
						int level = int.Parse(row["Level"].ToString().Trim());
						string sort = row["Sort"].ToString().Trim();
						string parentCode = row["ParentUnitCode"].ToString().Trim();

						s_Unit currentUnit;
						if (unitDictionary.ContainsKey(code))
						{
							currentUnit = unitDictionary[code];
							currentUnit.UnitTitle = title;
							currentUnit.Level = level;
							currentUnit.Sort = int.Parse(sort);
							currentUnit.ParentUnitCode = parentCode;
						}
						else
						{
							currentUnit = new s_Unit { UnitCode = code, UnitTitle = title, Level = level, Sort = int.Parse(sort), ParentUnitCode = parentCode };
							unitDictionary[code] = currentUnit;
						}

						if (!string.IsNullOrEmpty(parentCode) && unitDictionary.ContainsKey(parentCode))
						{
							s_Unit parentUnit = unitDictionary[parentCode];
							if (parentUnit != null)
							{
								if (parentUnit.children == null)
									parentUnit.children = new List<s_Unit>();

								parentUnit.children.Add(currentUnit);
							}
						}
					}
				}

				s_Unit rootUnit = unitDictionary[dataTables[0].Rows[0]["UnitCode"].ToString()];

				return rootUnit;
			}
			#endregion private s_Unit ConvertToUnits(DataTableCollection dataTables)
		}
	}
}