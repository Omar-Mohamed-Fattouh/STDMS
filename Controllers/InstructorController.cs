// Controllers/InstructorController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using STDMS.Filters;
using STDMS.Models;
using STDMS.Services.Interfaces;
using System.Threading.Tasks;

namespace STDMS.Controllers
{
    //[TypeFilter(typeof(CustomAuthorizationFilter), Arguments = new object[] { "Admin", "Instructor" })]
    [Authorize(Roles = RoleNames.SchoolRecords)]
    [Route("Instructor")]
    public class InstructorController : Controller
    {
        private readonly IInstructorService _service;
        private readonly ILogger<InstructorController> _logger;

        public InstructorController(IInstructorService service, ILogger<InstructorController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var instructors = await _service.GetAllAsync();
            return View(instructors);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var instructor = await _service.GetByIdAsync(id);
            if (instructor == null)
            {
                _logger.LogWarning("Instructor not found with ID = {id}", id);
                return NotFound();
            }
            return View(instructor);
        }

        [HttpGet("Create")]
        [Authorize(Roles = RoleNames.Administrators)]
        public IActionResult Create() => View();

        [HttpPost("Create")]
        [Authorize(Roles = RoleNames.Administrators)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Instructor instructor)
        {
            if (ModelState.IsValid)
            {
                await _service.AddAsync(instructor);
                return RedirectToAction(nameof(Index));
            }
            return View(instructor);
        }

        [HttpGet("Edit/{id}")]
        [Authorize(Roles = RoleNames.Administrators)]
        public async Task<IActionResult> Edit(int id)
        {
            var instructor = await _service.GetByIdAsync(id);
            if (instructor == null) return NotFound();
            return View(instructor);
        }

        [HttpPost("Edit")]
        [Authorize(Roles = RoleNames.Administrators)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Instructor instructor)
        {
            if (ModelState.IsValid)
            {
                await _service.UpdateAsync(instructor);
                return RedirectToAction(nameof(Index));
            }
            return View(instructor);
        }

        [HttpGet("Delete/{id}")]
        [Authorize(Roles = RoleNames.Administrators)]
        public async Task<IActionResult> Delete(int id)
        {
            var instructor = await _service.GetByIdAsync(id);
            if (instructor == null) return NotFound();
            return View(instructor);
        }

        [HttpPost("Delete/{id}")]
        [Authorize(Roles = RoleNames.Administrators)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
