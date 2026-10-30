using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace STDMS.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Department name cannot exceed 50 characters.")]
        public string Name { get; set; } = string.Empty;

        [ValidateNever]
        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
