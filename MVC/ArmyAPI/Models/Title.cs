using System;
<<<<<<< HEAD
using Microsoft.SqlServer.Server;
=======
>>>>>>> 1d406affd3ac667008e7a0a25cf64bfaed0662f5

namespace ArmyAPI.Models
{
	public class Title
	{
<<<<<<< HEAD
        private string _title_name = "";
        public string title_code { get; set; }
        public string title_name
        {
            get { return _title_name; }
            set { _title_name = value ?? value.Trim(); }
        }
=======
        public string title_code { get; set; }
        public string title_name { get; set; }
>>>>>>> 1d406affd3ac667008e7a0a25cf64bfaed0662f5
        [Newtonsoft.Json.JsonIgnore]
        public string title_id { get; set; }
	}
}
