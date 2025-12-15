namespace WorkChat2.ViewModels
{
    public class AdminUsersPageVm
    {
        public List<AdminUserListItemVm> Users { get; set; } = new();

        public string? Q {  get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((Double)TotalCount / PageSize);

        public bool HasPrev => Page > 1;
        public bool HasNext => Page < TotalPages;
    }
}
