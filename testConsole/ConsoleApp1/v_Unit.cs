using System.Collections.Generic;

namespace ArmyAPI.Models
{
	public class v_Unit
    {
        public string UnitCode { get; set; }
        public string ParentUnitCode { get; set; }
        public string UnitTitle { get; set; }
		public int Status { get; set; }
		public int L_index { get; set; }
        public string L_title { get; set; }
	}
}
