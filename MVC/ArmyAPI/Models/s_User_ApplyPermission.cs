namespace ArmyAPI.Models
{
	public class s_User_ApplyPermission
	{
        public string UserID { get; set; }
        public string PermissionStr { get; set; }

		public s_User_ApplyPermission()
		{
		}

		public s_User_ApplyPermission(string userID, string permissionStr)
		{
			UserID = userID;
			PermissionStr = permissionStr;
		}
	}
}
