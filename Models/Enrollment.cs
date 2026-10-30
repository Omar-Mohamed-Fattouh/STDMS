using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace STDMS.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [StringLength(2, ErrorMessage = "Grade must be 2 characters max.")]
        public string Grade { get; set; } = string.Empty;

        [ValidateNever]
        public Student Student { get; set; } = null!;

        [ValidateNever]
        public Course Course { get; set; } = null!;
    }
}
