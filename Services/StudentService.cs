// Services/StudentService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STDMS.Data;
using STDMS.Models;
using STDMS.Services.Interfaces;

namespace STDMS.Services
{
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StudentService> _logger;

        public StudentService(ApplicationDbContext context, ILogger<StudentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Student>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all students...");
            return await _context.Students
                .Include(s => s.Department)
                .ToListAsync();
        }

        public async Task<Student?> GetByIdAsync(int id)
        {
            _logger.LogInformation($"Fetching student with ID = {id}");
            return await _context.Students
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AddAsync(Student student)
        {
            _logger.LogInformation("Adding a new student...");
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Student student)
        {
            _logger.LogInformation($"Updating student with ID = {student.Id}");
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation($"Deleting student with ID = {id}");
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
            else
            {
                _logger.LogWarning($"Student with ID = {id} not found");
            }
        }
    }
}
