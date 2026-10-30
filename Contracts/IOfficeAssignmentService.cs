using STDMS.Models;

public interface IOfficeAssignmentService
{
    Task<IEnumerable<OfficeAssignment>> GetAllAsync();
    Task<OfficeAssignment?> GetByIdAsync(int id);
    Task AddAsync(OfficeAssignment officeAssignment);
    Task UpdateAsync(OfficeAssignment officeAssignment);
    Task DeleteAsync(int id);
}
