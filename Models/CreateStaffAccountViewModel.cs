using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace STDMS.Models
{
    public class CreateStaffAccountViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        public int? InstructorId { get; set; }

        public IReadOnlyList<SelectListItem> Roles { get; set; } = [];
        public IReadOnlyList<SelectListItem> Instructors { get; set; } = [];
    }
}
