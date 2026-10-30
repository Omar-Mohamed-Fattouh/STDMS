using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using STDMS.Data;
using STDMS.Models;
using STDMS.Services.Interfaces;

namespace STDMS.Controllers
{
    [Authorize(Roles = RoleNames.Administrator)]
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IInstructorCourseAssignmentService _assignmentService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IInstructorCourseAssignmentService assignmentService,
            ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _assignmentService = assignmentService;
            _logger = logger;
        }

        [HttpGet("Users")]
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users
                .AsNoTracking()
                .Include(user => user.Instructor)
                .OrderBy(user => user.Email)
                .ToListAsync();
            var rows = new List<AdminUserRowViewModel>();

            foreach (var user in users)
            {
                rows.Add(new AdminUserRowViewModel
                {
                    Email = user.Email ?? user.UserName ?? "(no email)",
                    Role = string.Join(", ", await _userManager.GetRolesAsync(user)),
                    InstructorName = user.Instructor?.Name
                });
            }

            return View(new AdminUsersViewModel { Users = rows });
        }

        [HttpGet("Users/Create")]
        public async Task<IActionResult> CreateUser()
        {
            var model = new CreateStaffAccountViewModel();
            await PopulateAccountOptionsAsync(model);
            return View(model);
        }

        [HttpPost("Users/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateStaffAccountViewModel model)
        {
            var isInstructor = model.Role == RoleNames.Instructor;
            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                ModelState.AddModelError(nameof(model.Role), "Select a valid role.");
            }

            if (isInstructor && !model.InstructorId.HasValue)
            {
                ModelState.AddModelError(nameof(model.InstructorId), "Select the instructor profile for this account.");
            }
            else if (!isInstructor && model.InstructorId.HasValue)
            {
                ModelState.AddModelError(nameof(model.InstructorId), "Only instructor accounts can be linked to an instructor profile.");
            }

            if (model.InstructorId is int instructorId)
            {
                if (!await _context.Instructors.AnyAsync(instructor => instructor.Id == instructorId))
                {
                    ModelState.AddModelError(nameof(model.InstructorId), "The selected instructor does not exist.");
                }
                else if (await _userManager.Users.AnyAsync(user => user.InstructorId == instructorId))
                {
                    ModelState.AddModelError(nameof(model.InstructorId), "That instructor already has a login account.");
                }
            }

            if (!ModelState.IsValid)
            {
                await PopulateAccountOptionsAsync(model);
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                InstructorId = model.InstructorId
            };
            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                AddIdentityErrors(createResult.Errors);
                await PopulateAccountOptionsAsync(model);
                return View(model);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
            if (!roleResult.Succeeded)
            {
                var deleteResult = await _userManager.DeleteAsync(user);
                if (!deleteResult.Succeeded)
                {
                    _logger.LogError(
                        "Could not remove partially provisioned account {UserId} after role assignment failed: {Errors}",
                        user.Id,
                        string.Join("; ", deleteResult.Errors.Select(error => error.Code)));
                }

                AddIdentityErrors(roleResult.Errors);
                await PopulateAccountOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Staff account created.";
            return RedirectToAction(nameof(Users));
        }

        [HttpGet("CourseAssignments")]
        public async Task<IActionResult> CourseAssignments()
        {
            var model = new InstructorCourseAssignmentsViewModel
            {
                Assignments = await _assignmentService.GetAllAsync()
            };
            await PopulateAssignmentOptionsAsync(model);
            return View(model);
        }

        [HttpPost("CourseAssignments")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CourseAssignments(InstructorCourseAssignmentsViewModel model)
        {
            var instructorExists = await _context.Instructors
                .AnyAsync(instructor => instructor.Id == model.InstructorId);
            var courseExists = await _context.Courses
                .AnyAsync(course => course.Id == model.CourseId);

            if (!instructorExists || !courseExists)
            {
                ModelState.AddModelError(string.Empty, "Select an existing instructor and course.");
            }
            else if (await _assignmentService.IsAssignedAsync(model.InstructorId, model.CourseId))
            {
                ModelState.AddModelError(string.Empty, "This instructor is already assigned to that course.");
            }

            if (!ModelState.IsValid)
            {
                model.Assignments = await _assignmentService.GetAllAsync();
                await PopulateAssignmentOptionsAsync(model);
                return View(model);
            }

            try
            {
                await _assignmentService.AddAsync(new InstructorCourseAssignment
                {
                    InstructorId = model.InstructorId,
                    CourseId = model.CourseId
                });
            }
            catch (DbUpdateException exception)
            {
                _logger.LogWarning(
                    exception,
                    "Could not create instructor-course assignment for instructor {InstructorId} and course {CourseId}",
                    model.InstructorId,
                    model.CourseId);
                ModelState.AddModelError(string.Empty, "The assignment could not be saved. Refresh the page and try again.");
                model.Assignments = await _assignmentService.GetAllAsync();
                await PopulateAssignmentOptionsAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Course assignment created.";
            return RedirectToAction(nameof(CourseAssignments));
        }

        [HttpPost("CourseAssignments/Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourseAssignment(int instructorId, int courseId)
        {
            try
            {
                await _assignmentService.DeleteAsync(instructorId, courseId);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Course assignment removed.";
            return RedirectToAction(nameof(CourseAssignments));
        }

        private async Task PopulateAccountOptionsAsync(CreateStaffAccountViewModel model)
        {
            model.Roles = new[]
            {
                RoleNames.Administrator,
                RoleNames.AcademicAdministrator,
                RoleNames.Instructor,
                RoleNames.ReadOnlyStaff
            }.Select(role => new SelectListItem(role, role)).ToArray();
            model.Instructors = await _context.Instructors
                .AsNoTracking()
                .OrderBy(instructor => instructor.Name)
                .Select(instructor => new SelectListItem(instructor.Name, instructor.Id.ToString()))
                .ToListAsync();
        }

        private async Task PopulateAssignmentOptionsAsync(InstructorCourseAssignmentsViewModel model)
        {
            model.Instructors = await _context.Instructors
                .AsNoTracking()
                .OrderBy(instructor => instructor.Name)
                .Select(instructor => new SelectListItem(instructor.Name, instructor.Id.ToString()))
                .ToListAsync();
            model.Courses = await _context.Courses
                .AsNoTracking()
                .OrderBy(course => course.Name)
                .Select(course => new SelectListItem(course.Name, course.Id.ToString()))
                .ToListAsync();
        }

        private void AddIdentityErrors(IEnumerable<IdentityError> errors)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
