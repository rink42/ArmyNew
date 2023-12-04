using System;
using System.Collections.Generic;
using ArmyAPI.Commons;
using Newtonsoft.Json;

namespace ArmyAPI.Models
{
	public class Users
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

        private string _UserID = "";
        public string UserID
        {
            set
            {
                if (Users.CheckUserId(value))
                {
                    _UserID = value;

                    // 檢查 UserID 是否存在
                    
                    
                }
            }
            get
            {
                return _UserID;
            }
        }
        public string Name { get; set; }
        public string Rank { get; set; }
        public string Title { get; set; }
        public string Skill { get; set; }

        private short? _Status = null;
		public short? Status  // 空值:未申請 -3 駁回 -2 停用 -1 申請中 0 審核中 1 通過
		{
			get { return _Status; }
			set
			{
				if (Enum.IsDefined(typeof(Users.Statuses), value))
				{
					_Status = (short?)value;
				}
			}
		}
		public string IPAddr1 { get; set; }
        public string IPAddr2 { get; set; }
        [JsonIgnore]
        public string Password { get; set; }
        public string Email { get; set; }
        public string PhoneMil { get; set; }
        public string Phone { get; set; }
		public string ApplyDate { get; set; }
        public string LastLoginDate { get; set; }
        public int GroupID { get; set; } = 0; // 預設沒有群組

        [JsonIgnore]
        public List<int> MenuUserGroup = null;

        private byte? _Process = null;
        public byte? Process
        {
            get { return _Process; }
            set
            {
                if (Enum.IsDefined(typeof(Users.Processes), value))
                {
                    _Process = (byte?)value;
                }
            }
        }
        public string Reason { get; set; }
        public string Review { get; set; }

        private byte? _Outcome = null;
        public byte? Outcome
		{
			get { return _Outcome; }
			set
			{
				if (Enum.IsDefined(typeof(Users.Outcomes), value))
				{
					_Outcome = (byte?)value;
				}
			}
		}


		public static bool CheckUserId(string userId)
        {
			bool result = false;
			string msg = "";

			result = (new Class_TaiwanID()).Check(userId, out msg);

			if (!result)
				throw new Exception(msg);

			return result;
        }
	}
}
