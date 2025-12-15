namespace WorkChat2.ViewModels
{
    public class AdminUserListItemVm
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Name { get; set; } = "";
        public string LastName { get; set; } = "";

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<string> Roles { get; set; } = new();
        public bool IsAdmin => Roles.Contains("Admin");
    }
}
