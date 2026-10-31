using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using STDMS.Data;
using STDMS.Filters;
using STDMS.Middleware;
using STDMS.Models;
using STDMS.Seed;
using STDMS.Services;
using STDMS.Services.Interfaces;

namespace STDMS
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add MVC controllers with views and register global filters
            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add<ActionLoggingFilter>();
                options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
            });

            // Register filters and middleware dependencies
            builder.Services.AddScoped<ActionLoggingFilter>();

            // Compress HTML/CSS/JS responses (Brotli preferred) — this cuts
            // the self-hosted Bootstrap/FontAwesome payload by ~85% on the wire.
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
                options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
            });
            builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(
                options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
            builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(
                options => options.Level = System.IO.Compression.CompressionLevel.Optimal);

            // Register services
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.Password.RequiredLength = 12;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.User.RequireUniqueEmail = true;
                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.Cookie.HttpOnly = true;
                // SameAsRequest: the cookie is marked Secure automatically once the
                // site runs over HTTPS; forcing Always breaks sign-in entirely on
                // HTTP-only hosting (e.g. MonsterASP free plan before HTTPS activation).
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
            });

            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            builder.Services.AddScoped<IStudentService, StudentService>();
            builder.Services.AddScoped<ICourseService, CourseService>();
            builder.Services.AddScoped<IDepartmentService, DepartmentService>();
            builder.Services.AddScoped<IInstructorService, InstructorService>();
            builder.Services.AddScoped<IAttendanceService, AttendanceService>();
            builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
            builder.Services.AddScoped<IOfficeAssignmentService, OfficeAssignmentService>();
            builder.Services.AddScoped<IInstructorCourseAssignmentService, InstructorCourseAssignmentService>();

            var app = builder.Build();

            // Initialize database
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();
                DbInitializer.Initialize(context);
                await IdentitySeeder.InitializeAsync(
                    services,
                    app.Configuration,
                    services.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeeder"));
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            // Re-execute 404s through the shared NotFound view instead of
            // returning an empty body. Placed before routing/auth so the
            // re-executed request still passes through the full pipeline.
            app.UseStatusCodePagesWithReExecute("/Home/PageNotFound");
            app.UseResponseCompression();
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    // Every stylesheet/script is referenced with asp-append-version,
                    // so static assets can be cached hard: a new build changes the
                    // URL, never a stale file. This matters most on shared hosting,
                    // where repeat visits should barely touch the server.
                    context.Context.Response.Headers.CacheControl = "public,max-age=2592000,immutable";
                }
            });
            app.UseRouting();

            // Add middleware
            app.UseRequestLogging();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
