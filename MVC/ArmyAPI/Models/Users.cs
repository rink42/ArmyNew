using System;
using ArmyAPI.Commons;

namespace ArmyAPI.Models
{
	public class Users
	{
        private string _UserID = "";
        public string UserID
        {
            set
            {
                if (Users.CheckUserId(value))
                {
                    _UserID = value;
                }
            }
            get
            {
                return _UserID;
            }
        }
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
