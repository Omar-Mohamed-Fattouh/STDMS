using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using STDMS.Models;
using STDMS.Services.Interfaces;

namespace STDMS.Controllers
{
    [Authorize(Roles = RoleNames.SchoolRecords)]
    [Route("OfficeAssignment")]
    public class OfficeAssignmentController : Controller
    {
        private readonly IOfficeAssignmentService _service;
        private readonly IInstructorService _instructorService;
        private readonly ILogger<OfficeAssignmentController> _logger;

        public OfficeAssignmentController(
            IOfficeAssignmentService service,
            IInstructorService instructorService,
            ILogger<OfficeAssignmentController> logger)
        {
            _service = service;
            _instructorService = instructorService;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var offices = await _service.GetAllAsync();
            return View(offices);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var office = await _service.GetByIdAsync(id);
            return office == null ? NotFound() : View(office);
        }

        [HttpGet("Create")]
        [Authorize(Roles = RoleNames.Administrators)]
        public async Task<IActionResult> Create()
        {
            await LoadInstructorsAsync();
            return View();
        }

        [HttpPost("Create")]
        [Authorize(Roles = RoleNames.Administrators)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OfficeAssignment office)
        {
            if (!ModelState.IsValid)
            {
                await LoadInstructorsAsync(office.InstructorId);
                return View(office);
            }

            try
            {
                await _service.AddAsync(office);
                TempData["SuccessMessage"] = "Office assignment created.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create an office assignment for instructor {InstructorId}", office.InstructorId);
                ModelState.AddModelError("", "The office assignment could not be saved. Check that the instructor does not already have an assignment.");
                await LoadInstructorsAsync(office.InstructorId);
                return View(office);
            }
        }

        [HttpGet("Edit/{id}")]
        [Authorize(Roles = RoleNames.Administrators)]
        public async Task<IActionResult> Edit(int id)
        {
            var office = await _service.GetByIdAsync(id);
            return office == null ? NotFound() : View(office);
        }

        [HttpPost("Edit")]
        [Authorize(Roles = RoleNames.Administrators)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OfficeAssignment office)
        {
            if (!ModelState.IsValid)
                return View(office);

            try
            {
                await _service.UpdateAsync(office);
                TempData["SuccessMessage"] = "Office assignment updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update the office assignment for instructor {InstructorId}", office.InstructorId);
                ModelState.AddModelError("", "The office assignment could not be updated. Please try again.");
                return View(office);
            }
        }

        [HttpGet("Delete/{id}")]
        [Authorize(Roles = RoleNames.Administrators)]
        public async Task<IActionResult> Delete(int id)
        {
            var office = await _service.GetByIdAsync(id);
            return office == null ? NotFound() : View(office);
        }

        [HttpPost("Delete/{id}")]
        [Authorize(Roles = RoleNames.Administrators)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                TempData["SuccessMessage"] = "Office assignment deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete the office assignment for instructor {InstructorId}", id);
                TempData["ErrorMessage"] = "The office assignment could not be deleted. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task LoadInstructorsAsync(int? selectedInstructorId = null)
        {
            var instructors = await _instructorService.GetAllAsync();
            ViewBag.Instructors = new SelectList(instructors, "Id", "Name", selectedInstructorId);
        }
    }
}
