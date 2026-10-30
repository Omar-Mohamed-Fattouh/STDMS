// Controllers/DepartmentController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using STDMS.Filters;
using STDMS.Models;
using STDMS.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace STDMS.Controllers
{
    //[TypeFilter(typeof(CustomAuthorizationFilter), Arguments = new object[] { "Admin" })]
    [Authorize(Roles = RoleNames.SchoolRecords)]
    [Route("Department")]
    public class DepartmentController : Controller
    {
        private readonly IDepartmentService _service;
        private readonly ILogger<DepartmentController> _logger;

        public DepartmentController(IDepartmentService service, ILogger<DepartmentController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Getting all departments");
            var depts = await _service.GetAllAsync();
            return View(depts);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var dept = await _service.GetByIdAsync(id);
            if (dept == null)
            {
                _logger.LogWarning("Department not found with id {Id}", id);
                return NotFound();
            }
            return View(dept);
        }

        [HttpGet("Create")]
        [Authorize(Roles = RoleNames.Administrators)]
        public IActionResult Create() => View();

        [HttpPost("Create")]
        [Authorize(Roles = RoleNames.Administrators)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department dept)
        {
            if (!ModelState.IsValid)
            {
                return View(dept);
            }

            try
            {
                // No longer assigns a result, just executes.
                await _service.AddAsync(dept);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Catches any exception from the service, logs it, and shows an error to the user.
                _logger.LogError(ex, "An error occurred while creating department.");
                ModelState.AddModelError("", "Something went wrong while saving. Please try again.");
                return View(dept);
            }
        }

        [HttpGet("Edit/{id}")]
        [Authorize(Roles = RoleNames.Administrators)]
        public async Task<IActionResult> Edit(int id)
        {
            var dept = await _service.GetByIdAsync(id);
            if (dept == null)
            {
                return NotFound();
            }
            return View(dept);
        }

        [HttpPost("Edit")]
        [Authorize(Roles = RoleNames.Administrators)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Department dept)
        {
            if (!ModelState.IsValid)
            {
                return View(dept);
            }

            try
            {
                await _service.UpdateAsync(dept);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException ex)
            {
                // This specific exception means the department to update didn't exist.
                _logger.LogWarning(ex, "Update failed: Department with ID {DepartmentId} not found.", dept.Id);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating department {DepartmentId}.", dept.Id);
                ModelState.AddModelError("", "Something went wrong while updating. Please try again.");
                return View(dept);
            }
        }

        [HttpGet("Delete/{id}")]
        [Authorize(Roles = RoleNames.Administrators)]
        public async Task<IActionResult> Delete(int id)
        {
            var dept = await _service.GetByIdAsync(id);
            if (dept == null)
            {
                return NotFound();
            }
            return View(dept);
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
            catch (KeyNotFoundException ex)
            {
                // The service throws this if the department to delete doesn't exist.
                _logger.LogWarning(ex, "Delete failed: Department with ID {DepartmentId} not found.", id);
                return NotFound();
            }
            catch (Exception ex)
            {
                // For other errors (e.g., database connection issues)
                _logger.LogError(ex, "An error occurred while deleting department {DepartmentId}.", id);
                // You could redirect to an error page or return a bad request.
                // For a user-facing view, it's often better to show the view again with an error.
                var dept = await _service.GetByIdAsync(id); // Re-fetch to display the page again
                if (dept != null)
                {
                    ModelState.AddModelError("", "The department could not be deleted. Please try again.");
                    return View("Delete", dept);
                }
                return BadRequest("Delete failed due to an unexpected error.");
            }
        }
    }
}
