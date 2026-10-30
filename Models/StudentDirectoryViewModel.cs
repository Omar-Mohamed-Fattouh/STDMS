using Microsoft.AspNetCore.Mvc.Rendering;

namespace STDMS.Models
{
    public class StudentDirectoryViewModel
    {
        public string? Search { get; set; }
        public int? DepartmentId { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public int TotalStudents { get; set; }
        public int FirstVisiblePage => Math.Max(1, Page - 2);
        public int LastVisiblePage => Math.Min(TotalPages, Page + 2);
        public IReadOnlyList<Student> Students { get; set; } = [];
        public IReadOnlyList<SelectListItem> Departments { get; set; } = [];
    }
}
