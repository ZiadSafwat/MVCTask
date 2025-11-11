using System.ComponentModel.DataAnnotations.Schema;

namespace mvcLab.Models
{
    public class InstructorCourse
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Course))]
        public int? CourseId { get; set; }
        public Course? Course { get; set; }

        [ForeignKey(nameof(Instructor))]
        public int? InstructorId { get; set; }
        public Instructor? Instructor { get; set; }
    }
}
