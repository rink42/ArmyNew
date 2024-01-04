using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace ArmyAPI.Models
{
	public class UserDetailLimits
	{
		public string Key { get; set; }

		public List<string> Values { get; set; }
//		[JsonIgnore]
		public List<string> Texts { get; set; }
		public List<string> Where { get; set; }
	}
}