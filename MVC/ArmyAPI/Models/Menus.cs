using System;
using System.Collections.Generic;

namespace ArmyAPI.Models
{
    public class Menus
    {

        public int Index { get; set; }
        public string Title { get; set; }
        public int ParentIndex { get; set; }
        public DateTime CreateDatetime { get; set; }

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
}
