namespace WorkChat2.Models
{
    public class Message : BaseEntity
    {
        public int Id { get; set; }

        public int ChatRoomId { get; set; }
        public ChatRoom ChatRoom { get; set; } = null!;

        public string SenderId { get; set; } = null!;
        public AppUser Sender { get; set; } = null!;

        public string Text { get; set; } = null!;

        public bool IsEdited { get; set; }
        public bool IsDeleted { get; set; }
    }
}
