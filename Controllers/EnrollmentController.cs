using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using STDMS.Data;
using STDMS.Models;
using STDMS.Services.Interfaces;

namespace STDMS.Controllers
{
    [Authorize(Roles = RoleNames.SchoolRecords)]
    [Route("Enrollment")]
    public class EnrollmentController : Controller
    {
        private const int PageSize = 15;
        private readonly IEnrollmentService _service;
        private readonly IStudentService _studentService;
        private readonly ICourseService _courseService;
        private readonly ApplicationDbContext _context;

        public EnrollmentController(
            IEnrollmentService service,
            IStudentService studentService,
            ICourseService courseService,
            ApplicationDbContext context)
        {
            _service = service;
            _studentService = studentService;
            _courseService = courseService;
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string? search, int? courseId, int page = 1)
        {
            page = Math.Max(page, 1);
            var query = _context.Enrollments
                .AsNoTracking()
                .Include(enrollment => enrollment.Student)
                .Include(enrollment => enrollment.Course)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(enrollment =>
                    enrollment.Student.Name.Contains(term)
                    || enrollment.Course.Name.Contains(term)
                    || enrollment.Grade.Contains(term));
            }

            if (courseId.HasValue)
            {
                query = query.Where(enrollment => enrollment.CourseId == courseId.Value);
            }

            var totalEnrollments = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalEnrollments / (double)PageSize);
            page = Math.Min(page, Math.Max(totalPages, 1));
            var model = new EnrollmentDirectoryViewModel
            {
                Search = search,
                CourseId = courseId,
                Page = page,
                TotalPages = totalPages,
                TotalEnrollments = totalEnrollments,
                Enrollments = await query
                    .OrderBy(enrollment => enrollment.Course.Name)
                    .ThenBy(enrollment => enrollment.Student.Name)
                    .ThenBy(enrollment => enrollment.Id)
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync(),
                Courses = await _context.Courses
                    .AsNoTracking()
                    .OrderBy(course => course.Name)
                    .Select(course => new SelectListItem
                    {
                        Text = course.Name,
                        Value = course.Id.ToString(),
                        Selected = courseId == course.Id
                    })
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var enrollment = await _service.GetByIdAsync(id);
            return enrollment is null ? NotFound() : View(enrollment);
        }

        [HttpGet("Create")]
        [Authorize(Roles = RoleNames.Administrators)]
        public async Task<IActionResult> Create()
        {
            await PopulateViewBagsAsync();
            return View();
        }

        [HttpPost("Create")]
        [Authorize(Roles = RoleNames.Administrators)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Enrollment enrollment)
        {
            if (!ModelState.IsValid)
            {
                await PopulateViewBagsAsync(enrollment.StudentId, enrollment.CourseId);
                return View(enrollment);
            }

            await _service.AddAsync(enrollment);
            TempData["SuccessMessage"] = "Enrollment created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Edit/{id}")]
        [Authorize(Roles = RoleNames.Administrators)]
        public async Task<IActionResult> Edit(int id)
        {
            var enrollment = await _service.GetByIdAsync(id);
            if (enrollment is null) return NotFound();

            await PopulateViewBagsAsync(enrollment.StudentId, enrollment.CourseId);
            return View(enrollment);
        }

        [HttpPost("Edit")]
        [Authorize(Roles = RoleNames.Administrators)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Enrollment enrollment)
        {
            if (!ModelState.IsValid)
            {
                await PopulateViewBagsAsync(enrollment.StudentId, enrollment.CourseId);
                return View(enrollment);
            }

            await _service.UpdateAsync(enrollment);
            TempData["SuccessMessage"] = "Enrollment updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Delete/{id}")]
        [Authorize(Roles = RoleNames.Administrators)]
        public async Task<IActionResult> Delete(int id)
        {
            var enrollment = await _service.GetByIdAsync(id);
            return enrollment is null ? NotFound() : View(enrollment);
        }

        [HttpPost("Delete/{id}")]
        [Authorize(Roles = RoleNames.Administrators)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            TempData["SuccessMessage"] = "Enrollment deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateViewBagsAsync(int? studentId = null, int? courseId = null)
        {
            // The Create/Edit views read ViewBag.Students / ViewBag.Courses.
            // These used to be set as StudentsList/CoursesList, so both
            // dropdowns rendered empty and no enrollment could be saved.
            ViewBag.Students = new SelectList(await _studentService.GetAllAsync(), "Id", "Name", studentId);
            ViewBag.Courses = new SelectList(await _courseService.GetAllAsync(), "Id", "Name", courseId);
        }
    }
}
