using System;

namespace ArmyAPI.Models
{
	public class Users
	{
        public string UserID { get; set; }
        public string Name { get; set; }
        public int Status { get; set; } = 0; // 預設 0 不啟用，1 啟用，2 申請中，3 審核中
        public string IPAddr1 { get; set; }
        public string IPAddr2 { get; set; }
        public string Password { get; set; }
        public string TransPassword { get; set; }
        public string Email { get; set; }
        public string PhoneMil { get; set; }
        public string Phone { get; set; }
		public DateTime ApplyDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public int GroupID { get; set; } = 0; // 預設沒有群組
	}
}
