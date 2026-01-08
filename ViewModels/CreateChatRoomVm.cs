using System.ComponentModel.DataAnnotations;

namespace WorkChat2.ViewModels
{
    public class CreateChatRoomVm
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = "";
        public bool IsGroup { get; set; } = true;

        [Required]
        public List<string> SelectedUserIds { get; set; } = new();

        public List<UserSelectVm> Users { get; set; } = new();
    }
}
