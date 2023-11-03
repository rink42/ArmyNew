using System;

namespace ArmyAPI.Models
{
	public class UserGroup
	{

        public int Index { get; set; }
        public int Sort { get; set; }
        public string Title { get; set; }
        public bool IsEnable { get; set; }
		public DateTime AddDatetime { get; set; }
        public DateTime ModifyDatetime { get; set; }
		public string ModifyUserId { get; set; }
	}
}
