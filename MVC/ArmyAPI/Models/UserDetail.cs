using System;
using System.Diagnostics;
using ArmyAPI.Commons;
using static ArmyAPI.Models.Users;

namespace ArmyAPI.Models
{
	public class UserDetail
	{
		public enum Statuses : short
		{
			/// <summary>
			/// 駁回
			/// </summary>
			Reject = -3,
			/// <summary>
			/// 停用
			/// </summary>
			Disable = -2,
			/// <summary>
			/// 申請中
			/// </summary>
			InProgress = -1,
			/// <summary>
			/// 審核中(不使用)
			/// </summary>
			InReview = 0,
			/// <summary>
			/// 通過
			/// </summary>
			Enable = 1
		}

		/// <summary>
		/// 申請管道
		/// </summary>
		public enum Processes : byte
		{
			/// <summary>
			/// 非正式
			/// </summary>
			Informal = 0,
			/// <summary>
			/// 正式
			/// </summary>
			Formal = 1
		}


		/// <summary>
		/// 申請結果
		/// </summary>
		public enum Outcomes : byte
		{
			/// <summary>
			/// 不同意
			/// </summary>
			Reject = 0,
			/// <summary>
			/// 同意
			/// </summary>
			Agree = 1,
			/// <summary>
			/// 臨時用
			/// </summary>
			TempUse = 2
		}
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
        public string RankCode { get; set; }
        public string RankTitle { get; set; }
		/// <summary>
		/// 職稱
		/// </summary>
		public string TitleCode { get; set; }
		public string TitleName { get; set; }
		/// <summary>
		/// 編專
		/// </summary>
		public string SkillCode { get; set; }
		public string SkillDesc { get; set; }
		/// <summary>
		/// 帳號狀態
		/// </summary>
		public short? Status { get; set; } // 空值:未申請 -3 臨時用 -2 停用 -1 申請中 0 審核中 1 通過
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
        /// 申請日期
        /// </summary>
        public DateTime? ApplyDate { get; set; }
		/// <summary>
		/// 申請管理
		/// </summary>
		private byte? _Process = null;
		public byte? Process
		{
			get { return _Process; }
			set
			{
				if (value != null)
				{
					if (Enum.IsDefined(typeof(Processes), value))
					{
						_Process = (byte?)value;
					}
				}
			}
		}
		/// <summary>
		/// 申請事由
		/// </summary>
		public string Reason { get; set; }
        /// <summary>
        /// 審查意見
        /// </summary>
		public string Review { get; set; }
		/// <summary>
		/// 申請結果
		/// </summary>
		private byte? _Outcome = null;
		public byte? Outcome
		{
			get { return _Outcome; }
			set
			{
				if (value != null)
				{
					if (Enum.IsDefined(typeof(Outcomes), value))
					{
						_Outcome = (byte?)value;
					}
				}
			}
		}
		/// <summary>
		/// 權限
		/// </summary>
		public string Limits1 { get; set; }
		public System.Collections.Generic.List<UserDetailLimits> Limits2 { get; set; }
	}
}
