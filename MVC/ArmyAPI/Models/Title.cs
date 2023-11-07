using System;
using Microsoft.SqlServer.Server;

namespace ArmyAPI.Models
{
	public class Title
	{
        private string _title_name = "";
        public string title_code { get; set; }
        public string title_name
        {
            get { return _title_name; }
            set { _title_name = value ?? value.Trim(); }
        }
        [Newtonsoft.Json.JsonIgnore]
        public string title_id { get; set; }
	}
}
