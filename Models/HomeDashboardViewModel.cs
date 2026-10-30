using System.Collections.Generic;

namespace STDMS.Models
{
    public class HomeDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalCourses { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalEnrollments { get; set; }
        public int AttendanceToday { get; set; }
        public int PresentToday { get; set; }
        public List<Student> RecentStudents { get; set; } = new();
        public List<Enrollment> RecentEnrollments { get; set; } = new();
    }
}
