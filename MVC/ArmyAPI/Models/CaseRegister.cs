﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArmyAPI.Models
{
    public class CaseRegister
    {
        public string CaseId { get; set; }

        public string PrimaryUnit { get; set; } = null;

        public string CurrentPosition { get; set; } = null;

        public string Name { get; set; } = null;

        public string IdNumber { get; set; }

        public string Branch { get; set; } = null;

        public string Rank { get; set; } = null;

        public string OldRankCode { get; set; } = null;

        public string NewRankCode { get; set; } = null;

        public string EffectDate { get; set; }

        public string FormType { get; set; }
    }
}