using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using STDMS.Data;
using STDMS.Models;

namespace STDMS.Controllers
{
    [Authorize(Roles = RoleNames.AttendanceUsers)]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // Anonymous visitors get the public welcome page; signed-in staff
            // continue straight to the dashboard below.
            if (User.Identity?.IsAuthenticated != true)
            {
                return View("Welcome");
            }

            if (User.IsInRole(RoleNames.Instructor))
            {
                return RedirectToAction("Index", "Attendance");
            }

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var model = new HomeDashboardViewModel
            {
                TotalStudents = await _context.Students.CountAsync(),
                TotalInstructors = await _context.Instructors.CountAsync(),
                TotalCourses = await _context.Courses.CountAsync(),
                TotalDepartments = await _context.Departments.CountAsync(),
                TotalEnrollments = await _context.Enrollments.CountAsync(),
                AttendanceToday = await _context.Attendances
                    .CountAsync(attendance => attendance.Date >= today && attendance.Date < tomorrow),
                PresentToday = await _context.Attendances
                    .CountAsync(attendance => attendance.Date >= today
                        && attendance.Date < tomorrow
                        && attendance.IsPresent),
                RecentStudents = await _context.Students
                    .AsNoTracking()
                    .Include(student => student.Department)
                    .OrderByDescending(student => student.Id)
                    .Take(5)
                    .ToListAsync(),
                RecentEnrollments = await _context.Enrollments
                    .AsNoTracking()
                    .Include(enrollment => enrollment.Student)
                    .Include(enrollment => enrollment.Course)
                    .OrderByDescending(enrollment => enrollment.Id)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        // Target for UseStatusCodePagesWithReExecute: serves a designed
        // error page for any bare status code (404 -> NotFound view,
        // 403 -> AccessDenied view, anything else -> generic Error view),
        // preserving the original status code. Before this wiring existed,
        // every such response returned an empty body.
        // AllowAnonymous: without it the fallback policy bounces anonymous
        // 404s (including missing static files) to the login page instead of
        // showing the designed NotFound page with its real 404 status.
        [AllowAnonymous]
        [Route("Home/PageNotFound")]
        public IActionResult PageNotFound()
        {
            var feature = HttpContext.Features
                .Get<Microsoft.AspNetCore.Diagnostics.IStatusCodeReExecuteFeature>();
            var statusCode = feature?.OriginalStatusCode ?? StatusCodes.Status404NotFound;
            Response.StatusCode = statusCode;

            return statusCode switch
            {
                StatusCodes.Status404NotFound => View("NotFound"),
                StatusCodes.Status403Forbidden => View("~/Views/Account/AccessDenied.cshtml"),
                _ => StatusError(statusCode)
            };
        }

        private IActionResult StatusError(int statusCode)
        {
            ViewData["StatusCode"] = statusCode;
            return View("Error", new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
