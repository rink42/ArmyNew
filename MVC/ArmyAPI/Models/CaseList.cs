using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArmyAPI.Models
{
    public class CaseList
    {
        public string CaseId { get; set; }

        public string CreateMember { get; set;}

        public string MemberId { get; set;}

        public string HostUrl { get; set; }

        public string PdfName { get; set;}

        public string ExcelName { get; set;}

        public string CreateDate { get; set;}
    }
}