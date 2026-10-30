// Services/Interfaces/IAttendanceService.cs
using STDMS.Models;

namespace STDMS.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<IEnumerable<Attendance>> GetAllAsync();
        Task<IEnumerable<Attendance>> GetForCoursesAsync(IReadOnlyCollection<int> courseIds);
        Task<AttendancePageResult> GetFilteredPageAsync(
            int? courseId,
            DateTime? date,
            IReadOnlyCollection<int>? allowedCourseIds,
            int page,
            int pageSize);
        Task<Attendance?> GetByIdAsync(int id);
        Task AddAsync(Attendance attendance);
        Task UpdateAsync(Attendance attendance);
        Task DeleteAsync(int id);
    }
}
