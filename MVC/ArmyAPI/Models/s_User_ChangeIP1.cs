using System;

namespace ArmyAPI.Models
{
	public class s_User_ChangeIP1
	{

        public DateTime ApplyDate { get; set; }
        public int ApplyNO { get; set; }
		public string UserID { get; set; }
		/// <summary>
		/// 新 IP1
		/// </summary>
		public string IP_New { get; set; }
		/// <summary>
		/// 舊 IP1
		/// </summary>
		public string IP_Old{ get; set; }
		/// <summary>
		/// 申請狀態(NULL 未處理，1 同意，0 駁回
		/// </summary>
		public bool? ApplyStatus{ get; set; }
		/// <summary>
		/// 申請事由
		/// </summary>
		public string ApplyReason{ get; set; }
		/// <summary>
		/// 管理者同意/駁回事由
		/// </summary>
		public string AdminReason{ get; set; }
		/// <summary>
		/// 審核人ID
		/// </summary>
		public string ModifyUserID { get; set; }
	}
}
