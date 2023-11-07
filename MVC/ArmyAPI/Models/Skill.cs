using System;

namespace ArmyAPI.Models
{
	public class Skill
	{
        public string skill_code { get; set; }
        public string skill_desc { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string skill_status { get; set; }
	}
}
