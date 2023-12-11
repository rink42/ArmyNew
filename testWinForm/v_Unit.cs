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
		public int R_index { get; set; }
        public string R_title { get; set; }
		public int G_index { get; set; }
        public string G_title { get; set; }
		public int B_index { get; set; }
        public string B_title { get; set; }
		public int C_index { get; set; }
        public string C_title { get; set; }
		public string StartDate { get; set; }
		public string EndDate { get; set; }
	}
}
