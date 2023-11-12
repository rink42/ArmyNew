using System;
using ArmyAPI.Commons;

namespace ArmyAPI.Models
{
	public class UserDetail
	{
        public string UserID { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 單位
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// 級職
        /// </summary>
        public string Rank { get; set; }
        /// <summary>
        /// 職稱
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 編專
        /// </summary>
        public string Skill { get; set; }
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
        /// 權限
        /// </summary>
        public System.Collections.Generic.List<UserDetailLimits> Limits { get; set; }
	}
}
