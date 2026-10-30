// Controllers/StudentController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using STDMS.Models;
using STDMS.Services.Interfaces;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using STDMS.Data;

namespace STDMS.Controllers
{
    [Authorize(Roles = RoleNames.SchoolRecords)]
    [Route("Student")]
    public class StudentController : Controller
    {
        private const int PageSize = 10;
        private readonly IStudentService _studentService;
        private readonly ILogger<StudentController> _logger;
        private readonly ApplicationDbContext _context;

        public StudentController(IStudentService studentService, ILogger<StudentController> logger, ApplicationDbContext context)
        {
            _studentService = studentService;
            _logger = logger;
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string? search, int? departmentId, int page = 1)
        {
            page = Math.Max(page, 1);
            var studentsQuery = _context.Students
                .AsNoTracking()
                .Include(student => student.Department)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.Trim();
                studentsQuery = studentsQuery.Where(student =>
                    student.Name.Contains(searchTerm) || student.Email.Contains(searchTerm));
            }

            if (departmentId.HasValue)
            {
                studentsQuery = studentsQuery.Where(student => student.DepartmentId == departmentId.Value);
            }

            var totalStudents = await studentsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalStudents / (double)PageSize);
            page = Math.Min(page, Math.Max(totalPages, 1));
            var model = new StudentDirectoryViewModel
            {
                Search = search,
                DepartmentId = departmentId,
                Page = page,
                TotalPages = totalPages,
                TotalStudents = totalStudents,
                Students = await studentsQuery
                    .OrderBy(student => student.Name)
                    .ThenBy(student => student.Id)
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync(),
                Departments = await _context.Departments
                    .AsNoTracking()
                    .OrderBy(department => department.Name)
                    .Select(department => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = department.Name,
                        Value = department.Id.ToString(),
                        Selected = departmentId == department.Id
                    })
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var student = await _studentService.GetByIdAsync(id);
            if (student == null)
            {
                _logger.LogWarning($"Student with ID = {id} not found");
                return NotFound();
            }

            return View(student);
        }

        [HttpGet("Create")]
        [Authorize(Roles = RoleNames.Administrators)]
        public async Task<IActionResult> Create()
        {
            await PopulateDepartmentsAsync();
            return View();
        }

        [HttpPost("Create")]
        [Authorize(Roles = RoleNames.Administrators)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDepartmentsAsync(student.DepartmentId);
                return View(student);
            }

            await _studentService.AddAsync(student);
            TempData["SuccessMessage"] = "Student created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Edit/{id}")]
        [Authorize(Roles = RoleNames.Administrators)]
        public async Task<IActionResult> Edit(int id)
        {
            var student = await _studentService.GetByIdAsync(id);
            if (student == null)
            {
                _logger.LogWarning($"Student with ID = {id} not found");
                return NotFound();
            }

            await PopulateDepartmentsAsync(student.DepartmentId);
            return View(student);
        }

        [HttpPost("Edit")]
        [Authorize(Roles = RoleNames.Administrators)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Student student)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDepartmentsAsync(student.DepartmentId);
                return View(student);
            }

            await _studentService.UpdateAsync(student);
            TempData["SuccessMessage"] = "Student updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Delete/{id}")]
        [Authorize(Roles = RoleNames.Administrators)]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _studentService.GetByIdAsync(id);
            if (student == null)
            {
                _logger.LogWarning($"Student with ID = {id} not found");
                return NotFound();
            }

            return View(student);
        }

        [HttpPost("Delete/{id}")]
        [Authorize(Roles = RoleNames.Administrators)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _studentService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Student deleted.";
            return RedirectToAction(nameof(Index));
        }

        // The Create/Edit views bind this directly with asp-items, so it must be
        // a SelectList (IEnumerable<SelectListItem>), not a List<Department>:
        // passing the entity list caused a RuntimeBinderException (HTTP 500)
        // on GET /Student/Create.
        private async Task PopulateDepartmentsAsync(int? selectedDepartmentId = null)
        {
            var departments = await _context.Departments
                .AsNoTracking()
                .OrderBy(department => department.Name)
                .ToListAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name", selectedDepartmentId);
        }
    }
}
