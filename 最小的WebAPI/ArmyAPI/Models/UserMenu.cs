namespace ArmyAPI.Models
{
    public class UserMenu
    {

        public int Index { get; set; }
        public string? Title { get; set; }
        public int ParentIndex { get; set; }
        public DateTime CreateDatetime { get; set; }
        public int Level { get; set; }
        public bool C { get; set; }
        public bool U { get; set; }
        public bool D { get; set; }
        public bool R { get; set; }
    }
}
