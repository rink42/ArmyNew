using System.Collections.Generic;

namespace ArmyAPI.Models
{
    public class Army_Unit
    {

        public string code { get; set; }
        public string title { get; set; }
        public string level { get; set; }
        //public string parent_code { get; set; }
		public List<Army_Unit> Items { get; set; }
    }
}
