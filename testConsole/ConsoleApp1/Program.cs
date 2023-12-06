using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArmyAPI.Data;

namespace ConsoleApp1
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Test1 t1 = new Test1();

			DataTable armyUnits = t1.ArmyUnits_GetAll();

			System.IO.File.WriteAllText("ArmyUnits.txt", Newtonsoft.Json.JsonConvert.SerializeObject(armyUnits));
		}

		public class Test1
		{
			private string _ConnectionString = "Server=192.168.42.62;Database=ArmyWeb;User Id=sa;Password=syscom;Connect Timeout=600";

			private MsSqlDataProvider _DB = new MsSqlDataProvider();

			public DataTable ArmyUnits_GetAll()
			{
				string sqlCmd = @"
                SELECT *
                FROM ArmyWeb.dbo.ArmyUnits
			";

				DataTable armyUnits = _DB.GetDataTable(_ConnectionString, sqlCmd, null);

				return armyUnits;
			}
		}
	}
}
