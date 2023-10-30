using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ArmyAPI.Models
{
    public class Menus
    {

        public int Index { get; set; }
        public int Sort { get; set; }
        public string Title { get; set; }
		public int ParentIndex { get; set; }
        public string Route_Tableau { get; set; }
        public bool IsEnable { get; set; }
        [JsonIgnore]
        public bool IsFix { get; set; }
		public DateTime AddDatetime { get; set; }
        public DateTime ModifyDatetime { get; set; }
		public string ModifyUserId { get; set; }

		public List<Menus> Items { get; set; }

        public Menus FindByIndex(int index)
        {
            Menus result = null;
            if (this.Index == index)
                result = this;

            if (result ==  null && this.Items != null)
            {
                foreach (var m in this.Items)
                {
                    result = m.FindByIndex(index);

                    if (result != null)
                        break;
                }
            }
            return result;
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
