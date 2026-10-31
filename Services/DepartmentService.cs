// Services/DepartmentService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STDMS.Data;
using STDMS.Models;
using STDMS.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace STDMS.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DepartmentService> _logger;

        public DepartmentService(ApplicationDbContext context, ILogger<DepartmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all departments.");
            return await _context.Departments.AsNoTracking().ToListAsync();
        }

        public async Task<Department?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching department with ID = {DepartmentId}", id);
            return await _context.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task AddAsync(Department department)
        {
            if (department == null)
            {
                throw new ArgumentNullException(nameof(department));
            }

            try
            {
                _logger.LogInformation("Adding new department: {DepartmentName}", department.Name);
                await _context.Departments.AddAsync(department);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding department {DepartmentName}", department.Name);
                throw; // Re-throw the exception to be handled by the controller
            }
        }

        public async Task UpdateAsync(Department department)
        {
            if (department == null)
            {
                throw new ArgumentNullException(nameof(department));
            }

            // First, check if the entity exists
            var existingDept = await _context.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == department.Id);
            if (existingDept == null)
            {
                throw new KeyNotFoundException($"Department with ID {department.Id} not found.");
            }

            try
            {
                _logger.LogInformation("Updating department with ID = {DepartmentId}", department.Id);
                _context.Departments.Update(department);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating department {DepartmentId}", department.Id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Attempting to delete department with ID = {DepartmentId}", id);
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                // Throw an exception if not found, so the controller can return NotFound()
                throw new KeyNotFoundException($"Department with ID {id} not found.");
            }

            try
            {
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully deleted department with ID = {DepartmentId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting department {DepartmentId}", id);
                throw;
            }
        }
    }
}
