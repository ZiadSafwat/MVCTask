using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace mvcLab.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

      
        public int? StudentId { get; set; }
        public Student? Student { get; set; }

        
        public int? InstructorId { get; set; }
        public Instructor? Instructor { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}