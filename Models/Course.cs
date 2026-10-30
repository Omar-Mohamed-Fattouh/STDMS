using System.ComponentModel.DataAnnotations;

namespace STDMS.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Course name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(1, 10, ErrorMessage = "Credits must be between 1 and 10.")]
        public int Credits { get; set; }
    }
}
