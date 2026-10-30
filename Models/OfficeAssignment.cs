using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace STDMS.Models
{
    public class OfficeAssignment
    {
        [Key]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Select an instructor.")]
        [Display(Name = "Instructor")]
        public int InstructorId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters.")]
        public string Location { get; set; } = string.Empty;

        [ValidateNever]
        public Instructor Instructor { get; set; } = null!;
    }
}
