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
		public class DB_ArmyUnits : MsSqlDataProvider
		{
			#region static DB_MenuUserGroup GetInstance ()
			public static DB_MenuUserGroup GetInstance()
			{
				return (new DB_MenuUserGroup());
			}
			#endregion static DB_MenuUserGroup GetInstance ()

			#region 建構子
			public DB_ArmyUnits()
			{
				_TableName = "ArmyUnits";
			}
			public DB_ArmyUnits(string connectionString) : base(connectionString, typeof(DB_Menus))
			{
				_TableName = "ArmyUnits";
			}
			#endregion 建構子

			#region int Add(ArmyUnits newUnits)
			public int Add(ArmyUnits newUnits)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				List<string> queries = new List<string>();
				List<object> parametersList = new List<object>();
				#region CommandText
				sb.AppendLine($"DELETE FROM {_TableName} ");
				queries.Add(sb.ToString());
				sb.Length = 0;

				sb.AppendLine($"INSERT INTO {_TableName} ");
				sb.AppendLine("    VALUES (@unit_code, @title, @level, @parent_unit_code, @sort) ");
				queries.Add(sb.ToString());
				sb.Length = 0;
				#endregion CommandText

				parametersList.Add(null);

				List<ArmyUnits> armyUnitsList = new List<ArmyUnits>();
				armyUnitsList.Add(newUnits);
				var flattenUnits = FlattenArmyUnits(armyUnitsList.ToArray());
				parametersList.Add(flattenUnits);

				int result = (new DapperHelper(BaseController._ConnectionString)).ExecuteTransaction(queries, parametersList);

				return result;
			}
			#endregion int Add(ArmyUnits newUnits)

			#region ArmyUnits GetAll()
			public ArmyUnits GetAll()
			{
				ArmyUnits result = new ArmyUnits();
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				int getlevel = int.Parse(ConfigurationManager.AppSettings.Get("GetArmyUnitLevel"));

				string tableName = "ArmyUnits AS au";
				#region CommandText
				for (int i = 0; i < getlevel; i++)
				{
					sb.AppendLine($"DECLARE @Level{i} TABLE (unit_code VARCHAR(14), title VARCHAR(100), level VARCHAR(1), sort INT, parent_unit_code VARCHAR(14))");
				}

				for (int i = 0; i < getlevel; i++)
				{
					sb.AppendLine($"--第{i}層 ");
					sb.AppendLine($"INSERT INTO @Level{i} ");
					sb.AppendLine("    SELECT au.unit_code, au.title, au.level, au.sort, au.parent_unit_code ");
					sb.AppendLine($"	FROM {tableName} ");
					sb.AppendLine("	WHERE 1=1 ");
					sb.AppendLine($"	  AND au.level = '{i + 1}' ");

					sb.AppendLine($"SELECT * FROM @Level{i} ORDER BY sort");
				}
				#endregion CommandText


				DataSet ds = GetDataSet(ConnectionString, sb.ToString(), null);

				result = ConvertToUnits(ds.Tables);

				return result;
			}
			#endregion ArmyUnits GetAll()


			#region private List<ArmyUnits> FlattenArmyUnits(ArmyUnits[] units) 把 巢狀 AarmyUnits 扁平化
			private List<ArmyUnits> FlattenArmyUnits(ArmyUnits[] units)
			{
				List<ArmyUnits> flattenedList = new List<ArmyUnits>();

				foreach (var au in units)
				{
					flattenedList.Add(au);

					if (au.children != null && au.children.Count > 0)
					{
						flattenedList.AddRange(FlattenArmyUnits(au.children.ToArray()));
						au.children = null; // Remove nested children
					}
				}

				return flattenedList;
			}
			#endregion private List<ArmyUnits> FlattenArmyUnits(ArmyUnits[] menus)

			#region int DeleteAll__s_Unit()
			public int DeleteAll__s_Unit()
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				List<string> queries = new List<string>();
				List<object> parametersList = new List<object>();
				string tableName = "s_Unit";
				#region CommandText
				sb.AppendLine($"DELETE FROM {tableName} ");
				queries.Add(sb.ToString());
				sb.Length = 0;
				#endregion CommandText

				parametersList.Add(null);

				int result = (new DapperHelper(BaseController._ConnectionString)).ExecuteTransaction(queries, parametersList);

				return result;
			}
			#endregion int DeleteAll__s_Unit()

			private ArmyUnits ConvertToUnits(DataTableCollection dataTables)
			{
				Dictionary<string, ArmyUnits> unitDictionary = new Dictionary<string, ArmyUnits>();

				foreach (DataTable dataTable in dataTables)
				{
					foreach (DataRow row in dataTable.Rows)
					{
						string code = row["unit_code"].ToString().Trim();
						string title = row["title"].ToString().Trim();
						string level = row["level"].ToString().Trim();
						string sort = row["sort"].ToString().Trim();
						string parentCode = row["parent_unit_code"].ToString().Trim();

						ArmyUnits currentUnit;
						if (unitDictionary.ContainsKey(code))
						{
							currentUnit = unitDictionary[code];
							currentUnit.title = title;
							currentUnit.level = level;
							currentUnit.sort = int.Parse(sort);
							currentUnit.parent_unit_code = parentCode;
						}
						else
						{
							currentUnit = new ArmyUnits { unit_code = code, title = title, level = level, sort = int.Parse(sort), parent_unit_code = parentCode };
							unitDictionary[code] = currentUnit;
						}

						if (!string.IsNullOrEmpty(parentCode) && unitDictionary.ContainsKey(parentCode))
						{
							ArmyUnits parentUnit = unitDictionary[parentCode];
							if (parentUnit != null)
							{
								if (parentUnit.children == null)
									parentUnit.children = new List<ArmyUnits>();

								parentUnit.children.Add(currentUnit);
							}
						}
					}
				}

				ArmyUnits rootUnit = unitDictionary[dataTables[0].Rows[0]["unit_code"].ToString()];

				return rootUnit;
			}
		}
	}
}