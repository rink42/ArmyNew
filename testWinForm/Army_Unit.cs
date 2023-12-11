using System.Collections.Generic;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using testWinForm;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace ArmyAPI.Models
{
    public class Army_Unit
    {

		[JsonProperty("unit_code", Order = 1)]
        public string code { get; set; }
		[JsonProperty(Order = 2)]
		public string title { get; set; }
		private int _level = 1;
		[JsonProperty(Order = 4)]
		public int level
		{
			get
			{
				if (this.parent != null)
					return this.parent.level + 1;
				return _level;
			}
		}
		[JsonProperty("parent_unit_code", Order = 3)]
		public string parent_code { get; set; }
		[JsonProperty(Order = 15)]
		public List<Army_Unit> children { get; set; }

		private Army_Unit _Parent = null;
		[JsonIgnore]
		public Army_Unit parent
		{
			get { return _Parent; }
			set
			{
				_Parent = value;

				//switch (level)
				//{
				//	case 1:
				//	case 2:
				//		_L_index = 0;
				//		_L_title = "";
				//		_R_index = 0;
				//		_R_title = "";
				//		_G_index = 0;
				//		_G_title = "";
				//		_B_index = 0;
				//		_B_title = "";
				//		_C_index = 0;
				//		_C_title = "";
				//		break;
				//	case 3:
				//		_L_index = parent.children.IndexOf(this) + 1;
				//		_L_title = title;
				//		break;
				//	case 4:
				//		_L_index = parent.parent.children.IndexOf(parent) + 1;
				//		_L_title = parent.title;
				//		_R_index = parent.children.IndexOf(this) + 1;
				//		_R_title = title;
				//		break;
				//	case 5:
				//		_L_index = parent.parent.parent.children.IndexOf(parent.parent) + 1;
				//		_L_title = parent.parent.title;
				//		_R_index = parent.children.IndexOf(parent) + 1;
				//		_R_title = parent.title;
				//		_G_index = parent.children.IndexOf(this) + 1;
				//		_G_title = title;
				//		break;
				//	case 6:
				//		_L_index = parent.parent.parent.parent.children.IndexOf(parent.parent.parent) + 1;
				//		_L_title = parent.parent.parent.title;
				//		_R_index = parent.parent.parent.children.IndexOf(parent.parent) + 1;
				//		_R_title = parent.parent.title;
				//		_G_index = parent.parent.children.IndexOf(parent) + 1;
				//		_G_title = parent.title;
				//		_B_index = parent.children.IndexOf(this) + 1;
				//		_B_title = title;
				//		break;
				//	case 7:
				//		_L_index = parent.parent.parent.parent.parent.children.IndexOf(parent.parent.parent.parent) + 1;
				//		_L_title = parent.parent.parent.parent.title;
				//		_R_index = parent.parent.parent.parent.children.IndexOf(parent.parent.parent) + 1;
				//		_R_title = parent.parent.parent.title;
				//		_G_index = parent.parent.parent.children.IndexOf(parent.parent) + 1;
				//		_G_title = parent.parent.title;
				//		_B_index = parent.parent.children.IndexOf(parent) + 1;
				//		_B_title = parent.title;
				//		_C_index = parent.children.IndexOf(this) + 1;
				//		_C_title = title;
				//		break;
				//}
			}
		}

		private Army_Unit _prev = null;
		[JsonIgnore]
		public Army_Unit prev
		{
			get
			{
				return _prev;
			}
		}
		private Army_Unit _next = null;
		[JsonIgnore]
		public Army_Unit next
		{
			get
			{
				return _next;
			}
		}

		public int index
		{
			get
			{
				int result = 1;
				if (_prev != null)
					result = _prev.index + 1;

				return result;
			}
		}
		private int _T_index = 0;
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

		[JsonIgnore]
		// 第2層使用(因為第1層就1個國防部)
		public int T_index
		{
			get
			{
				return _T_index;
			}
		}

		// 一級 
		[JsonProperty(Order = 5)]
		public int L_index
		{
			get
			{
				return _L_index;
			}
		}
		[JsonProperty(Order = 6)]
		public string L_title
		{
			get
			{
				string result = "";
				if (this.level == 2)
					result = this.title;
				else if (this.level > 2)
					result = this.parent.L_title;

				return result;
			}
		}
		// 旅級
		[JsonProperty(Order = 7)]
		public int R_index
		{
			get
			{
				return _R_index;
			}
		}
		[JsonProperty(Order = 8)]
		public string R_title
		{
			get
			{
				string result = "";
				if (this.level == 2)
					result = this.title;
				else if (this.level > 2)
					result = this.parent.R_title;

				return result;
			}
		}
		// 群級
		[JsonProperty(Order = 9)]
		public int G_index
		{
			get
			{
				return _G_index;
			}
		}
		[JsonProperty(Order = 10)]
		public string G_title
		{
			get
			{
				string result = "";
				if (this.level == 2)
					result = this.title;
				else if (this.level > 2)
					result = this.parent.G_title;

				return result;
			}
		}
		// 營級
		[JsonProperty(Order = 11)]
		public int B_index
		{
			get
			{
				return _B_index;
			}
		}
		[JsonProperty(Order = 12)]
		public string B_title
		{
			get
			{
				string result = "";
				if (this.level == 2)
					result = this.title;
				else if (this.level > 2)
					result = this.parent.B_title;

				return result;
			}
		}
		[JsonProperty(Order = 13)]
		// 連級
		public int C_index
		{
			get
			{
				return _C_index;
			}
		}
		[JsonProperty(Order = 14)]
		public string C_title
		{
			get
			{
				string result = "";
				if (this.level == 2)
					result = this.title;
				else if (this.level > 2)
					result = this.parent.C_title;

				return result;
			}
		}



		public static StringBuilder ModifiedCodes = new StringBuilder();

		private static int t = 1;
		private static int l = 1;
		private static int r = 1;
		private static int g = 1;
		private static int b = 1;
		private static int c = 1;
		public void Resets(Army_Unit parent)
		{
			this.parent = parent;
			if (parent != null)
			{
				this.parent_code = parent.code;

				int index = this.parent.children.IndexOf(this);

				_prev = (index == 0) ? null : this.parent.children[index - 1];
				_next = (index == this.parent.children.Count - 1) ? null : this.parent.children[index + 1];
			}

			switch (this.level)
			{
				case 2:
					_T_index = t++;
					break;
				case 3:
					_L_index = l++;
					break;
				case 4:
					_R_index = r++;
					break;
				case 5:
					_G_index = g++;
					break;
				case 6:
					_B_index = b++;
					break;
				case 7:
					_C_index = c++;
					break;
			}

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
