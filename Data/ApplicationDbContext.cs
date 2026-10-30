using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using STDMS.Models;

namespace STDMS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        
        public DbSet<Student> Students { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<OfficeAssignment> OfficeAssignments { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<InstructorCourseAssignment> InstructorCourseAssignments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // one to many Dept -> stds
            modelBuilder.Entity<Department>()
                .HasMany(d => d.Students)
                .WithOne(s => s.Department)
                .HasForeignKey(s => s.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // one to many std -> enrolls
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentId);

            // one to many crs -> enrolls
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany()
                .HasForeignKey(s => s.CourseId);

            // one to one: Ins -> office
            modelBuilder.Entity<OfficeAssignment>()
                .HasOne(o => o.Instructor)
                .WithOne()
                .HasForeignKey<OfficeAssignment>(o => o.InstructorId);

            // Std + Crs many to one 
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentId);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Course)
                .WithMany()
                .HasForeignKey(a => a.CourseId);

            modelBuilder.Entity<InstructorCourseAssignment>()
                .HasKey(assignment => new { assignment.InstructorId, assignment.CourseId });

            modelBuilder.Entity<InstructorCourseAssignment>()
                .HasOne(assignment => assignment.Instructor)
                .WithMany()
                .HasForeignKey(assignment => assignment.InstructorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InstructorCourseAssignment>()
                .HasOne(assignment => assignment.Course)
                .WithMany()
                .HasForeignKey(assignment => assignment.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(user => user.Instructor)
                .WithOne()
                .HasForeignKey<ApplicationUser>(user => user.InstructorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(user => user.InstructorId)
                .IsUnique()
                .HasFilter("[InstructorId] IS NOT NULL");
        }
    }
}
