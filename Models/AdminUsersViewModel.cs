namespace STDMS.Models
{
    public class AdminUsersViewModel
    {
        public IReadOnlyList<AdminUserRowViewModel> Users { get; set; } = [];
    }

    public class AdminUserRowViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? InstructorName { get; set; }
    }
}
