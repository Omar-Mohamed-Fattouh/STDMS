using Microsoft.AspNetCore.Mvc.Rendering;

namespace STDMS.Models
{
    public class AttendanceIndexViewModel
    {
        public int? CourseId { get; set; }
        public DateTime? Date { get; set; }
        public AttendancePageResult Result { get; set; } = new();
        public SelectList Courses { get; set; } = new(Array.Empty<SelectListItem>());
    }
}
