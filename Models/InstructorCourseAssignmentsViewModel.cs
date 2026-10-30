using Microsoft.AspNetCore.Mvc.Rendering;

namespace STDMS.Models
{
    public class InstructorCourseAssignmentsViewModel
    {
        public int InstructorId { get; set; }
        public int CourseId { get; set; }
        public IReadOnlyList<InstructorCourseAssignment> Assignments { get; set; } = [];
        public IReadOnlyList<SelectListItem> Instructors { get; set; } = [];
        public IReadOnlyList<SelectListItem> Courses { get; set; } = [];
    }
}
