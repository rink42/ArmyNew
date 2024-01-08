using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace ArmyAPI.Models
{
	[XmlRoot("TableauConfig")]
	public class TableauConfig
	{
		[XmlElement("List")]
		public List<TableauConfig_Item> List { get; set; }
	}

	public class TableauConfig_Item
	{
		[XmlElement("Name")]
		public string Name { get; set; }

		[XmlElement("Link")]
		public string Link { get; set; }

		[XmlElement("Table")]
		public string Table { get; set; }

		[XmlElement("SQL")]
		public string SQL { get; set; }
	}
}