using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ArmyAPI.Models
{
    public class Army_Unit
    {

		[JsonProperty("unit_code")]
        public string code { get; set; }
		public string title { get; set; }
		private int _level = 1;
		public int level
		{
			get
			{
				if (this.parent != null)
					return this.parent.level + 1;
				return _level;
			}
		}
		[JsonProperty("parent_unit_code")]
		public string parent_code { get; set; }
		public List<Army_Unit> children { get; set; }

		private Army_Unit _Parent = null;
		[JsonIgnore]
		public Army_Unit parent
		{
			get { return _Parent; }
			set
			{
				_Parent = value;

				switch (level)
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
						_L_index = parent.children.IndexOf(this) + 1;
						_L_title = title;
						break;
					case 4:
						_L_index = parent.parent.children.IndexOf(parent) + 1;
						_L_title = parent.title;
						_R_index = parent.children.IndexOf(this) + 1;
						_R_title = title;
						break;
					case 5:
						_L_index = parent.parent.parent.children.IndexOf(parent.parent) + 1;
						_L_title = parent.parent.title;
						_R_index = parent.children.IndexOf(parent) + 1;
						_R_title = parent.title;
						_G_index = parent.children.IndexOf(this) + 1;
						_G_title = title;
						break;
					case 6:
						_L_index = parent.parent.parent.parent.children.IndexOf(parent.parent.parent) + 1;
						_L_title = parent.parent.parent.title;
						_R_index = parent.parent.parent.children.IndexOf(parent.parent) + 1;
						_R_title = parent.parent.title;
						_G_index = parent.parent.children.IndexOf(parent) + 1;
						_G_title = parent.title;
						_B_index = parent.children.IndexOf(this) + 1;
						_B_title = title;
						break;
					case 7:
						_L_index = parent.parent.parent.parent.parent.children.IndexOf(parent.parent.parent.parent) + 1;
						_L_title = parent.parent.parent.parent.title;
						_R_index = parent.parent.parent.parent.children.IndexOf(parent.parent.parent) + 1;
						_R_title = parent.parent.parent.title;
						_G_index = parent.parent.parent.children.IndexOf(parent.parent) + 1;
						_G_title = parent.parent.title;
						_B_index = parent.parent.children.IndexOf(parent) + 1;
						_B_title = parent.title;
						_C_index = parent.children.IndexOf(this) + 1;
						_C_title = title;
						break;
				}
			}
		}

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

		//public static void ResetLevel(ref Army_Unit units, int _level = 1, string parentCode = null)
		//{
		//	units.level = _level.ToString();
		//	units.parent_code = parentCode;

		//	if (units.children != null && units.children.Count > 0)
		//	{
		//		for (int i = 0; i < units.children.Count; i++)
		//		{
		//			var unit = units.children[i];

		//			Army_Unit.ResetLevel(ref unit, _level + 1, units.code);
		//		}
		//	}
		//}

		public void CopyTo(ArmyUnits armyUnits)
		{
			armyUnits.unit_code = this.code.Length > 14 ? this.code.Substring(0, 14) : this.code;
			armyUnits.title = this.title.Length > 100 ? this.title.Substring(0, 100) : this.title;
			armyUnits.level = this.level.ToString();
			armyUnits.parent_unit_code = (!string.IsNullOrEmpty(this.parent_code) && this.parent_code.Length > 14) ? this.parent_code.Substring(0, 14) : this.parent_code;

			if (this.children != null && this.children.Count > 0)
			{
				armyUnits.children = new List<ArmyUnits>();
				foreach (var child in this.children)
				{
					var childArmyUnits = new ArmyUnits();
					child.CopyTo(childArmyUnits);
					armyUnits.children.Add(childArmyUnits);
				}
			}
		}

		public static StringBuilder ModifiedCodes = new StringBuilder();

		public void Resets(Army_Unit parent)
		{
			this.parent = parent;
			if (Army_Unit.ModifiedCodes.Length == 0 || Army_Unit.ModifiedCodes.ToString()[Army_Unit.ModifiedCodes.Length - 1] != ',')
				Army_Unit.ModifiedCodes.Append(",");
			if (this.code.Length > 5)
			{
				int index = 0;
				if (this.parent != null)
				{
					index = this.parent.children.IndexOf(this) + 1;
				}

				if (this.level != 7)
				{
					string oldCode = this.code;
					char[] levels = { 'L', 'R', 'G', 'B', 'C' };

					// 規則，第1碼: v，第2碼: 依 level 值取 levels，第1~3碼 取 上一層UnitCode的 3~5碼, 第6~8碼為 count 值
					string newCode = "";
					string parentCode = this.parent_code.Substring(0, 3);
					if (this.parent_code[0] == 'v')
						parentCode = this.parent_code.Substring(2, 3);
					do
					{
						newCode = $"v{levels[this.level - 2]}{parentCode}{index++.ToString("D3")}";

					} while (Army_Unit.ModifiedCodes.ToString().IndexOf($",{newCode},") >= 0);
					this.code = newCode;
					Army_Unit.ModifiedCodes.Append(newCode + ",");

					// 對所有指向這個 code 的 parentCode 修改
					if (this.children != null && this.children.Count > 0)
					{
						foreach (var child in this.children)
						{
							child.parent_code = newCode;
						}
					}
				}
			}

			if (this.children != null && this.children.Count > 0)
			{
				foreach (var child in this.children)
				{
					child.Resets(this);
				}
			}
		}
	}
}
