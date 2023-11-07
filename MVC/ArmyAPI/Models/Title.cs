using System;

namespace ArmyAPI.Models
{
	public class Title
	{
        public string title_code { get; set; }
        public string title_name { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string title_id { get; set; }
	}
}
