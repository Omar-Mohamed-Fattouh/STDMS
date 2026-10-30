using Microsoft.EntityFrameworkCore;
using STDMS.Data;
using STDMS.Models;
using STDMS.Services.Interfaces;

namespace STDMS.Services
{
    public class InstructorCourseAssignmentService : IInstructorCourseAssignmentService
    {
        private readonly ApplicationDbContext _context;

        public InstructorCourseAssignmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<InstructorCourseAssignment>> GetAllAsync()
        {
            return await _context.InstructorCourseAssignments
                .AsNoTracking()
                .Include(assignment => assignment.Instructor)
                .Include(assignment => assignment.Course)
                .OrderBy(assignment => assignment.Instructor.Name)
                .ThenBy(assignment => assignment.Course.Name)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<int>> GetCourseIdsForInstructorAsync(int instructorId)
        {
            return await _context.InstructorCourseAssignments
                .AsNoTracking()
                .Where(assignment => assignment.InstructorId == instructorId)
                .Select(assignment => assignment.CourseId)
                .ToListAsync();
        }

        public Task<bool> IsAssignedAsync(int instructorId, int courseId)
        {
            return _context.InstructorCourseAssignments
                .AsNoTracking()
                .AnyAsync(
                assignment => assignment.InstructorId == instructorId && assignment.CourseId == courseId);
        }

        public async Task AddAsync(InstructorCourseAssignment assignment)
        {
            _context.InstructorCourseAssignments.Add(assignment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int instructorId, int courseId)
        {
            var assignment = await _context.InstructorCourseAssignments.FindAsync(instructorId, courseId);
            if (assignment is null)
            {
                throw new KeyNotFoundException("Instructor course assignment not found.");
            }

            _context.InstructorCourseAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
        }
    }
}
