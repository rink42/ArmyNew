using System.Collections.Generic;

namespace ArmyAPI.Models
{
	public class ArmyUnits
    {
        public string unit_code { get; set; }
        public string title { get; set; }
        public string level { get; set; }
        public int sort { get; set; }
        public bool changed { get; set; }
		public string parent_unit_code { get; set; }
        public List<ArmyUnits> children { get; set; }

        public ArmyUnits FindUnit(string _parentCode, string _level)
        {
            ArmyUnits result = null;

            if (unit_code == _parentCode && level == _level)
                result = this;
            else
            {
                foreach (var child in children)
                {
                    result = child.FindUnit(_parentCode, _level);
                    if (result != null)
                        break;
                }
            }

            return result;
        }

        public bool AddChildren(string unitCode, string title, string level, int sort, string parentCode)
        {
            bool result = false;
            if (children == null)
                children = new List<ArmyUnits>();

            if (unit_code == parentCode)
            {
                ArmyUnits unit = new ArmyUnits();
                unit.unit_code = unitCode;
                unit.title = title;
                unit.level = level;
                unit.sort = sort;
                unit.parent_unit_code = unit_code;

                children.Add(unit);

                result = true;
            }
            else
            {
                foreach (var child in children)
                {
                    child.AddChildren(unitCode, title, level, sort, parentCode);
                }
            }

            return result;
        }

        public string GetAllChildUnitCode()
        {
            string result = unit_code[0] != 'v' ? unit_code : "";

            if (children != null)
            {
                foreach (var child in children)
                {
                    if (!string.IsNullOrEmpty(result))
                        result += ",";

                    if (child != null)
                        result += child.GetAllChildUnitCode();
                }
            }

            return result;
        }

        public static ArmyUnits FindByUnitCode(ArmyUnits unit, string unitCode)
        {
            ArmyUnits result = null;

            if (unit.unit_code == unitCode)
                result = unit;
            else if (unit.children != null && unit.children.Count > 0)
            {
                foreach (ArmyUnits child in unit.children)
                {
                    result = ArmyUnits.FindByUnitCode(child, unitCode);

                    if (result != null)
                        break;
                }
            }

            return result;
        }
	}
}
