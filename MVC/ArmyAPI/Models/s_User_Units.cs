namespace ArmyAPI.Models
{
	public class s_User_Units
	{
        public string UserID { get; set; }
        public string UnitCode { get; set; }

		public s_User_Units()
		{
		}

		public s_User_Units(string userID, string unitCode)
		{
			UnitCode = unitCode;
			UserID = userID;
		}
	}
}
