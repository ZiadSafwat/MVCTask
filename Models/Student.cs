using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;

namespace mvcLab.Models
{
    public class Student
    {
        [Key] 
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int SSN { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can only contain letters and spaces")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Remote("IsStudentEmailUnique", "Student", AdditionalFields = "SSN", 
                ErrorMessage = "This email is already registered")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 200 characters")]
        public required string Address { get; set; }

        public string? Image { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [RegularExpression("^(Male|Female)$", ErrorMessage = "Gender must be either Male or Female")]
        public required string Gender { get; set; }

        [Required(ErrorMessage = "Age is required")]
        [Range(16, 100, ErrorMessage = "Age must be between 16 and 100")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [ForeignKey(nameof(Department))]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }
        public Department? Department { get; set; }

        public ICollection<StudCourse> StudCourses { get; set; } = new List<StudCourse>();

        public string GetGradeColor(double grade)
        {
            return grade >= 90 ? "success" : 
                   grade >= 80 ? "info" : 
                   grade >= 70 ? "warning" : "danger";
        }

        public string GetGradeText(double grade)
        {
            return grade >= 90 ? "A" : 
                   grade >= 80 ? "B" : 
                   grade >= 70 ? "C" : 
                   grade >= 60 ? "D" : "F";
        }
    }
}