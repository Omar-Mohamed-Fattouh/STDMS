using System.ComponentModel.DataAnnotations;

namespace STDMS.Models
{
    public class InstructorCourseAssignment
    {
        [Range(1, int.MaxValue, ErrorMessage = "Select an instructor.")]
        public int InstructorId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Select a course.")]
        public int CourseId { get; set; }

        public Instructor Instructor { get; set; } = null!;
        public Course Course { get; set; } = null!;
    }
}
