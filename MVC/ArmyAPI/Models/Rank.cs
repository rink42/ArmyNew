using System;

namespace ArmyAPI.Models
{
	public class Rank
	{
        public string rank_code { get; set; }
        public string rank_title { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string old_rank_code { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string rank_class { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string rank_report { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string rank_soft { get; set; }
	}
}
