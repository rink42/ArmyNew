using System;
using ArmyAPI.Commons;

namespace ArmyAPI.Models
{
	public class UserDetail
	{
        public string UserID { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 管理員
        /// </summary>
        public bool Role1 { get; set; }
        /// <summary>
        /// 線傳作業
        /// </summary>
        public bool Role2 { get; set; }
        /// <summary>
        /// 人事作業
        /// </summary>
        public bool Role3 { get; set; }
        /// <summary>
        /// 人事查詢
        /// </summary>
        public bool Role4 { get; set; }
        /// <summary>
        /// 查非陸軍
        /// </summary>
        public bool Role5 { get; set; }
        /// <summary>
        /// 帳號狀態
        /// </summary>
		public short Status { get; set; } = -1; // -2 停用 -1 申請中 0 審核中 1 通過
        /// <summary>
        /// IP1
        /// </summary>
		public string IPAddr1 { get; set; }
        /// <summary>
        /// IP2
        /// </summary>
        public string IPAddr2 { get; set; }
        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 民線
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 軍線
        /// </summary>
        public string PhoneMil { get; set; }
        /// <summary>
        /// 業管單位
        /// </summary>
        public string Units { get; set; }
	}
}
