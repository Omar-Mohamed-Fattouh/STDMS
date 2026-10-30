using Microsoft.EntityFrameworkCore;
using STDMS.Data;
using STDMS.Models;
using STDMS.Services.Interfaces;

namespace STDMS.Services
{
    public class OfficeAssignmentService : IOfficeAssignmentService
    {
        private readonly ApplicationDbContext _context;

        public OfficeAssignmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OfficeAssignment>> GetAllAsync()
        {
            return await _context.OfficeAssignments
                                 .Include(o => o.Instructor)
                                 .ToListAsync();
        }

        public async Task<OfficeAssignment?> GetByIdAsync(int instructorId)
        {
            return await _context.OfficeAssignments
                                 .Include(o => o.Instructor)
                                 .FirstOrDefaultAsync(o => o.InstructorId == instructorId);
        }

        public async Task AddAsync(OfficeAssignment officeAssignment)
        {
            if (officeAssignment == null)
                throw new ArgumentNullException(nameof(officeAssignment));

            _context.OfficeAssignments.Add(officeAssignment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(OfficeAssignment officeAssignment)
        {
            if (!await _context.OfficeAssignments.AnyAsync(o => o.InstructorId == officeAssignment.InstructorId))
                throw new KeyNotFoundException("Office Assignment not found.");

            _context.OfficeAssignments.Update(officeAssignment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int instructorId)
        {
            var office = await _context.OfficeAssignments.FindAsync(instructorId);
            if (office == null)
                throw new KeyNotFoundException("Office Assignment not found.");

            _context.OfficeAssignments.Remove(office);
            await _context.SaveChangesAsync();
        }
    }
}
