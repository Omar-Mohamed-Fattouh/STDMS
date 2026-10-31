using STDMS.Models;

namespace STDMS.Services.Interfaces
{
    public interface IInstructorCourseAssignmentService
    {
        Task<IReadOnlyList<InstructorCourseAssignment>> GetAllAsync();
        Task<IReadOnlyList<int>> GetCourseIdsForInstructorAsync(int instructorId);
        Task<bool> IsAssignedAsync(int instructorId, int courseId);
        Task AddAsync(InstructorCourseAssignment assignment);
        Task DeleteAsync(int instructorId, int courseId);
    }
}
