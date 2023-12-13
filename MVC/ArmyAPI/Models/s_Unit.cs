using System;
using System.Collections.Generic;
using CrystalDecisions.Shared;
using Newtonsoft.Json;

namespace ArmyAPI.Models
{
	public class s_Unit
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
		public System.DateTime? StartDate { get; set; }
		public System.DateTime? EndDate { get; set; }
		public int Sort { get; set; }
		public int Level { get; set; }

		[JsonIgnore]
		public List<s_Unit> children { get; set; }
	}
}
