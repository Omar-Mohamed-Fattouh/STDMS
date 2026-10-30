// Controllers/AttendanceController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using STDMS.Data;
using Microsoft.Extensions.Logging;
using STDMS.Models;
using STDMS.Services.Interfaces;

namespace STDMS.Controllers
{
    [Authorize(Roles = RoleNames.AttendanceUsers)]
    [Route("Attendance")]
    public class AttendanceController : Controller
    {
        private const int PageSize = 25;
        private readonly IAttendanceService _service;
        private readonly IStudentService _studentService;
        private readonly ICourseService _courseService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IInstructorCourseAssignmentService _assignmentService;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(
            IAttendanceService service,
            IStudentService studentService,
            ICourseService courseService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IInstructorCourseAssignmentService assignmentService,
            ILogger<AttendanceController> logger)
        {
            _service = service;
            _studentService = studentService;
            _courseService = courseService;
            _context = context;
            _userManager = userManager;
            _assignmentService = assignmentService;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int? courseId, DateTime? date, int page = 1)
        {
            IReadOnlyCollection<int>? allowedCourseIds = null;
            if (User.IsInRole(RoleNames.Instructor))
            {
                allowedCourseIds = await GetAssignedCourseIdsAsync();
            }

            var courseOptions = await _courseService.GetAllAsync();
            if (allowedCourseIds is not null)
            {
                courseOptions = courseOptions
                    .Where(course => allowedCourseIds.Contains(course.Id))
                    .ToList();
            }

            var result = await _service.GetFilteredPageAsync(
                courseId,
                date,
                allowedCourseIds,
                page,
                PageSize);
            var model = new AttendanceIndexViewModel
            {
                CourseId = courseId,
                Date = date?.Date,
                Result = result,
                Courses = new SelectList(courseOptions, "Id", "Name", courseId)
            };
            return View(model);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var attendance = await _service.GetByIdAsync(id);
            if (attendance == null)
            {
                _logger.LogWarning("Attendance not found with ID = {id}", id);
                return NotFound();
            }
            if (!await CanAccessAttendanceAsync(attendance))
            {
                return NotFound();
            }
            return View(attendance);
        }

        [HttpGet("Create")]
        [Authorize(Roles = RoleNames.AttendanceWriters)]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();
            return View(new Attendance { Date = DateTime.Today });
        }

        [HttpPost("Create")]
        [Authorize(Roles = RoleNames.AttendanceWriters)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Attendance attendance)
        {
            if (!await CanUseCourseAsync(attendance.CourseId)
                || !await IsStudentEnrolledAsync(attendance.StudentId, attendance.CourseId))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                await _service.AddAsync(attendance);
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownsAsync(attendance.StudentId, attendance.CourseId);
            return View(attendance);
        }

        [HttpGet("Edit/{id}")]
        [Authorize(Roles = RoleNames.AttendanceWriters)]
        public async Task<IActionResult> Edit(int id)
        {
            var attendance = await _service.GetByIdAsync(id);
            if (attendance == null) return NotFound();
            if (!await CanAccessAttendanceAsync(attendance)) return NotFound();

            await LoadDropdownsAsync(attendance.StudentId, attendance.CourseId);
            return View(attendance);
        }

        [HttpPost("Edit")]
        [Authorize(Roles = RoleNames.AttendanceWriters)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Attendance attendance)
        {
            var existingAttendance = await _service.GetByIdAsync(attendance.Id);
            if (existingAttendance == null) return NotFound();
            if (!await CanAccessAttendanceAsync(existingAttendance)
                || !await CanUseCourseAsync(attendance.CourseId)
                || !await IsStudentEnrolledAsync(attendance.StudentId, attendance.CourseId))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                await _service.UpdateAsync(attendance);
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownsAsync(attendance.StudentId, attendance.CourseId);
            return View(attendance);
        }

        [HttpGet("Delete/{id}")]
        [Authorize(Roles = RoleNames.AttendanceWriters)]
        public async Task<IActionResult> Delete(int id)
        {
            var attendance = await _service.GetByIdAsync(id);
            if (attendance == null) return NotFound();
            if (!await CanAccessAttendanceAsync(attendance)) return NotFound();
            return View(attendance);
        }

        [HttpPost("Delete/{id}")]
        [Authorize(Roles = RoleNames.AttendanceWriters)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var attendance = await _service.GetByIdAsync(id);
            if (attendance == null) return NotFound();
            if (!await CanAccessAttendanceAsync(attendance)) return NotFound();

            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdownsAsync(int? selectedStudentId = null, int? selectedCourseId = null)
        {
            var assignedCourseIds = User.IsInRole(RoleNames.Instructor)
                ? await GetAssignedCourseIdsAsync()
                : null;
            var courses = await _courseService.GetAllAsync();
            if (assignedCourseIds != null)
            {
                courses = courses.Where(course => assignedCourseIds.Contains(course.Id)).ToList();
            }

            var studentsQuery = _context.Students.AsNoTracking();
            if (assignedCourseIds != null)
            {
                var allowedCourseIds = selectedCourseId.HasValue
                    ? assignedCourseIds.Where(id => id == selectedCourseId.Value).ToArray()
                    : assignedCourseIds.ToArray();

                studentsQuery = studentsQuery.Where(student => _context.Enrollments.Any(enrollment =>
                    enrollment.StudentId == student.Id && allowedCourseIds.Contains(enrollment.CourseId)));
            }

            var students = assignedCourseIds == null
                ? await _studentService.GetAllAsync()
                : await studentsQuery.ToListAsync();

            ViewBag.StudentsList = new SelectList(students, "Id", "Name", selectedStudentId);
            ViewBag.CoursesList = new SelectList(courses, "Id", "Name", selectedCourseId);
        }

        private async Task<int[]> GetAssignedCourseIdsAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.InstructorId is not int instructorId)
            {
                return [];
            }

            return (await _assignmentService.GetCourseIdsForInstructorAsync(instructorId)).ToArray();
        }

        private async Task<bool> CanAccessAttendanceAsync(Attendance attendance)
        {
            if (!User.IsInRole(RoleNames.Instructor))
            {
                return true;
            }

            return await CanUseCourseAsync(attendance.CourseId);
        }

        private async Task<bool> CanUseCourseAsync(int courseId)
        {
            if (!User.IsInRole(RoleNames.Instructor))
            {
                return true;
            }

            var user = await _userManager.GetUserAsync(User);
            return user?.InstructorId is int instructorId
                && await _assignmentService.IsAssignedAsync(instructorId, courseId);
        }

        private Task<bool> IsStudentEnrolledAsync(int studentId, int courseId)
        {
            return _context.Enrollments.AnyAsync(enrollment =>
                enrollment.StudentId == studentId && enrollment.CourseId == courseId);
        }
    }
}
