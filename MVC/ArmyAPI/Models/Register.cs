using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ArmyAPI.Models
{
    public class Register
    {
        public string PrimaryUnit { get; set; } = null;

        public string CurrentPosition { get; set; } = null;

        public string Name { get; set; } = null;

        public string IdNumber { get; set; }

        public string Branch { get; set; } = null;

        public string Rank { get; set; } = null;

        public string OldRankCode { get; set; } = null;

        public string NewRankCode { get; set; } = null;

        public DateTime? EffectDate { get; set; }

        public string FormType { get; set; }
    }
}