using System.Collections.Generic;
using Newtonsoft.Json;

namespace ArmyAPI.Models
{
    public class Army_Unit
    {

		[JsonProperty("unit_code")]
        public string code { get; set; }
		public string title { get; set; }
        public string level { get; set; }
		[JsonProperty("parent_unit_code")]
		public string parent_code { get; set; }
		public List<Army_Unit> children { get; set; }

		public static void ResetLevel(ref Army_Unit units, int _level = 1, string parentCode = null)
		{
			units.level = _level.ToString();
			units.parent_code = parentCode;

			if (units.children != null && units.children.Count > 0)
			{
				for (int i = 0; i < units.children.Count; i++)
				{
					var unit = units.children[i];

					Army_Unit.ResetLevel(ref unit, _level + 1, units.code);
				}
			}
		}

		public void CopyTo(ArmyUnits armyUnits)
		{
			armyUnits.unit_code = this.code.Length > 5 ? this.code.Substring(0, 5) : this.code;
			armyUnits.title = this.title.Length > 100 ? this.title.Substring(0, 100) : this.title;
			armyUnits.level = this.level;
			armyUnits.parent_unit_code = (!string.IsNullOrEmpty(this.parent_code) && this.parent_code.Length > 5) ? this.parent_code.Substring(0, 5) : this.parent_code;

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
	}
}
