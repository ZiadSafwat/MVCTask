using System.ComponentModel.DataAnnotations.Schema;

namespace mvcLab.Models
{
    public class StudCourse
    {
        public int Id { get; set; }

        public double Grade { get; set; }

        [ForeignKey(nameof(Student))]
        public int? StudId { get; set; }
        public Student? Student { get; set; }

        [ForeignKey(nameof(Course))]
        public int? CourseId { get; set; }
        public Course? Course { get; set; }

    }
}
