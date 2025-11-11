using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;

namespace mvcLab.Models
{
    public class Instructor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]
        [Display(Name = "Instructor Name")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Remote("IsEmailUnique", "Instructor", AdditionalFields = "Id", 
                ErrorMessage = "This email is already in use")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public required string Address { get; set; }

        [Required(ErrorMessage = "Age is required")]
        [Range(25, 70, ErrorMessage = "Age must be between 25 and 70")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Salary is required")]
        [CustomValidation(typeof(Instructor), nameof(ValidateSalary))]
        [DataType(DataType.Currency)]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Degree is required")]
        [Range(0, 100, ErrorMessage = "Degree must be between 0 and 100")]
        public decimal Degree { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [ForeignKey(nameof(Department))]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }
        public Department? Department { get; set; }

        public ICollection<InstructorCourse> InstructorCourses { get; set; } = new List<InstructorCourse>();

        public static ValidationResult ValidateSalary(decimal salary, ValidationContext context)
        {
            var instructor = (Instructor)context.ObjectInstance;
            var department = instructor.Department?.Name?.ToLower();

            if (department == "software development" && salary < 10000)
            {
                return new ValidationResult("Software Development department requires a minimum salary of 10,000");
            }
            else if (department == "human resources" && (salary < 5000 || salary > 15000))
            {
                return new ValidationResult("HR department salary must be between 5,000 and 15,000");
            }

            return ValidationResult.Success!;
        }
    }
}
