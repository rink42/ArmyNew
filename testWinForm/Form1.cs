using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using ArmyAPI.Data;
using ArmyAPI.Models;
using Dapper;
using Newtonsoft.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace testWinForm
{
	public partial class Form1 : Form
	{
		private string _ConnectionString = "Server=192.168.42.62;Database=ArmyWeb;User Id=sa;Password=syscom;Connect Timeout=600";

		private string _SqlCmd = @"
INSERT INTO ArmyWeb.dbo.s_Unit
			([UnitCode], [ParentUnitCode], [UnitTitle], [Status], [L_index], [L_title], [R_index], [R_title], [G_index], [G_title], [B_index], [B_title], [C_index], [C_title], [StartDate], [EndDate])
	VALUES (@UnitCode, @ParentUnitCode, @UnitTitle, @Status, @L_index, @L_title, @R_index, @R_title, @G_index, @G_title, @B_index, @B_title, @C_index, @C_title, @StartDate, @EndDate)
			";

		private MsSqlDataProvider _DB = new MsSqlDataProvider();

		public Form1()
		{
			InitializeComponent();
			//LoadTreeView();

			SetArmyUnit();
		}

		#region void LoadTreeView()
		private void LoadTreeView()
		{
			string jsonString = System.IO.File.ReadAllText(@"D:\_My Documents\Visual Studio 2022\_Code\Web\國軍常備兵\testConsole\ConsoleApp1\bin\Debug\ArmyUnits.txt");
			List<TreeNodeData> treeData = JsonConvert.DeserializeObject<List<TreeNodeData>>(jsonString);

			treeData = treeData.OrderBy(node => node.level).ToList();

			// Create a dictionary for quick lookup
			Dictionary<string, MyTreeNode> nodeDictionary = new Dictionary<string, MyTreeNode>();

			int virtualIndex = 1;

			int sort = 0;

			foreach (TreeNodeData data in treeData)
			{


				MyTreeNode newNode = new MyTreeNode(data.title);
				newNode.UnitCode = data.unit_code;
				newNode.ParentUnitCode = data.parent_unit_code;
				newNode.Level = data.level;

				nodeDictionary[data.unit_code] = newNode;

				if (data.parent_unit_code == null)
				{
					treeView1.Nodes.Add(newNode);
				}
				else
				{
					if (nodeDictionary.TryGetValue(data.parent_unit_code, out MyTreeNode parentNode))
					{
						newNode.ParentNode = parentNode;


						parentNode.Nodes.Add(newNode);
					}
				}
			}

			Write_v_Units(treeView1.Nodes);

			using (IDbConnection conn = new SqlConnection(_ConnectionString))
			{
				conn.Execute(_SqlCmd, paras);
			}
		}
		#endregion void LoadTreeView()

		private List<v_Unit> paras = new List<v_Unit>();
		#region void Write_v_Units(TreeNodeCollection nodes)
		private void Write_v_Units(TreeNodeCollection nodes)
		{
			foreach (MyTreeNode node in nodes)
			{
				if (node.Text != "")
				{
					string armySqlCmd = @"
SELECT start_date, end_date, unit_status
FROM Army.dbo.v_mu_unit
WHERE 1=1
  AND unit_code = @unit_code";

					List<SqlParameter> parameters = new List<SqlParameter>();
					int parameterIndex = 0;

					parameters.Add(new SqlParameter("@unit_code", SqlDbType.Char, 5));
					parameters[parameterIndex++].Value = node.UnitCode;

					string status = "";
					string startDate = "";
					string endDate = "";

					_DB.GetDataReturnDataTable(_ConnectionString, armySqlCmd, parameters.ToArray());
					DataTable dt = _DB.ResultDataTable;

					if (dt != null && dt.Rows.Count > 0)
					{
						status = dt.Rows[0]["unit_status"].ToString();
						startDate = dt.Rows[0]["start_date"].ToString();
						endDate = dt.Rows[0]["end_date"].ToString();
					}

					v_Unit unit = new v_Unit();
					unit.UnitCode = node.UnitCode;
					unit.ParentUnitCode = node.ParentUnitCode ?? "";
					unit.UnitTitle = node.Text;
					unit.L_index = node.L_index;
					unit.L_title = node.L_title;
					unit.R_index = node.R_index;
					unit.R_title = node.R_title;
					unit.G_index = node.G_index;
					unit.G_title = node.G_title;
					unit.B_index = node.B_index;
					unit.B_title = node.B_title;
					unit.C_index = node.C_index;
					unit.C_title = node.C_title;
					if (status != "")
						unit.Status = int.Parse(status);
					unit.StartDate = startDate;
					unit.EndDate = endDate;

					paras.Add(unit);

					// 再寫入子節點
					if (node.Nodes != null && node.Nodes.Count > 0)
						Write_v_Units(node.Nodes);
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
		#endregion void Write_v_Units(TreeNodeCollection nodes)


		private void SetArmyUnit()
		{
			string all = System.IO.File.ReadAllText(@"D:\20231207.txt");
			List<Army_Unit> units = JsonConvert.DeserializeObject<List<Army_Unit>>(all);

			units[0].Resets(null);
			Army_Unit.ModifiedCodes.Length = 0;

			System.IO.File.WriteAllText("treeView1.txt", JsonConvert.SerializeObject(units));

			foreach (var unit in units)
			{
				MyTreeNode node = CreateTreeNode(unit);
				treeView1.Nodes.Add(node);
			}


			//Write_v_Units1(units);

			//using (IDbConnection conn = new SqlConnection(_ConnectionString))
			//{
			//	conn.Execute(_SqlCmd, paras);
			//}
		}


		#region void Write_v_Units1(List<Army_Unit> nodes)
		private void Write_v_Units1(List<Army_Unit> nodes)
		{
			var units = nodes.Where(n => n.title != "").Select(n => n.code).ToList();

			foreach (Army_Unit node in nodes)
			{
				System.Threading.Thread.Sleep(1);
				Application.DoEvents();
				if (node.title != "")
				{
					string armySqlCmd = @"
SELECT start_date, end_date, unit_status
FROM Army.dbo.v_mu_unit
WHERE 1=1
  AND unit_code = @unit_code";

					List<SqlParameter> parameters = new List<SqlParameter>();
					int parameterIndex = 0;

					parameters.Add(new SqlParameter("@unit_code", SqlDbType.Char, 5));
					parameters[parameterIndex++].Value = node.code;

					string status = "";
					string startDate = "";
					string endDate = "";

					_DB.GetDataReturnDataTable(_ConnectionString, armySqlCmd, parameters.ToArray());
					DataTable dt = _DB.ResultDataTable;

					if (dt != null && dt.Rows.Count > 0)
					{
						status = dt.Rows[0]["unit_status"].ToString();
						startDate = dt.Rows[0]["start_date"].ToString();
						endDate = dt.Rows[0]["end_date"].ToString();
					}

					v_Unit unit = new v_Unit();
					unit.UnitCode = node.code;
					unit.ParentUnitCode = node.parent_code ?? "";
					unit.UnitTitle = node.title;
					unit.L_index = node.L_index;
					unit.L_title = node.L_title;
					unit.R_index = node.R_index;
					unit.R_title = node.R_title;
					unit.G_index = node.G_index;
					unit.G_title = node.G_title;
					unit.B_index = node.B_index;
					unit.B_title = node.B_title;
					unit.C_index = node.C_index;
					unit.C_title = node.C_title;
					if (status != "")
						unit.Status = int.Parse(status);
					unit.StartDate = startDate;
					unit.EndDate = endDate;

					paras.Add(unit);

					// 再寫入子節點
					if (node.children != null && node.children.Count > 0)
						Write_v_Units1(node.children);
				}
			}
		}
		#endregion void Write_v_Units1(List<Army_Unit> nodes)

		private MyTreeNode CreateTreeNode(Army_Unit unit)
		{
			MyTreeNode node = new MyTreeNode(unit.title);
			node.Tag = unit; // Store the corresponding Army_Unit object in the Tag property

			// Recursively create child nodes
			if (unit.children != null && unit.children.Count > 0)
			{
				foreach (var child in unit.children)
				{
					MyTreeNode childNode = CreateTreeNode(child);
					node.Nodes.Add(childNode);
				}
			}

			return node;
		}
	}



	#region class MyTreeNode : TreeNode
	public class MyTreeNode : TreeNode
	{
		public MyTreeNode(string title)
		{
			base.Text = title;
		}

		private MyTreeNode _ParentNode = null;
		public MyTreeNode ParentNode
		{
			get { return _ParentNode; }
			set
			{
				_ParentNode = value;

				switch (Level)
				{
					case 1:
					case 2:
						_L_index = 0;
						_L_title = "";
						_R_index = 0;
						_R_title = "";
						_G_index = 0;
						_G_title = "";
						_B_index = 0;
						_B_title = "";
						_C_index = 0;
						_C_title = "";
						break;
					case 3:
						_L_index = ParentNode.Nodes.IndexOf(this) + 1;
						_L_title = Text;
						break;
					case 4:
						_L_index = ParentNode.ParentNode.Nodes.IndexOf(ParentNode) + 1;
						_L_title = ParentNode.Text;
						_R_index = ParentNode.Nodes.IndexOf(this) + 1;
						_R_title = Text;
						break;
					case 5:
						_L_index = ParentNode.ParentNode.ParentNode.Nodes.IndexOf(ParentNode.ParentNode) + 1;
						_L_title = ParentNode.ParentNode.Text;
						_R_index = ParentNode.Nodes.IndexOf(ParentNode) + 1;
						_R_title = ParentNode.Text;
						_G_index = ParentNode.Nodes.IndexOf(this) + 1;
						_G_title = Text;
						break;
					case 6:
						_L_index = ParentNode.ParentNode.ParentNode.ParentNode.Nodes.IndexOf(ParentNode.ParentNode.ParentNode) + 1;
						_L_title = ParentNode.ParentNode.ParentNode.Text;
						_R_index = ParentNode.ParentNode.ParentNode.Nodes.IndexOf(ParentNode.ParentNode) + 1;
						_R_title = ParentNode.ParentNode.Text;
						_G_index = ParentNode.ParentNode.Nodes.IndexOf(ParentNode) + 1;
						_G_title = ParentNode.Text;
						_B_index = ParentNode.Nodes.IndexOf(this) + 1;
						_B_title = Text;
						break;
					case 7:
						_L_index = ParentNode.ParentNode.ParentNode.ParentNode.ParentNode.Nodes.IndexOf(ParentNode.ParentNode.ParentNode.ParentNode) + 1;
						_L_title = ParentNode.ParentNode.ParentNode.ParentNode.Text;
						_R_index = ParentNode.ParentNode.ParentNode.ParentNode.Nodes.IndexOf(ParentNode.ParentNode.ParentNode) + 1;
						_R_title = ParentNode.ParentNode.ParentNode.Text;
						_G_index = ParentNode.ParentNode.ParentNode.Nodes.IndexOf(ParentNode.ParentNode) + 1;
						_G_title = ParentNode.ParentNode.Text;
						_B_index = ParentNode.ParentNode.Nodes.IndexOf(ParentNode) + 1;
						_B_title = ParentNode.Text;
						_C_index = ParentNode.Nodes.IndexOf(this) + 1;
						_C_title = Text;
						break;
				}
			}
		}


		public string UnitCode { get; set; }
		public string ParentUnitCode { get; set; }
		public new int Level { get; set; }

		private int _L_index = 0;
		private string _L_title = "";
		private int _R_index = 0;
		private string _R_title = "";
		private int _G_index = 0;
		private string _G_title = "";
		private int _B_index = 0;
		private string _B_title = "";
		private int _C_index = 0;
		private string _C_title = "";

		// 一級
		public int L_index
		{
			get
			{
				return _L_index;
			}
		}
		public string L_title
		{
			get
			{
				return _L_title;
			}
		}
		// 旅級
		public int R_index
		{
			get
			{
				return _R_index;
			}
		}
		public string R_title
		{
			get
			{
				return _R_title;
			}
		}
		// 群級
		public int G_index
		{
			get
			{
				return _G_index;
			}
		}
		public string G_title
		{
			get
			{
				return _G_title;
			}
		}
		// 營級
		public int B_index
		{
			get
			{
				return _B_index;
			}
		}
		public string B_title
		{
			get
			{
				return _B_title;
			}
		}
		// 連級
		public int C_index
		{
			get
			{
				return _C_index;
			}
		}
		public string C_title
		{
			get
			{
				return _C_title;
			}
		}
	}
	#endregion class MyTreeNode : TreeNode
}