// Services/AttendanceService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STDMS.Data;
using STDMS.Models;
using STDMS.Services.Interfaces;

namespace STDMS.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AttendanceService> _logger;

        public AttendanceService(ApplicationDbContext context, ILogger<AttendanceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Attendance>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all attendances.");
            return await _context.Attendances
                                 .AsNoTracking()
                                 .Include(a => a.Student)
                                 .Include(a => a.Course)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetForCoursesAsync(IReadOnlyCollection<int> courseIds)
        {
            return await _context.Attendances
                .AsNoTracking()
                .Where(attendance => courseIds.Contains(attendance.CourseId))
                .Include(attendance => attendance.Student)
                .Include(attendance => attendance.Course)
                .ToListAsync();
        }

        public async Task<AttendancePageResult> GetFilteredPageAsync(
            int? courseId,
            DateTime? date,
            IReadOnlyCollection<int>? allowedCourseIds,
            int page,
            int pageSize)
        {
            var query = _context.Attendances
                .AsNoTracking()
                .AsQueryable();

            if (allowedCourseIds is not null)
            {
                query = query.Where(attendance => allowedCourseIds.Contains(attendance.CourseId));
            }

            if (courseId.HasValue)
            {
                query = query.Where(attendance => attendance.CourseId == courseId.Value);
            }

            if (date.HasValue)
            {
                var start = date.Value.Date;
                var end = start.AddDays(1);
                query = query.Where(attendance => attendance.Date >= start && attendance.Date < end);
            }

            var totalRecords = await query.CountAsync();
            var presentRecords = await query.CountAsync(attendance => attendance.IsPresent);
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            page = Math.Min(Math.Max(page, 1), Math.Max(totalPages, 1));

            var records = await query
                .Include(attendance => attendance.Student)
                .Include(attendance => attendance.Course)
                .OrderByDescending(attendance => attendance.Date)
                .ThenBy(attendance => attendance.Course.Name)
                .ThenBy(attendance => attendance.Student.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new AttendancePageResult
            {
                Records = records,
                TotalRecords = totalRecords,
                PresentRecords = presentRecords,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<Attendance?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching attendance with ID = {id}", id);
            // AsNoTracking matters here beyond speed: the Edit POST fetches the
            // existing record for its access check and then Update()s the bound
            // model — a tracked fetch would make EF refuse the second instance
            // with the same key ("another instance is already being tracked").
            return await _context.Attendances
                                 .AsNoTracking()
                                 .Include(a => a.Student)
                                 .Include(a => a.Course)
                                 .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAsync(Attendance attendance)
        {
            _logger.LogInformation("Adding new attendance for StudentId = {studentId}, CourseId = {courseId}", attendance.StudentId, attendance.CourseId);
            await _context.Attendances.AddAsync(attendance);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Attendance attendance)
        {
            _logger.LogInformation("Updating attendance ID = {id}", attendance.Id);
            _context.Attendances.Update(attendance);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting attendance ID = {id}", id);
            var entity = await _context.Attendances.FindAsync(id);
            if (entity != null)
            {
                _context.Attendances.Remove(entity);
                await _context.SaveChangesAsync();
            }
            else
            {
                _logger.LogWarning("Attendance not found with ID = {id}", id);
            }
        }
    }
}
