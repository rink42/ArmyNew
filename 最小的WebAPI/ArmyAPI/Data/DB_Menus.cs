#define DEBUG // 定义 DEBUG 符号
using ArmyAPI.Models;
using System;
using System.Collections.Generic;

namespace ArmyAPI.Data
{
	public partial class MsSqlDataProvider : IDisposable
	{
		public class DB_Menus : MsSqlDataProvider
		{
			#region static DB_Menus GetInstance ()
			public static DB_Menus GetInstance()
			{
				return (new DB_Menus());
			}
			#endregion static DB_Menus GetInstance ()

			#region 建構子
			public DB_Menus()
			{
			}
			public DB_Menus(string connectionString) : base(connectionString, typeof(DB_Menus))
			{
			}
			#endregion 建構子

			#region List<Menus> GetAll()
			public List<Menus> GetAll()
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("SELECT *");
				sb.AppendLine("FROM Menus");
				#endregion CommandText

				List<Menus> result = Get<Menus>(ConnectionString, sb.ToString(), null);

				return result;
			}
            #endregion List<Menus> GetAll()
        }
    }
}