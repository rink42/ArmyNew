using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Xml;
using ArmyAPI.Models;

namespace ArmyAPI.Data
{
	public class XML_TableauConfig
	{
		public List<TableauConfig_Item> GetAll()
		{
			List<TableauConfig_Item> itemList = null;

			string file = HttpContext.Current.Server.MapPath("../File/TableauConfig.xml");
			if (File.Exists(file))
			{
				// 匯入 XML
				XmlDocument xd = new XmlDocument();
				xd.Load(file);

				itemList = new List<TableauConfig_Item>();

				XmlNodeList nodelist = xd.SelectNodes("/TableauConfig/List");

				for (int l = 0; l < nodelist.Count; l++)
				{
					XmlNodeList nodeItems = nodelist[l].SelectNodes("Item");

					foreach (XmlNode xn in nodeItems)
					{
						TableauConfig_Item item = new TableauConfig_Item();
						item.Name = xn.ChildNodes[0].InnerText;
						item.Link = xn.ChildNodes[1].InnerText;
						item.Table = xn.ChildNodes[2].InnerText;
						item.SQL = xn.ChildNodes[3].InnerText;

						itemList.Add(item);
					}
				}
			}

			return itemList;
		}
	}
}