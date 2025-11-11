using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;

namespace mvcLab.Models
{
    public class Course
    {
        [Key]
        public int Num { get; set; }

        [Required(ErrorMessage = "Course name is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Course name must be between 3 and 200 characters")]
        [Display(Name = "Course Name")]
        [Remote("IsNameUnique", "Course", AdditionalFields = "DepartmentId,Num", 
                ErrorMessage = "This course name already exists in the selected department")]
        public required string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Course Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Total degree is required")]
        [Range(100, 120, ErrorMessage = "Total degree must be between 100 and 120")]
        [Display(Name = "Total Degree")]
        public decimal Degree { get; set; }

        [Required(ErrorMessage = "Minimum degree is required")]
        [Range(50, 60, ErrorMessage = "Minimum degree must be between 50 and 60")]
        [Display(Name = "Minimum Degree")]
        public decimal MinDegree { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [ForeignKey(nameof(Department))]
        public int DepartmentId { get; set; }
        public Department? Department { get; set; }

        public ICollection<StudCourse> StudCourses { get; set; } = new List<StudCourse>();
        public ICollection<InstructorCourse> InstructorCourses { get; set; } = new List<InstructorCourse>();
    }
}
