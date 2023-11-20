using System.Collections.Generic;

namespace ArmyAPI.Models
{
    public class ArmyUnits
    {

        public string unit_code { get; set; }
        public string title { get; set; }
        public string level { get; set; }
		public string parent_unit_code { get; set; }
		public List<ArmyUnits> children { get; set; }

	}
}
