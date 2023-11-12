using System;

namespace ArmyAPI.Models
{
	public class Limits
	{
        public string LimitCode { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public bool IsEnable { get; set; }
        public int Sort { get; set; }
		public DateTime AddDatetime { get; set; }
        public DateTime ModifyDatetime { get; set; }
		public string ModifyUserID { get; set; }
	}
}
