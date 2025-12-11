namespace WorkChat2.Models
{
    public class ChatRoomParticipant : BaseEntity
    {
        public int ChatRoomId { get; set; }
        public ChatRoom ChatRoom { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public AppUser User { get; set; } = null!;

        public bool IsAdmin { get; set; }
    }
}
