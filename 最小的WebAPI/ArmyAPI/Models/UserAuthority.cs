namespace ArmyAPI.Models
{
    public class UserAuthority
	{
        /// <summary>
        /// 使用者 Id
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// Insert
        /// </summary>
        public bool? I { get; set; }
        /// <summary>
        /// Update
        /// </summary>
        public bool? U { get; set; }
        /// <summary>
        /// Select
        /// </summary>
        public bool? S { get; set; }
        /// <summary>
        /// Delete
        /// </summary>
        public bool? D { get; set; }
    }
}
