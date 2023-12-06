using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace testWinForm
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			LoadTreeView();

		}

		private void LoadTreeView()
		{
			string jsonString = System.IO.File.ReadAllText(@"D:\_My Documents\Visual Studio 2022\_Code\Web\國軍常備兵\testConsole\ConsoleApp1\bin\Debug\ArmyUnits.txt");
			List<TreeNodeData> treeData = JsonConvert.DeserializeObject<List<TreeNodeData>>(jsonString);

			treeData = treeData.OrderBy(node => node.level).ToList();

			// Create a dictionary for quick lookup
			Dictionary<string, TreeNode> nodeDictionary = new Dictionary<string, TreeNode>();

			int l_index = 0;
			string l_title = "";
			int r_index = 0;
			string r_title = "";
			int g_index = 0;
			string g_title = "";
			int b_index = 0;
			string b_title = "";
			int c_index = 0;
			string c_title = "";

			foreach (TreeNodeData data in treeData)
			{
				MyTreeNode newNode = new MyTreeNode(data.title);
				newNode.UnitCode = data.unit_code;
				newNode.ParentUnitCode = data.parent_unit_code;
				newNode.Level = data.level;

				//switch (data.level)
				//{

				//}

				nodeDictionary[data.unit_code] = newNode;

				if (data.parent_unit_code == null)
				{
					treeView1.Nodes.Add(newNode);
				}
				else
				{
					if (nodeDictionary.TryGetValue(data.parent_unit_code, out TreeNode parentNode))
					{
						parentNode.Nodes.Add(newNode);
					}
				}
			}

			//Write_v_Units(treeView1.Nodes);
		}

		private void Write_v_Units(TreeNodeCollection nodes)
		{
			string sqlCmd = @"
DECLARE	@StartDate DATETIME;
DECLARE @EndDate DATETIME;
DECLARE @Status CHAR(1);


SELECT @StartDate = start_date, @EndDate = end_date, @Status = unit_status
FROM Army.dbo.v_mu_unit
WHERE 1=1
  AND unit_code = @unit_code

INSERT INOTO ArmyWeb.dbo.v_Units
			([UnitCode], [ParentUnitCode], [UnitTitle], [Status], [L_index], [L_title], [R_index], [R_title], [G_index], [G_title], [B_index], [B_title], [C_index], [C_title], [StartDate], [EndDate])
	VALUES (@unit_code, @parent_unit_code, @title, @Status, @L_index, @L_title, @R_index, @R_title, @G_index, @G_title, @B_index, @B_title, @C_index, @C_title, @StartDate, @EndDate)
			";

			foreach (MyTreeNode node in nodes)
			{
				if (node.Text != "")
				{
					// 先寫入自己
					List<SqlParameter> parameters = new List<SqlParameter>();
					int parameterIndex = 0;

					parameters.Add(new SqlParameter("@unit_code", SqlDbType.NVarChar, 5));
					parameters[parameterIndex++].Value = node.UnitCode;
					parameters.Add(new SqlParameter("@parent_unit_code", SqlDbType.NVarChar, 5));
					parameters[parameterIndex++].Value = node.ParentUnitCode;
					parameters.Add(new SqlParameter("@title", SqlDbType.NVarChar, 128));
					parameters[parameterIndex++].Value = node.Text;

					parameters.Add(new SqlParameter("@L_index", SqlDbType.Int));
					parameters[parameterIndex++].Value = "";
					parameters.Add(new SqlParameter("@L_title", SqlDbType.NVarChar, 50));
					parameters[parameterIndex++].Value = "";
					parameters.Add(new SqlParameter("@R_index", SqlDbType.Int));
					parameters[parameterIndex++].Value = "";
					parameters.Add(new SqlParameter("@R_title", SqlDbType.NVarChar, 50));
					parameters[parameterIndex++].Value = "";
					parameters.Add(new SqlParameter("@G_index", SqlDbType.Int));
					parameters[parameterIndex++].Value = "";
					parameters.Add(new SqlParameter("@G_title", SqlDbType.NVarChar, 50));
					parameters[parameterIndex++].Value = "";
					parameters.Add(new SqlParameter("@B_index", SqlDbType.Int));
					parameters[parameterIndex++].Value = "";
					parameters.Add(new SqlParameter("@B_title", SqlDbType.NVarChar, 50));
					parameters[parameterIndex++].Value = "";
					parameters.Add(new SqlParameter("@C_index", SqlDbType.Int));
					parameters[parameterIndex++].Value = "";
					parameters.Add(new SqlParameter("@C_title", SqlDbType.NVarChar, 50));
					parameters[parameterIndex++].Value = "";
					// 再寫入子節點
				}
			}
		}

		private class TreeNodeData
		{
			public string unit_code { get; set; }
			public string title { get; set; }
			public int level { get; set; }
			public string parent_unit_code { get; set; }
		}
	}

	public class MyTreeNode : TreeNode
	{
		public MyTreeNode(string title) {
			base.Text = title;
		}
		public string UnitCode { get; set; }
		public string ParentUnitCode { get; set; }
		public new int Level { get; set; }

		public int L_index { get; set; }
		public string L_title { get; set; }
		public int R_index { get; set; }
		public string R_title { get; set; }
		public int G_index { get; set; }
		public string G_title { get; set; }
		public int B_index { get; set; }
		public string B_title { get; set; }
		public int C_index { get; set; }
		public string C_title { get; set; }
}

	}