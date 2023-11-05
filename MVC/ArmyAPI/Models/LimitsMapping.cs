using System;

namespace ArmyAPI.Models
{
	public class LimitsMapping
	{
		public string LimitCode { get; set; }
        public int UserGroupIndex { get; set; }
        public int UserIndex { get; set; }
        public string Unit_Code { get; set; }
		public DateTime AddDatetime { get; set; }
	}
}
