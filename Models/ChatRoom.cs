namespace WorkChat2.Models
{
    public class ChatRoom : BaseEntity
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public bool IsGroup { get; set; }

        public string CreatedByUserId { get; set; } = null!;
        public AppUser CreatedByUser { get; set; } = null!;

        public ICollection<ChatRoomParticipant> Participants { get; set; } = new List<ChatRoomParticipant>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
