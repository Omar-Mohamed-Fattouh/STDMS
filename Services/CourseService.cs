using Microsoft.EntityFrameworkCore;
using STDMS.Data;
using STDMS.Models;
using STDMS.Services.Interfaces;

namespace STDMS.Services
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;

        public CourseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Course>> GetAllAsync()
        {
            return await _context.Courses.ToListAsync();
        }

        public async Task<Course?> GetByIdAsync(int id)
        {
            return await _context.Courses.FindAsync(id);
        }

        public async Task AddAsync(Course course)
        {
            ArgumentNullException.ThrowIfNull(course);

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Course course)
        {
            if (!await _context.Courses.AnyAsync(c => c.Id == course.Id))
                throw new KeyNotFoundException("Course not found.");

            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course is null)
                throw new KeyNotFoundException("Course not found.");

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
        }
    }
}
