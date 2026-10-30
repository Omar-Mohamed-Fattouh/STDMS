using Microsoft.EntityFrameworkCore;
using STDMS.Data;
using STDMS.Models;

namespace STDMS.Seed
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Keep the database under EF Core migrations. EnsureCreated bypasses the
            // migration history and can leave a database that migrations cannot upgrade.
            context.Database.Migrate();

            // Seed Departments
            if (!context.Departments.Any())
            {
                context.Departments.AddRange(
                    new Department { Name = "Computer Science" },
                    new Department { Name = "Business" }
                );
                context.SaveChanges();
            }

            // Seed Students
            if (!context.Students.Any())
            {
                context.Students.AddRange(
                    new Student { Name = "Ali Ahmed", Email = "ali@example.com", DateOfBirth = new DateTime(2000, 5, 20), DepartmentId = 1 },
                    new Student { Name = "Sara Mohamed", Email = "sara@example.com", DateOfBirth = new DateTime(1999, 8, 15), DepartmentId = 2 }
                );
                context.SaveChanges();
            }

            // Seed Courses
            if (!context.Courses.Any())
            {
                context.Courses.AddRange(
                    new Course { Name = "Database Systems", Credits = 3 },
                    new Course { Name = "Marketing 101", Credits = 2 }
                );
                context.SaveChanges();
            }

            // Seed Instructors
            if (!context.Instructors.Any())
            {
                context.Instructors.AddRange(
                    new Instructor { Name = "Dr. Amr Youssef", HireDate = new DateTime(2010, 1, 15) },
                    new Instructor { Name = "Dr. Mona Hassan", HireDate = new DateTime(2015, 9, 1) }
                );
                context.SaveChanges();
            }

            // Seed OfficeAssignments
            if (!context.OfficeAssignments.Any())
            {
                context.OfficeAssignments.AddRange(
                    new OfficeAssignment { InstructorId = 1, Location = "Building A - Room 101" },
                    new OfficeAssignment { InstructorId = 2, Location = "Building B - Room 202" }
                );
                context.SaveChanges();
            }

            // Seed Enrollments
            if (!context.Enrollments.Any())
            {
                context.Enrollments.AddRange(
                    new Enrollment { StudentId = 1, CourseId = 1, Grade = "A" },
                    new Enrollment { StudentId = 2, CourseId = 2, Grade = "B" }
                );
                context.SaveChanges();
            }

            // Seed Attendances
            if (!context.Attendances.Any())
            {
                context.Attendances.AddRange(
                    new Attendance { StudentId = 1, CourseId = 1, Date = DateTime.Today.AddDays(-2), IsPresent = true },
                    new Attendance { StudentId = 1, CourseId = 1, Date = DateTime.Today.AddDays(-1), IsPresent = false },
                    new Attendance { StudentId = 2, CourseId = 2, Date = DateTime.Today.AddDays(-2), IsPresent = true }
                );
                context.SaveChanges();
            }
        }
    }
}
