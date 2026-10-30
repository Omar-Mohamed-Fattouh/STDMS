using Microsoft.AspNetCore.Identity;

namespace STDMS.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? InstructorId { get; set; }
        public Instructor? Instructor { get; set; }
    }
}
