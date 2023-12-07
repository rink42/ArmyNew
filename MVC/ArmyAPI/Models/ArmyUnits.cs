using System.Collections.Generic;

namespace ArmyAPI.Models
{
	public class ArmyUnits
    {
        public string unit_code { get; set; }
        public string title { get; set; }
        public string level { get; set; }
        public int sort { get; set; }
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
	}
}
