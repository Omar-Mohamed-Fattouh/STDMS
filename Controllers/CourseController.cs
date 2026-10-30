using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STDMS.Models;
using STDMS.Services.Interfaces;
using STDMS.Filters;

namespace STDMS.Controllers
{
    [Authorize(Roles = RoleNames.SchoolRecords)]
    [Route("Course")]
    [ServiceFilter(typeof(ActionLoggingFilter))]
    public class CourseController : Controller
    {
        private readonly ICourseService _service;

        public CourseController(ICourseService service)
        {
            _service = service;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var courses = await _service.GetAllAsync();
            return View(courses);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var course = await _service.GetByIdAsync(id);
            return course is null ? NotFound() : View(course);
        }

        [HttpGet("Create")]
        [Authorize(Roles = RoleNames.Administrators)]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("Create")]
        [Authorize(Roles = RoleNames.Administrators)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course)
        {
            if (!ModelState.IsValid)
                return View(course);

            try
            {
                await _service.AddAsync(course);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(course);
            }
        }

        [HttpGet("Edit/{id}")]
        [Authorize(Roles = RoleNames.Administrators)]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _service.GetByIdAsync(id);
            return course is null ? NotFound() : View(course);
        }

        [HttpPost("Edit")]
        [Authorize(Roles = RoleNames.Administrators)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Course course)
        {
            if (!ModelState.IsValid)
                return View(course);

            try
            {
                await _service.UpdateAsync(course);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(course);
            }
        }

        [HttpGet("Delete/{id}")]
        [Authorize(Roles = RoleNames.Administrators)]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _service.GetByIdAsync(id);
            return course is null ? NotFound() : View(course);
        }

        [HttpPost("Delete/{id}")]
        [Authorize(Roles = RoleNames.Administrators)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
