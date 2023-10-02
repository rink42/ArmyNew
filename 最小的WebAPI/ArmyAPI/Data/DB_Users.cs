#define DEBUG // 定义 DEBUG 符号
using ArmyAPI.Models;
using System;
using System.Collections.Generic;

namespace ArmyAPI.Data
{
	public partial class MsSqlDataProvider : IDisposable
	{
		public class DB_Users : MsSqlDataProvider
		{
			#region static DB_Users GetInstance ()
			public static DB_Users GetInstance()
			{
				return (new DB_Users());
			}
			#endregion static DB_Users GetInstance ()

			#region 建構子
			public DB_Users()
			{
			}
			public DB_Users(string connectionString) : base(connectionString, typeof(DB_Users))
			{
			}
			#endregion 建構子

			#region List<Users> GetAll()
			public List<Users> GetAll()
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				#region CommandText
				sb.AppendLine("SELECT *");
				sb.AppendLine("FROM Users");
				#endregion CommandText

				List<Users> result = Get<Users>(ConnectionString, sb.ToString(), null);

				return result;
			}
            #endregion List<Users> GetAll()
        }
    }
}