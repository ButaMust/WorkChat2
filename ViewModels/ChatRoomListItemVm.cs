namespace WorkChat2.ViewModels
{
    public class ChatRoomListItemVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsGroup { get; set; }
        public DateTime LastUpdatedUtc { get; set; }
    }
}
