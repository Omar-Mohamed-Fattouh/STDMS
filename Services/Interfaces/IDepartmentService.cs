// Services/Interfaces/IDepartmentService.cs
using STDMS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace STDMS.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<Department>> GetAllAsync();
        Task<Department?> GetByIdAsync(int id);
        Task AddAsync(Department department);    // Changed from Task<bool>
        Task UpdateAsync(Department department);  // Changed from Task<bool>
        Task DeleteAsync(int id);                 // Changed from Task<bool>
    }
}
