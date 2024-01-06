using System;
using System.Collections.Generic;
using System.Data;

namespace ArmyAPI.Models
{
	public class Menus
    {

        public int Index { get; set; }
        public int Sort { get; set; }
        public string Title { get; set; }
		public int ParentIndex { get; set; }
        public string Route_Tableau { get; set; }
		public short Level { get; set; }
        public bool IsEnable { get; set; }
		public bool IsFix { get; set; }
		public bool IsCheck { get; set; } = false;
		public DateTime AddDatetime { get; set; }
        public DateTime ModifyDatetime { get; set; }
		public string ModifyUserID { get; set; }

		public List<Menus> Children { get; set; }

        public Menus FindByIndex(int index)
        {
            Menus result = null;
            if (this.Index == index)
                result = this;

            if (result ==  null && this.Children != null)
            {
                foreach (var m in this.Children)
                {
                    result = m.FindByIndex(index);

                    if (result != null)
                        break;
                }
            }
            return result;
        }

		public DataTable ToDataTable()
		{
			DataTable dt = new DataTable("Menus");
			dt.Columns.Add("Index", typeof(int));
			dt.Columns.Add("Sort", typeof(int));
			dt.Columns.Add("Title", typeof(string));
			dt.Columns.Add("ParentIndex", typeof(int));
			dt.Columns.Add("Route_Tableau", typeof(string));
			dt.Columns.Add("Level", typeof(int));
			dt.Columns.Add("IsEnable", typeof(bool));
			dt.Columns.Add("IsFix", typeof(bool));
			dt.Columns.Add("AddDatetime", typeof(DateTime));
			dt.Columns.Add("ModifyDatetime", typeof(DateTime));
			dt.Columns.Add("ModifyUserID", typeof(string));

			AddToDataTable(dt);

			return dt;
		}

		private void AddToDataTable(DataTable dt)
		{
			DataRow row = dt.NewRow();
			row["Index"] = this.Index;
			row["Sort"] = this.Sort;
			row["Title"] = this.Title;
			row["ParentIndex"] = this.ParentIndex;
			row["Route_Tableau"] = this.Route_Tableau;
			row["Level"] = this.ParentIndex;
			row["IsEnable"] = this.IsEnable;
			row["IsFix"] = this.IsFix;
			row["AddDatetime"] = this.AddDatetime;
			row["ModifyDatetime"] = this.ModifyDatetime;
			row["ModifyUserID"] = this.ModifyUserID;
			dt.Rows.Add(row);

			if (this.Children != null)
			{
				foreach (var item in this.Children)
				{
					item.AddToDataTable(dt);
				}
			}
		}
	}

    public class ChangeParent
    {
        /// <summary>
        /// Old_Id
        /// </summary>
        public int o { get; set; }
        /// <summary>
        /// New_Id
        /// </summary>
        public int n { get; set; }
    }
}
