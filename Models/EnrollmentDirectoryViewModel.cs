using Microsoft.AspNetCore.Mvc.Rendering;

namespace STDMS.Models
{
    public class EnrollmentDirectoryViewModel
    {
        public string? Search { get; set; }
        public int? CourseId { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public int TotalEnrollments { get; set; }
        public int FirstVisiblePage => Math.Max(1, Page - 2);
        public int LastVisiblePage => Math.Min(TotalPages, Page + 2);
        public IReadOnlyList<Enrollment> Enrollments { get; set; } = [];
        public IReadOnlyList<SelectListItem> Courses { get; set; } = [];
    }
}
