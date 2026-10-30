using Microsoft.EntityFrameworkCore;
using STDMS.Data;
using STDMS.Models;
using STDMS.Services.Interfaces;

namespace STDMS.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Enrollment>> GetAllAsync()
        {
            return await _context.Enrollments
                                 .Include(e => e.Student)
                                 .Include(e => e.Course)
                                 .ToListAsync();
        }

        public async Task<Enrollment?> GetByIdAsync(int id)
        {
            return await _context.Enrollments
                                 .Include(e => e.Student)
                                 .Include(e => e.Course)
                                 .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task AddAsync(Enrollment enrollment)
        {
            ArgumentNullException.ThrowIfNull(enrollment);

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Enrollment enrollment)
        {
            if (!await _context.Enrollments.AnyAsync(e => e.Id == enrollment.Id))
                throw new KeyNotFoundException("Enrollment not found.");

            _context.Enrollments.Update(enrollment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Enrollments.FindAsync(id);
            if (entity is null)
                throw new KeyNotFoundException("Enrollment not found.");

            _context.Enrollments.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
